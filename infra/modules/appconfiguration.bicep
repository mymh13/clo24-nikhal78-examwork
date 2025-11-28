@description('Name of the App Configuration resource (must be globally unique, 5-50 characters, alphanumeric, hyphens, and underscores only)')
param appConfigName string

@description('Location for all resources')
param location string = 'swedencentral'

@description('SKU for App Configuration - Free tier for dev, Standard for production')
param sku string = 'Free'

@description('Name of the Key Vault to store connection string')
param keyVaultName string

@description('Principal ID of the App Service managed identity (for RBAC access)')
param appServicePrincipalId string = ''

// App Configuration
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigName
  location: location
  sku: {
    name: sku
  }
  properties: {
    enablePurgeProtection: false  // Set to true for production if needed
  }
}

// Get reference to Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Get the connection string from App Configuration
// Note: Connection string is available via listKeys operation, but we'll store the endpoint
// and let the application construct the connection string using managed identity instead
resource appConfigEndpoint 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AppConfiguration--Endpoint'
  properties: {
    value: appConfiguration.properties.endpoint
    contentType: 'App Configuration Endpoint URL'
  }
}

// Store App Configuration name for reference
resource appConfigNameSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AppConfiguration--Name'
  properties: {
    value: appConfiguration.name
    contentType: 'App Configuration Resource Name'
  }
}

// Grant App Service managed identity access to App Configuration (if principal ID provided)
// Using subscriptionResourceId for built-in role: App Configuration Data Reader (for reading)
// Note: Role definition ID verified: 516239f1-63e1-4d78-a4de-a74fb236a071
resource appConfigDataReaderRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(appServicePrincipalId)) {
  name: guid(appConfiguration.id, appServicePrincipalId, 'App Configuration Data Reader')
  scope: appConfiguration
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '516239f1-63e1-4d78-a4de-a74fb236a071') // App Configuration Data Reader
    principalId: appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant App Service managed identity write access to App Configuration (for feature flag toggling)
// Using subscriptionResourceId for built-in role: App Configuration Data Owner
// Note: Role definition ID: 5ae67dd6-50cb-40e7-96ac-d0fae5166711
resource appConfigDataOwnerRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(appServicePrincipalId)) {
  name: guid(appConfiguration.id, appServicePrincipalId, 'App Configuration Data Owner')
  scope: appConfiguration
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5ae67dd6-50cb-40e7-96ac-d0fae5166711') // App Configuration Data Owner
    principalId: appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output appConfigName string = appConfiguration.name
output appConfigEndpoint string = appConfiguration.properties.endpoint
output endpointSecretName string = 'AppConfiguration--Endpoint'
output nameSecretName string = 'AppConfiguration--Name'

