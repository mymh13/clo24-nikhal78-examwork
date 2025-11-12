@description('Name of the App Service')
param appServiceName string = 'examwork-web-dev'

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

// Output the App Service URL
output appServiceUrl string = appService.outputs.appServiceUrl
output appServiceName string = appService.outputs.appServiceName

