@description('Name of the Service Bus namespace (must be globally unique, 6-50 characters, alphanumeric and hyphens only)')
param serviceBusNamespaceName string

@description('Location for all resources')
param location string = 'swedencentral'

@description('SKU for Service Bus - Basic tier for dev, Standard for production')
param sku string = 'Basic'

@description('Name of the Key Vault to store connection string')
param keyVaultName string

@description('Name of the queue for booking events')
param bookingEventsQueueName string = 'booking-events'

@description('Principal ID of the App Service managed identity (for RBAC access)')
param appServicePrincipalId string = ''

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: sku
    tier: sku == 'Basic' ? 'Basic' : 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

// Get reference to Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Get connection string from Service Bus (requires listKeys operation)
// Note: We'll store the namespace endpoint and let the application use managed identity
resource serviceBusEndpoint 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ServiceBus--Endpoint'
  properties: {
    value: serviceBusNamespace.properties.serviceBusEndpoint
    contentType: 'Service Bus Namespace Endpoint'
  }
}

// Store Service Bus namespace name for reference
resource serviceBusNameSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ServiceBus--NamespaceName'
  properties: {
    value: serviceBusNamespace.name
    contentType: 'Service Bus Namespace Name'
  }
}

// Create queue for booking events
resource bookingEventsQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: bookingEventsQueueName
  properties: {
    maxSizeInMegabytes: sku == 'Basic' ? 1024 : 5120
    defaultMessageTimeToLive: 'P14D'  // 14 days
    lockDuration: 'PT1M'  // 1 minute
    requiresDuplicateDetection: false
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 10
    enableBatchedOperations: true
  }
}

// Grant App Service managed identity access to Service Bus (if principal ID provided)
// Using full resource ID for built-in role: Azure Service Bus Data Owner
resource serviceBusDataOwnerRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(appServicePrincipalId)) {
  name: guid(serviceBusNamespace.id, appServicePrincipalId, 'Azure Service Bus Data Owner')
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: '/providers/Microsoft.Authorization/roleDefinitions/090c5cfd-751d-490a-894a-3ce6f1109419' // Azure Service Bus Data Owner
    principalId: appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output serviceBusNamespaceName string = serviceBusNamespace.name
output serviceBusEndpoint string = serviceBusNamespace.properties.serviceBusEndpoint
output bookingEventsQueueName string = bookingEventsQueue.name
output endpointSecretName string = 'ServiceBus--Endpoint'
output namespaceNameSecretName string = 'ServiceBus--NamespaceName'

