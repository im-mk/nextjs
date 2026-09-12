.PHONY: db api start stop garage

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