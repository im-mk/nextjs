# database

Postgres schema and migrations, managed with Liquibase. Runs as two Compose services: `db` (Postgres 18) and `liquibase` (applies migrations on startup, then exits).

## Layout

- `changelog.yaml` — root Liquibase changelog; includes all files under `sql/`, `seed/`, and `dev/` (in that order)
- `sql/` — schema changesets, always applied (e.g. `00010.sql` countries, `00040-custaddresses.sql` contact_addresses, `00050-documents.sql` documents)
- `seed/` — data seeding changesets, always applied
- `dev/` — data seeding changesets, only applied when the `dev` context is active (see `--contexts=dev` in `docker-compose.yaml`)
- `init/init.sql` — runs once via Postgres's own `docker-entrypoint-initdb.d` mechanism (before Liquibase), e.g. for creating the database/extensions
- `liquibase.properties` — Liquibase CLI config (changelog path, driver)

## Adding a migration

1. Add a new file to `sql/` (schema) or `seed/`/`dev/` (data), named with the next sequential number, e.g. `00060-my-change.sql`.
2. Use the Liquibase formatted-SQL header:
   ```sql
   --liquibase formatted sql

   --changeset user:00060
   --comment: short description
   CREATE TABLE ...;

   --rollback DROP TABLE ...;
   ```
3. Files are picked up automatically via `includeAll` in `changelog.yaml` — no need to edit it directly.

## Running migrations

```bash
make db   # starts db + liquibase, which applies pending changesets and exits
```

Check status with:

```bash
docker compose logs liquibase
```

## Connecting

Default connection (see [.env](../.env)): `localhost:${DB_EXTERNAL_PORT}` (5434), database `appdb`, user/password `postgres`/`postgres`. Optional [pgAdmin](../docker-compose.yaml) UI available on port 5050 (`make pgadmin`-style, or `docker compose --profile dev up -d pgadmin`).
