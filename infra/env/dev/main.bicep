@description('Name of the App Service')
param appServiceName string = 'examwork-web-dev'

@description('Name of the Cosmos DB account')
param cosmosAccountName string = 'examwork-cosmos-dev'

@description('Location for all resources')
param location string = 'swedencentral'

@description('App Service plan SKU - Basic B1 tier for dev')
param appServicePlanSku string = 'B1'

// Deploy App Service using the module
module appService '../../modules/appservice.bicep' = {
  name: 'appService-deployment'
  params: {
    appServiceName: appServiceName
    location: location
    appServicePlanSku: appServicePlanSku
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

// Outputs
output appServiceUrl string = appService.outputs.appServiceUrl
output appServiceName string = appService.outputs.appServiceName
output cosmosAccountName string = cosmosDb.outputs.cosmosAccountName
output cosmosEndpoint string = cosmosDb.outputs.cosmosEndpoint
output databaseName string = cosmosDb.outputs.databaseName
output bookingsContainerName string = cosmosDb.outputs.bookingsContainerName

