// ---------- parameters ----------
@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Globally unique Azure Container Registry name. Use only alphanumeric characters.')
param containerRegistryName string = 'appapi${uniqueString(resourceGroup().id)}'

@description('Name of the Azure Container Apps environment.')
param containerAppsEnvironmentName string = 'app-api-env'

@description('Name of the PostgreSQL flexible server.')
param dbServerName string = '${containerAppsEnvironmentName}-db'

@description('Administrator username for the PostgreSQL flexible server.')
param dbAdminUsername string = 'appadmin'

@secure()
@description('Administrator password for the PostgreSQL flexible server.')
param dbAdminPassword string

@description('Name of the application database to create.')
param databaseName string = 'app'

@description('Name of the Key Vault used to store secrets consumed by the app.')
param keyVaultName string = '${containerAppsEnvironmentName}-kv'

@description('Name of the Azure Storage account used for document storage.')
param objectStorageAccountName string = 'obj${uniqueString(resourceGroup().id)}'

@description('Name of the blob container used for documents.')
param objectStorageContainerName string = 'documents'

// ---------- logging ----------
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${containerAppsEnvironmentName}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// ---------- container registry + identity for pulling images ----------
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${containerAppsEnvironmentName}-identity'
  location: location
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, containerAppIdentity.id, 'AcrPull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '7f951dda-4ed3-4680-a7ca-43fe172d538d'
    )
    principalId: containerAppIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---------- container apps environment ----------
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppsEnvironmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

// ---------- postgres flexible server (application database) ----------
// cheapest burstable tier + minimum storage to keep cost low
resource dbServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: dbServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: dbAdminUsername
    administratorLoginPassword: dbAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource dbFirewallAllowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: dbServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: dbServer
  name: databaseName
}

// ---------- key vault (secrets consumed by the app via managed identity) ----------
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
  }
}

resource keyVaultSecretsUserAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, containerAppIdentity.id, 'KeyVaultSecretsUser')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    )
    principalId: containerAppIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource databaseConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'database-connection-string'
  properties: {
    value: 'Host=${dbServer.properties.fullyQualifiedDomainName};Database=${databaseName};Username=${dbAdminUsername};Password=${dbAdminPassword};Ssl Mode=Require;Trust Server Certificate=true'
  }
}

resource objectStorageAccountKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'object-storage-account-key'
  properties: {
    value: objectStorageAccount.listKeys().keys[0].value
  }
}

resource objectStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: objectStorageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource objectStorageBlobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' existing = {
  parent: objectStorageAccount
  name: 'default'
}

resource objectStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: objectStorageBlobServices
  name: objectStorageContainerName
  properties: {
    publicAccess: 'None'
  }
}

// ---------- outputs ----------
output containerRegistryName string = containerRegistry.name
output containerRegistryLoginServer string = containerRegistry.properties.loginServer
output containerAppsEnvironmentName string = containerAppsEnvironment.name
output containerAppIdentityResourceId string = containerAppIdentity.id
output dbServerFqdn string = dbServer.properties.fullyQualifiedDomainName
output dbAdminUsername string = dbAdminUsername
output databaseName string = database.name
output keyVaultName string = keyVault.name
output databaseConnectionStringSecretUri string = databaseConnectionStringSecret.properties.secretUri
output objectStorageAccountName string = objectStorageAccount.name
output objectStorageAccountKeySecretUri string = objectStorageAccountKeySecret.properties.secretUri
output objectStorageContainerName string = objectStorageContainer.name
output objectStorageServiceUri string = objectStorageAccount.properties.primaryEndpoints.blob
output objectStoragePublicServiceUri string = objectStorageAccount.properties.primaryEndpoints.blob
