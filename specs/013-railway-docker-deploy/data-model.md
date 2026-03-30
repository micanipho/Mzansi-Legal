# Data Model: Deploy Backend to Railway via Docker

**Branch**: `013-railway-docker-deploy` | **Date**: 2026-03-30

---

## Overview

This is a DevOps/infrastructure feature. **No new domain entities or database migrations are introduced.** The existing schema (all prior migrations through `012-legislation-seed-data`) is applied to the Railway-provisioned PostgreSQL database at first startup.

---

## Configuration Entities (not domain entities)

These are structural artifacts — not EF Core entities — but they define the shape of runtime configuration consumed by the application.

### Environment Variable Configuration Map

| Config Key (ASP.NET Core) | Railway Env Var | Type | Required | Notes |
|--------------------------|-----------------|------|----------|-------|
| `ConnectionStrings:Default` | `ConnectionStrings__Default` | string | Yes | Full Npgsql connection string |
| `App:ServerRootAddress` | `App__ServerRootAddress` | string (URL) | Yes | Must end with `/` |
| `App:ClientRootAddress` | `App__ClientRootAddress` | string (URL) | No | Used for redirect links |
| `App:CorsOrigins` | `App__CorsOrigins` | string (CSV URLs) | Yes | Comma-separated, no trailing slashes |
| `Authentication:JwtBearer:SecurityKey` | `Authentication__JwtBearer__SecurityKey` | string | Yes | Min 32 characters |
| `Authentication:JwtBearer:Issuer` | `Authentication__JwtBearer__Issuer` | string | Yes | |
| `Authentication:JwtBearer:Audience` | `Authentication__JwtBearer__Audience` | string | Yes | |
| `OpenAI:ApiKey` | `OpenAI__ApiKey` | string | Yes | |
| `OpenAI:EmbeddingModel` | `OpenAI__EmbeddingModel` | string | No | Default: `text-embedding-ada-002` |
| `OpenAI:EnrichmentModel` | `OpenAI__EnrichmentModel` | string | No | Default: `gpt-4o-mini` |
| `OpenAI:BaseUrl` | `OpenAI__BaseUrl` | string (URL) | No | Default: `https://api.openai.com/` |
| `ASPNETCORE_ENVIRONMENT` | `ASPNETCORE_ENVIRONMENT` | string | Yes | Set to `Production` |
| `ASPNETCORE_URLS` | `ASPNETCORE_URLS` | string | Yes | `http://+:${PORT}` |

**ASP.NET Core double-underscore convention**: Environment variables use `__` (double underscore) as the hierarchy separator, mapping to `:` in config keys (e.g., `ConnectionStrings__Default` → `ConnectionStrings:Default`).

---

## New File Artifacts

These files are created as part of this feature (not EF Core migrations):

### `backend/Dockerfile` (revised)

Multi-stage build:
1. **Stage 1 — build** (`mcr.microsoft.com/dotnet/sdk:9.0-alpine`): Restores and publishes to `/publish`
2. **Stage 2 — runtime** (`mcr.microsoft.com/dotnet/aspnet:9.0-alpine`): Copies published output, exposes `$PORT`

Key changes from existing Dockerfile:
- Upgrade from .NET 8 → .NET 9 base images
- Copy all `src/` projects from the `backend/` build context
- Use `ASPNETCORE_URLS=http://+:$PORT` as the default (Railway-compatible)
- Remove hardcoded `EXPOSE 80` in favour of `ENV PORT=80` default with Railway override

### `backend/railway.toml`

Railway deployment configuration:
```toml
[build]
builder = "dockerfile"
dockerfilePath = "Dockerfile"

[deploy]
healthcheckPath = "/api/health"
healthcheckTimeout = 60
restartPolicyType = "ON_FAILURE"
restartPolicyMaxRetries = 3
```

### `backend/src/backend.Web.Host/Controllers/HealthController.cs` (new)

Lightweight unauthenticated controller returning `{ "status": "healthy" }` at `GET /api/health`.

---

## Migration Strategy

| Step | Action | Tool |
|------|--------|------|
| 1 | Railway PostgreSQL plugin provisioned | Railway dashboard |
| 2 | `DATABASE_URL` (Railway format) converted to Npgsql connection string format | Manual config in Railway env vars |
| 3 | All existing migrations applied at first startup | EF Core `Database.MigrateAsync()` on startup |
| 4 | Seed data (legislation, from feature 012) optionally re-run via ETL pipeline | Existing ETL pipeline |

---

## State Transition: Startup Flow

```
Container start
    → Read environment variables (fail fast if required vars missing)
    → Connect to PostgreSQL
    → Apply pending EF Core migrations via MigrateAsync()
    → Start Kestrel on $PORT
    → Register /api/health endpoint
    → Service ready (Railway health check passes)
```
