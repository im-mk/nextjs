# web

Next.js 16 (App Router) frontend for the contacts/documents CRM UI.

## Stack

- Next.js 16, React 19
- Tailwind CSS v4
- shadcn-style UI primitives in `shadcn/ui/` (manually added, no CLI config — `components.json` doesn't exist in this repo)
- CSS Modules for page-specific styling

## Project layout

- `app/` — routed pages (`contacts/`, `documents/`, `tasks/`, root `page.tsx`)
- `app/lib/` — shared client-side config (`api-config.ts` — API base URL) and models
- `components/ui/` — currently empty; project-level UI components live under `shadcn/ui/`
- `shadcn/ui/` — reusable primitives (`button.tsx`, `dialog.tsx`, `input.tsx`, `table.tsx`)
- `lib/utils.ts` — `cn()` class-merging helper

## Configuration

- `NEXT_PUBLIC_API_BASE_URL` — base URL for the API, defaults to `http://localhost:8080` (see [app/lib/api-config.ts](app/lib/api-config.ts)). All API calls happen client-side (`"use client"` components), so this must be reachable from the browser, not just from inside Docker's network.

## Running locally (recommended for development)

```bash
npm install
npm run dev
```

Hot-reloading dev server on [http://localhost:3000](http://localhost:3000).

## Running in Docker

Built via [Dockerfile](Dockerfile) as a production build (`npm run build` + `npm start`, listening on port 3000 inside the container). This does **not** hot-reload — after code changes, rebuild with:

```bash
docker compose up -d --build web
```

Mapped to `${WEB_EXTERNAL_PORT}` (default 8090) by `docker-compose.yaml`.

## Lint

```bash
npm run lint
```
