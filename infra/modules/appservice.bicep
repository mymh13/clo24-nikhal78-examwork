@description('Name of the App Service')
param appServiceName string

@description('Location for all resources')
param location string = 'swedencentral'

@description('App Service plan SKU - Basic B1 tier for dev')
param appServicePlanSku string = 'B1'

@description('Application Insights connection string')
param appInsightsConnectionString string = ''

@description('Key Vault name for secret references')
param keyVaultName string = ''

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

// App Service - using Docker container from GHCR
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'  // Enable managed identity for Key Vault access
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|ghcr.io/mymh13/clo24-nikhal78-examwork/web:latest'
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://ghcr.io'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: 'mymh13'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: ''
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'KeyVault:Name'
          value: keyVaultName
        }
      ]
    }
  }
}

// Output the default URL
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output appServiceName string = appService.name

