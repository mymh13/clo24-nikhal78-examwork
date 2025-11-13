@description('Name of the App Service')
param appServiceName string

@description('Location for all resources')
param location string = 'swedencentral'

@description('App Service plan SKU - Basic B1 tier for dev')
param appServicePlanSku string = 'B1'

// App Service Plan - minimal
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '${appServiceName}-plan'
  location: location
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: appServicePlanSku
    tier: appServicePlanSku == 'F1' ? 'Free' : (appServicePlanSku == 'B1' ? 'Basic' : 'Standard')
  }
}

// App Service - minimal: Force .NET 8 runtime
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNET|8.0'
      appCommandLine: 'dotnet Ticketing.Web.dll'
      appSettings: [
        {
          name: 'WEBSITE_STACK'
          value: 'DOTNET'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
      ]
    }
  }
}

// Output the default URL
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output appServiceName string = appService.name

