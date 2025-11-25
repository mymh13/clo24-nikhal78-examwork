@description('Name of the App Service')
param appServiceName string = 'examwork-web-dev'

@description('Name of the Cosmos DB account')
param cosmosAccountName string = 'examwork-cosmos-dev'

@description('Name of the Application Insights resource')
param appInsightsName string = 'examwork-insights-dev'

@description('Name of the Key Vault resource')
param keyVaultName string = 'examwork-kv-dev'

@description('Name of the App Configuration resource')
param appConfigName string = 'examwork-appconfig-dev'

@description('Name of the Service Bus namespace')
param serviceBusNamespaceName string = 'examwork-sb-dev'

@description('Location for all resources')
param location string = 'swedencentral'

@description('App Service plan SKU - Basic B1 tier for dev')
param appServicePlanSku string = 'B1'

// Deploy Application Insights using the module (must be before App Service)
module appInsights '../../modules/applicationinsights.bicep' = {
  name: 'appInsights-deployment'
  params: {
    appInsightsName: appInsightsName
    location: location
  }
}

// Deploy Cosmos DB using the module
module cosmosDb '../../modules/cosmosdb.bicep' = {
  name: 'cosmosDb-deployment'
  params: {
    cosmosAccountName: cosmosAccountName
    location: location
  }
}

// Deploy Key Vault using the module (must be before App Configuration)
module keyVault '../../modules/keyvault.bicep' = {
  name: 'keyVault-deployment'
  params: {
    keyVaultName: keyVaultName
    location: location
  }
}

// Deploy App Service using the module (must be before App Configuration for managed identity)
module appService '../../modules/appservice.bicep' = {
  name: 'appService-deployment'
  params: {
    appServiceName: appServiceName
    location: location
    appServicePlanSku: appServicePlanSku
    appInsightsConnectionString: appInsights.outputs.connectionString
    keyVaultName: keyVaultName
  }
}

// Deploy App Configuration using the module
module appConfiguration '../../modules/appconfiguration.bicep' = {
  name: 'appConfiguration-deployment'
  params: {
    appConfigName: appConfigName
    location: location
    keyVaultName: keyVaultName
    appServicePrincipalId: appService.outputs.managedIdentityPrincipalId
    sku: 'Free'  // Free tier for dev environment
  }
  dependsOn: [
    keyVault
  ]
}

// Deploy Service Bus using the module
module serviceBus '../../modules/servicebus.bicep' = {
  name: 'serviceBus-deployment'
  params: {
    serviceBusNamespaceName: serviceBusNamespaceName
    location: location
    keyVaultName: keyVaultName
    appServicePrincipalId: appService.outputs.managedIdentityPrincipalId
    sku: 'Basic'  // Basic tier for dev environment
    bookingEventsQueueName: 'booking-events'
  }
  dependsOn: [
    keyVault
  ]
}

// Outputs
output appServiceUrl string = appService.outputs.appServiceUrl
output appServiceName string = appService.outputs.appServiceName
output cosmosAccountName string = cosmosDb.outputs.cosmosAccountName
output cosmosEndpoint string = cosmosDb.outputs.cosmosEndpoint
output databaseName string = cosmosDb.outputs.databaseName
output bookingsContainerName string = cosmosDb.outputs.bookingsContainerName
output appInsightsName string = appInsights.outputs.appInsightsName
output appInsightsConnectionString string = appInsights.outputs.connectionString
output keyVaultName string = keyVault.outputs.keyVaultName
output keyVaultUri string = keyVault.outputs.keyVaultUri
output appConfigName string = appConfiguration.outputs.appConfigName
output appConfigEndpoint string = appConfiguration.outputs.appConfigEndpoint
output appConfigEndpointSecretName string = appConfiguration.outputs.endpointSecretName
output serviceBusNamespaceName string = serviceBus.outputs.serviceBusNamespaceName
output serviceBusEndpoint string = serviceBus.outputs.serviceBusEndpoint
output bookingEventsQueueName string = serviceBus.outputs.bookingEventsQueueName

