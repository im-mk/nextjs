// ---------- parameters ----------
@description('Globally unique Azure Container Registry name. Use only alphanumeric characters.')
param containerRegistryName string = 'appapi${uniqueString(resourceGroup().id)}'

@description('Name of the existing Azure Container Apps environment.')
param containerAppsEnvironmentName string = 'app-api-env'

@description('Resource ID of the managed identity authorized to pull images from the registry.')
param containerAppIdentityResourceId string

@description('Name of the Azure Container App.')
param containerAppName string = 'app-api'

@description('Container image to run. Deploy an image built by the CI workflow after provisioning.')
param image string

@description('Name of the frontend Azure Container App.')
param webAppName string = 'app-web'

@description('Frontend container image to run. Leave empty to deploy the API only.')
param webImage string = ''

@secure()
@description('Key Vault secret URI (from foundation.bicep output) holding the Npgsql connection string.')
param databaseConnectionStringSecretUri string

@description('S3-compatible service endpoint used by the API.')
param objectStorageServiceUrl string = ''

@description('Public S3-compatible service endpoint used when generating browser-facing URLs.')
param objectStoragePublicServiceUrl string = ''

@secure()
@description('Key Vault secret URI (from foundation.bicep output) holding the Garage access key.')
param objectStorageAccessKeySecretUri string

@secure()
@description('Key Vault secret URI (from foundation.bicep output) holding the Garage secret key.')
param objectStorageSecretKeySecretUri string

@description('Bucket name for documents.')
param objectStorageBucket string = ''

// ---------- existing resources created by foundation.bicep ----------
var containerRegistryLoginServer = '${containerRegistryName}.azurecr.io'
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppsEnvironmentName
}

// ---------- the api container app ----------
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentityResourceId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistryLoginServer
          identity: containerAppIdentityResourceId
        }
      ]
      secrets: [
        {
          name: 'database-connection-string'
          keyVaultUrl: databaseConnectionStringSecretUri
          identity: containerAppIdentityResourceId
        }
        {
          name: 'object-storage-access-key'
          keyVaultUrl: objectStorageAccessKeySecretUri
          identity: containerAppIdentityResourceId
        }
        {
          name: 'object-storage-secret-key'
          keyVaultUrl: objectStorageSecretKeySecretUri
          identity: containerAppIdentityResourceId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: image
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'database-connection-string'
            }
            {
              name: 'ObjectStorage__ServiceUrl'
              value: objectStorageServiceUrl
            }
            {
              name: 'ObjectStorage__PublicServiceUrl'
              value: objectStoragePublicServiceUrl
            }
            {
              name: 'ObjectStorage__AccessKey'
              secretRef: 'object-storage-access-key'
            }
            {
              name: 'ObjectStorage__SecretKey'
              secretRef: 'object-storage-secret-key'
            }
            {
              name: 'ObjectStorage__Bucket'
              value: objectStorageBucket
            }
            {
              name: 'Cors__WebOrigin'
              value: 'https://${webApp.?properties.?configuration.?ingress.?fqdn ?? webAppName}'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 80
              }
              initialDelaySeconds: 10
              periodSeconds: 10
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 80
              }
              initialDelaySeconds: 5
              periodSeconds: 5
            }
          ]
        }
      ]
      scale: {
        // scale to zero when idle to minimize cost; Container Apps wakes on incoming HTTP requests
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// ---------- the web container app ----------
resource webApp 'Microsoft.App/containerApps@2024-03-01' = if (!empty(webImage)) {
  name: webAppName
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentityResourceId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 3000
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistryLoginServer
          identity: containerAppIdentityResourceId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'web'
          image: webImage
          env: [
            {
              name: 'NODE_ENV'
              value: 'production'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/'
                port: 3000
              }
              initialDelaySeconds: 10
              periodSeconds: 10
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/'
                port: 3000
              }
              initialDelaySeconds: 5
              periodSeconds: 5
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// ---------- outputs ----------
output containerAppName string = containerApp.name
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output webAppName string = webAppName
output webAppUrl string = !empty(webImage)
  ? 'https://${webApp.?properties.?configuration.?ingress.?fqdn ?? webAppName}'
  : ''
output containerRegistryName string = containerRegistry.name
output containerRegistryLoginServer string = containerRegistryLoginServer
