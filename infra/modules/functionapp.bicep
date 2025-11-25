@description('Name of the Function App (must be globally unique, 2-60 characters, alphanumeric and hyphens only)')
param functionAppName string

@description('Location for all resources')
param location string = 'swedencentral'

@description('Application Insights connection string')
param appInsightsConnectionString string = ''

@description('Name of the Key Vault for secret references')
param keyVaultName string = ''

@description('Service Bus namespace name')
param serviceBusNamespaceName string = ''

@description('Cosmos DB account name')
param cosmosAccountName string = ''

@description('Storage account name for Function App (must be globally unique, 3-24 characters, alphanumeric only)')
param storageAccountName string

@description('Function App Plan SKU - Y1 for Consumption (serverless), B1 for Basic (dev when Y1 not available)')
param functionAppPlanSku string = 'Y1'

// Storage Account for Function App (required for Azure Functions)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

// Get storage account keys
var storageAccountKeys = storageAccount.listKeys()
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccountKeys.keys[0].value}'

// Function App Plan
// Use Y1 (Consumption) for serverless, or B1 (Basic) when Y1 not available in resource group
resource functionAppPlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '${functionAppName}-plan'
  location: location
  kind: 'functionapp,linux'
  sku: functionAppPlanSku == 'Y1' ? {
    name: 'Y1'  // Consumption plan
    tier: 'Dynamic'
  } : {
    name: 'B1'  // Basic plan (fallback when Y1 not available)
    tier: 'Basic'
  }
  properties: {
    reserved: true  // Linux
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'  // Enable managed identity for Service Bus and Cosmos DB access
  }
  properties: {
    serverFarmId: functionAppPlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageAccountConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'KeyVault__Name'
          value: keyVaultName
        }
        {
          name: 'ServiceBus__NamespaceName'
          value: serviceBusNamespaceName
        }
        {
          name: 'CosmosDb__AccountName'
          value: cosmosAccountName
        }
        {
          name: 'AzureWebJobsServiceBus'
          value: 'Endpoint=sb://${serviceBusNamespaceName}.servicebus.windows.net/;Authentication=ManagedIdentity'
        }
      ]
      http20Enabled: true
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

// Grant Function App managed identity access to Service Bus
// Get reference to Service Bus namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

// Grant Function App "Azure Service Bus Data Receiver" role (read from queue)
// Note: Using functionApp.name in guid since identity.principalId is not available at deployment start
resource functionAppServiceBusReceiverRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, functionApp.name, 'Azure Service Bus Data Receiver')
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0') // Azure Service Bus Data Receiver
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Function App access to Cosmos DB
// Get reference to Cosmos DB account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-09-15' existing = {
  name: cosmosAccountName
}

// Grant Function App "DocumentDB Account Contributor" role for Cosmos DB access
// Note: Cosmos DB data access typically uses connection strings, but this role provides account-level access
// Using functionApp.name in guid since identity.principalId is not available at deployment start
resource functionAppCosmosDbRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cosmosAccount.id, functionApp.name, 'DocumentDB Account Contributor')
  scope: cosmosAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5bd9cd88-fe45-4216-938b-f97437e15450') // DocumentDB Account Contributor
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output functionAppName string = functionApp.name
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'
output managedIdentityPrincipalId string = functionApp.identity.principalId
output storageAccountName string = storageAccount.name

