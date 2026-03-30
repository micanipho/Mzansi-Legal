# Implementation Plan: Deploy Backend to Railway via Docker

**Branch**: `013-railway-docker-deploy` | **Date**: 2026-03-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/013-railway-docker-deploy/spec.md`

## Summary

Deploy the Mzansi Legal .NET 9 + ABP Zero backend to Railway using Docker. The plan covers: fixing and upgrading the existing Dockerfile (.NET 8 → .NET 9), adding a health check endpoint, configuring environment variables in Railway, provisioning a Railway PostgreSQL plugin, and enabling automatic redeployment on push to `main`. No new domain entities or EF Core migrations are required — all existing migrations are applied at first startup via `Database.MigrateAsync()`.

## Technical Context

**Language/Version**: C# on .NET 9.0
**Primary Dependencies**: ABP Zero 10.x, EF Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, ASP.NET Core 9.0
**Storage**: PostgreSQL 15+ — provisioned as Railway PostgreSQL plugin
**Testing**: Existing test suite (`backend.Tests`, `backend.Web.Tests`); deployment verified via health check endpoint
**Target Platform**: Railway cloud (Linux container, x64)
**Project Type**: Web service (REST API)
**Performance Goals**: Service startup within 5 minutes; health check responds within 5 seconds
**Constraints**: Railway free/hobby tier limits (512 MB RAM, shared CPU); `$PORT` must be respected dynamically
**Scale/Scope**: Single Railway service + single Railway PostgreSQL plugin; no horizontal scaling for MVP

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: No new entities. The `HealthController` goes in `backend.Web.Host/Controllers/` — correct layer (Web.Host = presentation). ✅
- [x] **Naming Gate**: `HealthController` extends `backendControllerBase`, follows `{Entity}Controller` naming. ✅
- [x] **Coding Standards Gate**: `HealthController` will have purpose comment, single method, zero nesting — compliant with `docs/RULES.md`. ✅
- [x] **Skill Gate**: `follow-git-workflow` applies. No ABP CRUD endpoint skill needed (HealthController is trivial). ✅
- [x] **Multilingual Gate**: No user-facing outputs (infrastructure feature). ✅ N/A
- [x] **Citation Gate**: No AI-facing endpoints added. ✅ N/A
- [x] **Accessibility Gate**: No frontend components. ✅ N/A
- [x] **ETL/Ingestion Gate**: Not modifying document ingestion pipeline. ✅ N/A

**All gates pass.** No complexity tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/013-railway-docker-deploy/
├── plan.md              ← this file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   └── health-endpoint.md  ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit.tasks - not yet created)
```

### Source Code (repository root)

```text
backend/
├── Dockerfile                          ← NEW: revised multi-stage build (.NET 9, correct build context)
├── railway.toml                        ← NEW: Railway deployment configuration
└── src/
    └── backend.Web.Host/
        ├── Controllers/
        │   └── HealthController.cs     ← NEW: GET /api/health (unauthenticated)
        ├── Startup/
        │   └── Startup.cs              ← MODIFIED: add Database.MigrateAsync() on startup
        └── appsettings.json            ← MODIFIED: remove hardcoded Kestrel HTTPS endpoint (superseded by ASPNETCORE_URLS)
```

**Structure Decision**: Web application (Option 2). Only backend changes — no frontend modifications. The new `Dockerfile` moves to `backend/` to allow a correct Docker build context that includes all `src/` sibling projects.

## Phase 0: Research

**Status**: ✅ Complete — see [research.md](./research.md)

All unknowns resolved:

| Unknown | Decision |
|---------|----------|
| Database: external vs Railway plugin | Railway PostgreSQL plugin (see research Decision 1) |
| Dockerfile .NET version mismatch | Upgrade to .NET 9 images (see research Decision 2) |
| Dockerfile location for Railway | Move to `backend/Dockerfile` (see research Decision 3) |
| Health check approach | New `HealthController` at `GET /api/health` (see research Decision 4) |
| Environment variable strategy | ASP.NET Core double-underscore convention in Railway (see research Decision 5) |
| Startup migration strategy | `Database.MigrateAsync()` on startup (see research Decision 6) |
| Railway configuration file | `railway.toml` at `backend/railway.toml` (see research Decision 7) |

## Phase 1: Design & Contracts

**Status**: ✅ Complete

### Data Model

No new EF Core entities. See [data-model.md](./data-model.md) for:
- Environment variable configuration map
- New file artifacts (`Dockerfile`, `railway.toml`, `HealthController.cs`)
- Migration strategy and startup flow state diagram

### Interface Contracts

See [contracts/health-endpoint.md](./contracts/health-endpoint.md):
- `GET /api/health` → `200 OK` with `{ "status": "healthy" }`
- Anonymous (no JWT required)
- Used by Railway health check and uptime monitoring

### Quickstart

See [quickstart.md](./quickstart.md) for full step-by-step developer guide:
1. Verify Docker build locally
2. Create Railway project
3. Provision Railway PostgreSQL plugin
4. Configure environment variables
5. Deploy via `railway up`
6. Verify deployment
7. Set up automatic CI/CD

## Implementation Phases

### Phase A: Fix & Upgrade Dockerfile

**Files changed**:
- `backend/Dockerfile` (new location, replaces `backend/src/backend.Web.Host/Dockerfile`)

**Changes**:
1. Upgrade base images: `sdk:8.0` → `sdk:9.0-alpine`, `aspnet:8.0` → `aspnet:9.0-alpine`
2. Set build context to `backend/` — adjust all `COPY` source paths to match
3. Remove hardcoded `EXPOSE 80`; add `ENV PORT=80` as fallback (Railway sets `PORT` dynamically)
4. Add `ENV ASPNETCORE_URLS=http://+:$PORT` so Kestrel uses Railway's assigned port
5. Use `--no-restore` on publish (restore is a separate layer for better caching)

### Phase B: Add Health Check Endpoint

**Files changed**:
- `backend/src/backend.Web.Host/Controllers/HealthController.cs` (new)

**Changes**:
1. Create `HealthController : backendControllerBase`
2. `[AllowAnonymous]` attribute to bypass JWT
3. `[Route("api/[controller]")]` → resolves to `/api/health`
4. `[HttpGet]` action returning `Ok(new { status = "healthy" })`
5. Purpose comment on class and method per `docs/RULES.md`

### Phase C: Configure Startup Migrations

**Files changed**:
- `backend/src/backend.Web.Host/Startup/Startup.cs`

**Changes**:
1. Add `IServiceProvider` to `Configure` method signature (or use `IApplicationBuilder.ApplicationServices`)
2. Call `dbContext.Database.MigrateAsync()` during application startup, after ABP initialization
3. Wrap in `try/catch` — log error and rethrow on migration failure (fail fast)

### Phase D: Clean Up appsettings.json

**Files changed**:
- `backend/src/backend.Web.Host/appsettings.json`

**Changes**:
1. Remove the `Kestrel:Endpoints:Http:Url` section (conflicts with `ASPNETCORE_URLS` in Railway)
2. Replace `Authentication:JwtBearer:SecurityKey` placeholder with a safe empty string (real key comes from env var)
3. Set `Logging:LogLevel:Default` to `Information` in Production (Debug is too verbose for hosted env)

### Phase E: Add railway.toml

**Files changed**:
- `backend/railway.toml` (new)

**Changes**:
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

### Phase F: Railway Dashboard Setup (manual, one-time)

Not a code change — documented in [quickstart.md](./quickstart.md):
1. Create Railway project, connect GitHub repo
2. Set root directory to `backend/`
3. Provision Railway PostgreSQL plugin
4. Configure all environment variables (see data-model.md for full list)
5. Trigger first deploy and verify health check passes

## Complexity Tracking

No constitution violations. All gates pass with N/A or explicit justification above.
