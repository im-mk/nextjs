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

@description('Name of the Garage (S3-compatible) container app.')
param garageAppName string = 'garage'

@description('Name of the storage account backing Garage persistent volumes.')
param garageStorageAccountName string = 'garage${uniqueString(resourceGroup().id)}'

@secure()
@description('Access key for the Garage default bucket, used by the API to authenticate.')
param garageAccessKey string

@secure()
@description('Secret key for the Garage default bucket, used by the API to authenticate.')
param garageSecretKey string

@secure()
@description('RPC secret for Garage inter-node communication (required even in single-node mode).')
param garageRpcSecret string

@secure()
@description('Admin API token for Garage.')
param garageAdminToken string

@description('Name of the default bucket Garage creates for documents.')
param garageBucket string = 'documents'

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

resource garageAccessKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'garage-access-key'
  properties: {
    value: garageAccessKey
  }
}

resource garageSecretKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'garage-secret-key'
  properties: {
    value: garageSecretKey
  }
}

// ---------- garage (S3-compatible object storage) - persistent storage backing ----------
// cheapest redundancy tier; small quotas since documents are modest in size, cost scales with actual usage not quota
resource garageStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: garageStorageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

resource garageFileServices 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' existing = {
  parent: garageStorageAccount
  name: 'default'
}

resource garageMetaShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  parent: garageFileServices
  name: 'garage-meta'
  properties: {
    shareQuota: 5
  }
}

resource garageDataShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  parent: garageFileServices
  name: 'garage-data'
  properties: {
    shareQuota: 20
  }
}

resource garageMetaEnvStorage 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: containerAppsEnvironment
  name: 'garage-meta'
  properties: {
    azureFile: {
      accountName: garageStorageAccount.name
      accountKey: garageStorageAccount.listKeys().keys[0].value
      shareName: garageMetaShare.name
      accessMode: 'ReadWrite'
    }
  }
}

resource garageDataEnvStorage 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: containerAppsEnvironment
  name: 'garage-data'
  properties: {
    azureFile: {
      accountName: garageStorageAccount.name
      accountKey: garageStorageAccount.listKeys().keys[0].value
      shareName: garageDataShare.name
      accessMode: 'ReadWrite'
    }
  }
}

// ---------- garage (S3-compatible object storage) - container app ----------
resource garageApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: garageAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 3900
        transport: 'auto'
        allowInsecure: false
      }
      secrets: [
        {
          name: 'garage-toml'
          // single-node config mounted as a file, mirrors garage/garage.toml used for local dev
          // (inlined, not via a var, so Bicep treats it as secure since it's built from secure params)
          value: 'metadata_dir = "/var/lib/garage/meta"\ndata_dir = "/var/lib/garage/data"\ndb_engine = "lmdb"\n\nreplication_factor = 1\n\nrpc_bind_addr = "[::]:3901"\nrpc_public_addr = "127.0.0.1:3901"\nrpc_secret = "${garageRpcSecret}"\n\n[s3_api]\ns3_region = "garage"\napi_bind_addr = "[::]:3900"\nroot_domain = ".s3.garage.localhost"\n\n[admin]\napi_bind_addr = "[::]:3903"\nadmin_token = "${garageAdminToken}"\n'
        }
        {
          name: 'garage-access-key'
          value: garageAccessKey
        }
        {
          name: 'garage-secret-key'
          value: garageSecretKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'garage'
          image: 'docker.io/dxflrs/garage:v2.3.0'
          command: [
            '/garage'
            'server'
            '--single-node'
            '--default-bucket'
          ]
          env: [
            {
              name: 'GARAGE_DEFAULT_ACCESS_KEY'
              secretRef: 'garage-access-key'
            }
            {
              name: 'GARAGE_DEFAULT_SECRET_KEY'
              secretRef: 'garage-secret-key'
            }
            {
              name: 'GARAGE_DEFAULT_BUCKET'
              value: garageBucket
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          volumeMounts: [
            {
              volumeName: 'garage-config'
              mountPath: '/etc/garage.toml'
              subPath: 'garage.toml'
            }
            {
              volumeName: 'garage-meta'
              mountPath: '/var/lib/garage/meta'
            }
            {
              volumeName: 'garage-data'
              mountPath: '/var/lib/garage/data'
            }
          ]
        }
      ]
      volumes: [
        {
          name: 'garage-config'
          storageType: 'Secret'
          secrets: [
            {
              secretRef: 'garage-toml'
              path: 'garage.toml'
            }
          ]
        }
        {
          name: 'garage-meta'
          storageType: 'AzureFile'
          storageName: garageMetaEnvStorage.name
        }
        {
          name: 'garage-data'
          storageType: 'AzureFile'
          storageName: garageDataEnvStorage.name
        }
      ]
      // single, always-on replica: Garage is single-node/stateful and isn't designed to scale horizontally
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
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
output garageAccessKeySecretUri string = garageAccessKeySecret.properties.secretUri
output garageSecretKeySecretUri string = garageSecretKeySecret.properties.secretUri
output garageBucket string = garageBucket
output garageServiceUrl string = 'http://${garageApp.name}'
output garagePublicServiceUrl string = 'https://${garageApp.properties.configuration.ingress.fqdn}'
