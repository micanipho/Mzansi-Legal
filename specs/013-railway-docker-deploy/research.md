# Research: Deploy Backend to Railway via Docker

**Branch**: `013-railway-docker-deploy` | **Date**: 2026-03-30

---

## Decision 1: Database — Railway PostgreSQL Plugin vs External Instance

**Decision**: Use the Railway PostgreSQL plugin (a managed PostgreSQL instance provisioned within the same Railway project).

**Rationale**: The existing local database (`mzansi-pg`) runs in Docker and is not reachable from Railway's cloud infrastructure. Provisioning a Railway PostgreSQL plugin gives a production-appropriate, always-on database with a `DATABASE_URL` environment variable that Railway injects automatically. Migration from local data to the Railway instance is a one-time `pg_dump` / `pg_restore` operation.

**Alternatives considered**:
- Keep using the local Docker container: Not reachable from Railway cloud; ruled out.
- External managed PostgreSQL (Neon, Supabase, Azure): Valid long-term option but adds unnecessary external dependency for the initial deployment. Can be swapped later by changing the connection string.

---

## Decision 2: Docker Base Image — .NET SDK Version

**Decision**: Upgrade the Dockerfile from `mcr.microsoft.com/dotnet/sdk:8.0` / `aspnet:8.0` to `sdk:9.0` / `aspnet:9.0`.

**Rationale**: All `.csproj` files target `net9.0` and the CLAUDE.md confirms .NET 9.0 as the active runtime. The existing Dockerfile incorrectly references .NET 8 images, which causes a runtime version mismatch. Railway builds and runs the Dockerfile as-is, so this mismatch must be fixed before deployment.

**Alternatives considered**:
- Use `latest` tag: Avoids pinning but risks silent upgrades breaking the build; ruled out for production.
- Stay on .NET 8 and downgrade projects: Significant rework with no benefit; ruled out.

---

## Decision 3: Dockerfile Location for Railway

**Decision**: Place the primary `Dockerfile` at `backend/Dockerfile` (repo root of the backend subtree) and set Railway's **Root Directory** to `backend/`. Railway auto-detects a `Dockerfile` when the root directory is configured.

**Rationale**: The existing `Dockerfile` is at `backend/src/backend.Web.Host/Dockerfile`. That path works when Railway's root directory is set to `backend/src/backend.Web.Host/`, but `COPY` paths in the file reference sibling project directories (`../backend.Web.Core`, etc.) relative to `/src`, which breaks when the Docker build context is the Web.Host folder alone. Moving the Dockerfile one level up to `backend/` (and adjusting `COPY` paths) gives the build context access to all `src/` projects.

**Alternatives considered**:
- Keep Dockerfile at `backend/src/backend.Web.Host/` and set Railway root to that path: The `COPY` instructions reference sibling directories that fall outside the build context — the build fails.
- Use a `.railwayignore` to include parent directories: Railway doesn't support this pattern.

---

## Decision 4: Health Check Endpoint

**Decision**: Add a lightweight `GET /api/health` endpoint to `HomeController` that returns `200 OK` with a JSON body `{ "status": "healthy" }`. This endpoint is unauthenticated (no JWT required) and is used by Railway's health check and deployment verification.

**Rationale**: The current `HomeController.Index()` redirects to `/swagger`, which is not a reliable health check target (redirects return 3xx, not 2xx). Railway requires a 2xx response from the health check URL within a configurable timeout after startup. Adding a dedicated endpoint is the least-invasive approach and aligns with ABP's `backendControllerBase` pattern.

**Alternatives considered**:
- Use `/swagger/v1/swagger.json` as health check URL: Returns 200 but loads the entire Swagger document on every health poll — wasteful.
- Use ASP.NET Core's built-in `app.MapHealthChecks("/health")`: Valid, but requires `services.AddHealthChecks()` wiring and conflicts with ABP's middleware ordering. The simple controller endpoint is sufficient for MVP.

---

## Decision 5: Environment Variable Strategy

**Decision**: All runtime secrets and environment-specific values are supplied via Railway environment variables. The application reads them through ASP.NET Core's standard configuration system (`IConfiguration`). No secrets are stored in `appsettings.json` — that file contains only safe defaults and placeholders.

**Required Railway environment variables**:

| Variable | Maps to | Description |
|----------|---------|-------------|
| `ConnectionStrings__Default` | `ConnectionStrings:Default` | PostgreSQL connection string |
| `App__ServerRootAddress` | `App:ServerRootAddress` | Public Railway service URL (e.g., `https://mzansi-legal-backend.railway.app/`) |
| `App__ClientRootAddress` | `App:ClientRootAddress` | Frontend URL (for redirect links) |
| `App__CorsOrigins` | `App:CorsOrigins` | Comma-separated allowed CORS origins |
| `Authentication__JwtBearer__SecurityKey` | `Authentication:JwtBearer:SecurityKey` | JWT signing key (min 32 chars) |
| `Authentication__JwtBearer__Issuer` | `Authentication:JwtBearer:Issuer` | JWT issuer |
| `Authentication__JwtBearer__Audience` | `Authentication:JwtBearer:Audience` | JWT audience |
| `OpenAI__ApiKey` | `OpenAI:ApiKey` | OpenAI API key |
| `OpenAI__EmbeddingModel` | `OpenAI:EmbeddingModel` | Embedding model (default: text-embedding-ada-002) |
| `OpenAI__EnrichmentModel` | `OpenAI:EnrichmentModel` | Chat model (default: gpt-4o-mini) |
| `OpenAI__BaseUrl` | `OpenAI:BaseUrl` | OpenAI base URL |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment name | Set to `Production` |
| `ASPNETCORE_URLS` | Kestrel listen URL | Set to `http://+:$PORT` (Railway injects `$PORT`) |

**Note on `$PORT`**: Railway dynamically assigns a port and sets the `PORT` environment variable. The `ASPNETCORE_URLS` variable must be set to `http://+:${PORT}` so Kestrel listens on Railway's assigned port. The existing `appsettings.json` hardcodes `https://localhost:44311/` in `Kestrel:Endpoints:Http:Url` — this must be overridden via `ASPNETCORE_URLS` (which takes precedence over Kestrel config).

---

## Decision 6: Database Migrations at Startup

**Decision**: Migrations are applied automatically at application startup via a call to `dbContext.Database.MigrateAsync()` in the ABP module's `PostInitialize` or via a startup filter registered in `Startup.cs`.

**Rationale**: The backend already uses EF Core with Npgsql migrations. Applying migrations at startup ensures schema is always current after a redeploy, without needing a separate migration job or Railway cron. For the MVP scale, startup migration is safe and the simplest path.

**Alternatives considered**:
- Manual `dotnet ef database update` before each deploy: Fragile, requires CLI access to Railway environment; ruled out.
- Separate `backend.Migrator` project run as Railway job: Cleaner for production scale but adds complexity for MVP; defer to future iteration.

---

## Decision 7: Railway Configuration File

**Decision**: Add a `railway.toml` at `backend/railway.toml` to configure the build and deploy settings declaratively.

**Key settings**:
- `[build] builder = "dockerfile"` — use the Dockerfile
- `[deploy] healthcheckPath = "/api/health"` — verify the health endpoint post-deploy
- `[deploy] restartPolicyType = "ON_FAILURE"` — restart on crash, not on success

**Rationale**: A `railway.toml` makes the deployment configuration version-controlled and reproducible. Without it, all settings must be manually configured in the Railway dashboard on each project setup.
