targetScope = 'subscription'

// ---------- parameters ----------
@description('Name of the resource group to create/use for all app resources.')
param resourceGroupName string = 'nextjs'

@description('Azure region for the resource group and all resources.')
param location string = 'uksouth'

@description('Globally unique Azure Container Registry name. Use only alphanumeric characters.')
// matches the resourceGroup().id-based default used in foundation.bicep/main.bicep so both resolve to the same name
param containerRegistryName string = 'appapi${uniqueString('/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroupName}')}'

@description('Name of the Azure Container Apps environment.')
param containerAppsEnvironmentName string = 'app-api-env-${uniqueString('/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroupName}')}'

@description('Name of the PostgreSQL flexible server.')
param dbServerName string = 'appdb-${uniqueString('/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroupName}')}'

@description('Name of the Key Vault used to store secrets consumed by the app.')
param keyVaultName string = 'appkv${uniqueString('/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroupName}')}'

@description('Administrator username for the PostgreSQL flexible server.')
param dbAdminUsername string = 'appadmin'

@secure()
@description('Administrator password for the PostgreSQL flexible server.')
param dbAdminPassword string

@description('Name of the application database to create.')
param databaseName string = 'app'

@description('Name of the blob container used for documents.')
param objectStorageContainerName string = 'documents'

// ---------- resource group (created here so the whole stack is a single subscription-level deployment) ----------
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

// ---------- foundation module (registry, identity, environment, db, key vault, storage) ----------
module foundation 'foundation.bicep' = {
  name: 'foundation'
  scope: rg
  params: {
    location: location
    containerRegistryName: containerRegistryName
    containerAppsEnvironmentName: containerAppsEnvironmentName
    dbServerName: dbServerName
    dbAdminUsername: dbAdminUsername
    dbAdminPassword: dbAdminPassword
    databaseName: databaseName
    keyVaultName: keyVaultName
    objectStorageContainerName: objectStorageContainerName
  }
}

// ---------- outputs ----------
output resourceGroupName string = rg.name
output containerRegistryName string = foundation.outputs.containerRegistryName
output containerRegistryLoginServer string = foundation.outputs.containerRegistryLoginServer
output containerAppsEnvironmentName string = foundation.outputs.containerAppsEnvironmentName
output containerAppIdentityResourceId string = foundation.outputs.containerAppIdentityResourceId
output dbServerFqdn string = foundation.outputs.dbServerFqdn
output dbAdminUsername string = foundation.outputs.dbAdminUsername
output databaseName string = foundation.outputs.databaseName
output keyVaultName string = foundation.outputs.keyVaultName
output databaseConnectionStringSecretUri string = foundation.outputs.databaseConnectionStringSecretUri
output objectStorageAccountName string = foundation.outputs.objectStorageAccountName
output objectStorageAccountKeySecretUri string = foundation.outputs.objectStorageAccountKeySecretUri
output objectStorageContainerName string = foundation.outputs.objectStorageContainerName
output objectStorageServiceUri string = foundation.outputs.objectStorageServiceUri
output objectStoragePublicServiceUri string = foundation.outputs.objectStoragePublicServiceUri
