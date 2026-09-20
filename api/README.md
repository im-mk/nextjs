# api

.NET 10 Web API providing REST endpoints for contacts, countries, and documents.

## Stack

- ASP.NET Core 10, Dapper (raw SQL, no EF Core)
- Npgsql (Postgres driver)
- FusionCache (in-memory caching, e.g. countries list)
- Serilog (console + rolling file logs under `Logs/`)
- Swagger/Swashbuckle (Development environment only, at `/swagger`)
- Azure.Storage.Blobs (default document storage provider)
- AWSSDK.S3 (optional AWS document storage provider)

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
| `ObjectStorage:Provider` | Selects `Azure` or `Aws`; defaults to `Azure` |
| `ObjectStorage:Azure:*` | Azure Blob Storage or Azurite settings |
| `ObjectStorage:Aws:*` | AWS S3-compatible settings |
| `Serilog` | Logging sinks/levels |

## Endpoints

- `GET/POST/PATCH/DELETE /contacts` — contact CRUD
- `GET /countries` — cached country list
- `GET /documents`, `GET /documents/{id}` — document metadata
- `POST /documents/upload-url` — get a signed PUT URL for uploading a file directly to the configured object store
- `POST /documents` — confirm a completed upload and persist its metadata
- `GET /documents/{id}/download-url` — get a presigned GET URL for downloading
- `DELETE /documents/{id}` — remove object + metadata
- `GET /health` — health check (used by Docker healthcheck)

## Running locally

```bash
cd api/App.Api
dotnet run
```

Requires `db` (port 5434) and a local object storage endpoint to be reachable.

When `ObjectStorage:Provider` is `Azure`, the development API configures Azurite blob CORS on startup for the local web origins used by this repo (`http://localhost:3000` and `http://localhost:8090`, plus optional `Cors:WebOrigin`). This allows the browser to upload directly to signed Azurite URLs during local development.

When `ObjectStorage:Provider` is `Aws`, the development API creates the configured bucket if needed and applies matching S3 bucket CORS rules for the same local web origins. This supports direct browser uploads to a local S3-compatible endpoint such as LocalStack.

Azurite local setup for this API:

- Docker Compose runs Azurite as the `azurite` service.
- The blob endpoint is exposed on `http://localhost:10000`.
- The API uses the default Azurite account `devstoreaccount1` and creates the `documents` container automatically on startup.
- Internal API endpoint: `http://azurite:10000/devstoreaccount1`
- Browser-facing endpoint for SAS URLs: `http://localhost:10000/devstoreaccount1`
- If you need a different host port, set `AZURITE_BLOB_PORT` before starting Docker Compose.

LocalStack S3 setup for this API:

- Docker Compose runs LocalStack as the `localstack` service.
- The S3 endpoint is exposed on `http://localhost:4566`.
- Switch Docker Compose to AWS mode with `OBJECT_STORAGE_PROVIDER=Aws make start`.
- Internal API endpoint: `http://localstack:4566`
- Browser-facing endpoint for presigned URLs: `http://localhost:4566`
- The API creates the `documents` bucket automatically on startup and configures bucket CORS for local web origins.

## Running in Docker

Built via [Dockerfile](App.Api/Dockerfile); listens on port 80 inside the container (`ENV ASPNETCORE_URLS=http://+:80`), mapped to `${EXTERNAL_PORT}` (default 8080) by `docker-compose.yaml`.

## Tests

```bash
cd api
dotnet test
```
