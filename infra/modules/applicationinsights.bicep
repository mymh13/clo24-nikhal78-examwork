@description('Name of the Application Insights resource')
param appInsightsName string

@description('Location for all resources')
param location string = 'swedencentral'

@description('Application type - Web for ASP.NET/Blazor')
param applicationType string = 'web'

// Application Insights component
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: applicationType
  properties: {
    Application_Type: applicationType
    Request_Source: 'rest'
    IngestionMode: 'ApplicationInsights'
  }
}

// Output the connection string and instrumentation key
output appInsightsName string = appInsights.name
output connectionString string = appInsights.properties.ConnectionString
output instrumentationKey string = appInsights.properties.InstrumentationKey

