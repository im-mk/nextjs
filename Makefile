.PHONY: db api start stop azurite infra-up infra-build-push infra-build-push-web infra-deploy-app infra-down

# docker

db:
	docker compose up -d db liquibase

api:
	make db
	docker compose up -d api --build

azurite:
	docker compose up -d azurite

start:
	docker compose up -d --build

stop:	
	docker compose stop

# azure infra

RESOURCE_GROUP ?= nextjs
LOCATION ?= uksouth
IMAGE_TAG ?= latest
ACR_NAME := $(shell az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.containerRegistryName.value -o tsv)

# create the resource group + registry + container apps environment + postgres db + blob storage foundation (idempotent)
infra-up:
	@test -n "$(DB_ADMIN_PASSWORD)" || (echo "DB_ADMIN_PASSWORD is required, e.g. make infra-up DB_ADMIN_PASSWORD=..." && exit 1)
	az deployment sub create -l $(LOCATION) -f infra/azure/subscription.bicep -p resourceGroupName=$(RESOURCE_GROUP) location=$(LOCATION) \
		dbAdminPassword="$(DB_ADMIN_PASSWORD)"

# build the api image in ACR and push it
infra-build-push:
	az acr build -r $(ACR_NAME) -t $(ACR_NAME).azurecr.io/app-api:$(IMAGE_TAG) -f api/App.Api/Dockerfile api

# build the web image in ACR; API_BASE_URL must be the deployed API origin
infra-build-push-web:
	@test -n "$(API_BASE_URL)" || (echo "API_BASE_URL is required, e.g. make infra-build-push-web API_BASE_URL=https://app-api.example" && exit 1)
	az acr build -r $(ACR_NAME) -t $(ACR_NAME).azurecr.io/app-web:$(IMAGE_TAG) -f web/Dockerfile --build-arg NEXT_PUBLIC_API_BASE_URL=$(API_BASE_URL) web

# deploy/update the container app with the freshly pushed image
# all secrets (db password, blob storage account key) are read from Key Vault via the app's managed identity - nothing plaintext passed here
infra-deploy-app:
	az deployment group create -g $(RESOURCE_GROUP) -f infra/azure/main.bicep \
		-p containerRegistryName=$(ACR_NAME) \
		   containerAppIdentityResourceId=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.containerAppIdentityResourceId.value -o tsv) \
		   image=$(ACR_NAME).azurecr.io/app-api:$(IMAGE_TAG) \
		   webImage=$(ACR_NAME).azurecr.io/app-web:$(IMAGE_TAG) \
		   databaseConnectionStringSecretUri=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.databaseConnectionStringSecretUri.value -o tsv) \
		   objectStorageServiceUri=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.objectStorageServiceUri.value -o tsv) \
		   objectStoragePublicServiceUri=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.objectStoragePublicServiceUri.value -o tsv) \
		   objectStorageAccountName=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.objectStorageAccountName.value -o tsv) \
		   objectStorageAccountKeySecretUri=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.objectStorageAccountKeySecretUri.value -o tsv) \
		   objectStorageContainerName=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.objectStorageContainerName.value -o tsv)

# delete the whole resource group (destructive, prompts for confirmation)
infra-down:
	az group delete -n $(RESOURCE_GROUP)