@description('Name of the Key Vault resource (must be globally unique, 3-24 characters, alphanumeric and hyphens only)')
param keyVaultName string

@description('Location for all resources')
param location string = 'swedencentral'

@description('Enable soft delete (recommended for data protection)')
param enableSoftDelete bool = true

@description('Soft delete retention period in days (7-90)')
param softDeleteRetentionInDays int = 7

@description('Enable purge protection (prevents immediate deletion, recommended for production)')
param enablePurgeProtection bool = false

// Key Vault properties - conditionally include purge protection
var baseProperties = {
  tenantId: subscription().tenantId
  sku: {
    name: 'standard'
    family: 'A'
  }
  enabledForDeployment: false
  enabledForTemplateDeployment: false
  enabledForDiskEncryption: false
  enableSoftDelete: enableSoftDelete
  softDeleteRetentionInDays: softDeleteRetentionInDays
  accessPolicies: []
}

var purgeProtectionProperty = enablePurgeProtection ? {
  enablePurgeProtection: true
} : {}

var keyVaultProperties = union(baseProperties, purgeProtectionProperty)

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    ...keyVaultProperties
    enableRbacAuthorization: true  // Use RBAC instead of access policies
  }
}

// Output the vault name and URI
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri

