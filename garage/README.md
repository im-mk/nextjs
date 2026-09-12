# garage

Local S3-compatible object storage (via [Garage](https://garagehq.deuxfleurs.fr/)), used by the `api` service to store uploaded document files. Only metadata (filename, size, content type, storage key) is kept in Postgres — the file bytes live here.

> Note: MinIO's community edition is no longer maintained (its OSS repo is archived), so this project uses Garage instead — it's actively developed and S3-API compatible.

## Why not store files in Postgres or on the api container's disk?

- The `api` container is stateless and rebuilt often — anything written to its local filesystem is lost on rebuild unless mounted, and it won't scale across multiple replicas.
- Storing large binary blobs directly in Postgres bloats backups and hurts performance at any real scale.

## Setup

- [garage.toml](garage.toml) — single-node config. `rpc_secret` and `admin_token` are **fixed dev-only values** committed to the repo; regenerate them (`openssl rand -hex 32` / `openssl rand -base64 32`) before using this anywhere beyond local dev.
- Runs with `--single-node --default-bucket`, which auto-creates a default access key and bucket from the `GARAGE_DEFAULT_ACCESS_KEY` / `GARAGE_DEFAULT_SECRET_KEY` / `GARAGE_DEFAULT_BUCKET` env vars in [.env](../.env) — no manual `garage layout`/`bucket`/`key` commands needed for local dev.
- Data persists in the `garage-meta` / `garage-data` Docker volumes.

## Ports

| Port (host) | Purpose |
|---|---|
| `${GARAGE_S3_PORT}` (3900) | S3 API — what the `api` service and AWS SDK talk to |
| `${GARAGE_ADMIN_PORT}` (3903) | Admin API |

`api` connects to Garage via `http://garage:3900` inside the Docker network (see `ObjectStorage__*` env vars in `docker-compose.yaml`); from the host, use `http://localhost:3900`.

## Inspecting the bucket

Use `awscli` or the `mc` client pointed at the Garage endpoint with the credentials from `.env`, e.g.:

```bash
aws --endpoint-url http://localhost:3900 s3 ls s3://documents --region garage
```

## Upgrading

Pin the image tag explicitly (currently `dxflrs/garage:v2.3.0`) in `docker-compose.yaml`; check the [Garage upgrade docs](https://garagehq.deuxfleurs.fr/documentation/operations/upgrading/) before bumping major versions.
