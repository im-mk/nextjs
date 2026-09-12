# api

.NET 10 Web API providing REST endpoints for contacts, countries, and documents.

## Stack

- ASP.NET Core 10, Dapper (raw SQL, no EF Core)
- Npgsql (Postgres driver)
- FusionCache (in-memory caching, e.g. countries list)
- Serilog (console + rolling file logs under `Logs/`)
- Swagger/Swashbuckle (Development environment only, at `/swagger`)
- AWSSDK.S3 (talks to Garage for document storage)

## Project layout

- `Controllers/` — thin HTTP layer, delegates to services
- `Services/` — validation + business logic
- `Repositories/` — Dapper queries against Postgres
- `Entities/` — DB row models
- `Models/` — request/response DTOs
- `Options/` — strongly-typed config sections (e.g. `ObjectStorageOptions`)
- `Mappers/` — manual entity/DTO mapping helpers

## Configuration

Config is loaded from `appsettings.json` → `appsettings.Development.json` → environment variables (Docker Compose sets these).

| Section | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | Postgres connection string |
| `ObjectStorage:ServiceUrl/AccessKey/SecretKey/Bucket` | Garage S3-compatible endpoint |
| `Serilog` | Logging sinks/levels |

## Endpoints

- `GET/POST/PATCH/DELETE /contacts` — contact CRUD
- `GET /countries` — cached country list
- `GET /documents`, `GET /documents/{id}` — document metadata
- `POST /documents/upload-url` — get a presigned PUT URL for uploading a file directly to Garage
- `POST /documents` — confirm a completed upload and persist its metadata
- `GET /documents/{id}/download-url` — get a presigned GET URL for downloading
- `DELETE /documents/{id}` — remove object + metadata
- `GET /health` — health check (used by Docker healthcheck)

## Running locally

```bash
cd api/App.Api
dotnet run
```

Requires `db` (port 5434) and `garage` (port 3900) to be reachable — start them with `make db` and `docker compose up -d garage` from the repo root first.

## Running in Docker

Built via [Dockerfile](App.Api/Dockerfile); listens on port 80 inside the container (`ENV ASPNETCORE_URLS=http://+:80`), mapped to `${EXTERNAL_PORT}` (default 8080) by `docker-compose.yaml`.

## Tests

```bash
cd api
dotnet test
```
