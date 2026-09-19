.PHONY: db api start stop garage infra-up infra-build-push infra-build-push-web infra-deploy-app infra-down

# docker

db:
	docker compose up -d db liquibase

api:
	make db
	docker compose up -d api --build

garage:
	docker compose up -d garage --build

start:
	docker compose up -d --build

stop:	
	docker compose stop

# azure infra

RESOURCE_GROUP ?= nextjs
LOCATION ?= uksouth
IMAGE_TAG ?= latest
ACR_NAME := $(shell az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.containerRegistryName.value -o tsv)

# create the resource group + registry + container apps environment + postgres db + garage (idempotent)
infra-up:
	@test -n "$(DB_ADMIN_PASSWORD)" || (echo "DB_ADMIN_PASSWORD is required, e.g. make infra-up DB_ADMIN_PASSWORD=... GARAGE_ACCESS_KEY=... GARAGE_SECRET_KEY=... GARAGE_RPC_SECRET=... GARAGE_ADMIN_TOKEN=..." && exit 1)
	@test -n "$(GARAGE_ACCESS_KEY)" || (echo "GARAGE_ACCESS_KEY is required" && exit 1)
	@test -n "$(GARAGE_SECRET_KEY)" || (echo "GARAGE_SECRET_KEY is required" && exit 1)
	@test -n "$(GARAGE_RPC_SECRET)" || (echo "GARAGE_RPC_SECRET is required, e.g. openssl rand -hex 32" && exit 1)
	@test -n "$(GARAGE_ADMIN_TOKEN)" || (echo "GARAGE_ADMIN_TOKEN is required, e.g. openssl rand -base64 32" && exit 1)
	az deployment sub create -l $(LOCATION) -f infra/azure/subscription.bicep -p resourceGroupName=$(RESOURCE_GROUP) location=$(LOCATION) \
		dbAdminPassword="$(DB_ADMIN_PASSWORD)" \
		garageAccessKey="$(GARAGE_ACCESS_KEY)" \
		garageSecretKey="$(GARAGE_SECRET_KEY)" \
		garageRpcSecret="$(GARAGE_RPC_SECRET)" \
		garageAdminToken="$(GARAGE_ADMIN_TOKEN)"

# build the api image in ACR and push it
infra-build-push:
	az acr build -r $(ACR_NAME) -t $(ACR_NAME).azurecr.io/app-api:$(IMAGE_TAG) -f api/App.Api/Dockerfile api

# build the web image in ACR; API_BASE_URL must be the deployed API origin
infra-build-push-web:
	@test -n "$(API_BASE_URL)" || (echo "API_BASE_URL is required, e.g. make infra-build-push-web API_BASE_URL=https://app-api.example" && exit 1)
	az acr build -r $(ACR_NAME) -t $(ACR_NAME).azurecr.io/app-web:$(IMAGE_TAG) -f web/Dockerfile --build-arg NEXT_PUBLIC_API_BASE_URL=$(API_BASE_URL) web

# deploy/update the container app with the freshly pushed image
# all secrets (db password, garage keys) are read from Key Vault via the app's managed identity - nothing plaintext passed here
infra-deploy-app:
	az deployment group create -g $(RESOURCE_GROUP) -f infra/azure/main.bicep \
		-p containerRegistryName=$(ACR_NAME) \
		   containerAppIdentityResourceId=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.containerAppIdentityResourceId.value -o tsv) \
		   image=$(ACR_NAME).azurecr.io/app-api:$(IMAGE_TAG) \
		   webImage=$(ACR_NAME).azurecr.io/app-web:$(IMAGE_TAG) \
		   databaseConnectionStringSecretUri=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.databaseConnectionStringSecretUri.value -o tsv) \
		   objectStorageServiceUrl=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.garageServiceUrl.value -o tsv) \
		   objectStoragePublicServiceUrl=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.garagePublicServiceUrl.value -o tsv) \
		   objectStorageAccessKeySecretUri=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.garageAccessKeySecretUri.value -o tsv) \
		   objectStorageSecretKeySecretUri=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.garageSecretKeySecretUri.value -o tsv) \
		   objectStorageBucket=$$(az deployment group show -g $(RESOURCE_GROUP) -n foundation --query properties.outputs.garageBucket.value -o tsv)

# delete the whole resource group (destructive, prompts for confirmation)
infra-down:
	az group delete -n $(RESOURCE_GROUP)