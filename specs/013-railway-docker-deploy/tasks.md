# Tasks: Deploy Backend to Railway via Docker

**Input**: Design documents from `/specs/013-railway-docker-deploy/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Not explicitly requested — no test tasks generated. Verification is done via live health check and curl commands as described in quickstart.md.

**Organization**: Tasks grouped by user story to enable independent implementation and verification.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no blocking dependency)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)

## Path Conventions

Web application layout — all backend changes under `backend/`:

```text
backend/
├── Dockerfile                                         ← new root Dockerfile
├── railway.toml                                       ← new Railway config
└── src/
    └── backend.Web.Host/
        ├── Controllers/HealthController.cs            ← new
        ├── Startup/Startup.cs                        ← modified
        └── appsettings.json                          ← modified
```

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the correct build context and remove the stale artifact before any story work begins.

- [x] T001 Add `backend/.dockerignore` to exclude `bin/`, `obj/`, `.vs/`, `tests/` from the Docker build context (reduces build time and image size) — create file `backend/.dockerignore`
- [x] T002 Delete (or archive) the stale `backend/src/backend.Web.Host/Dockerfile` to avoid confusion — this file targets .NET 8 and uses the wrong build context; the canonical Dockerfile will live at `backend/Dockerfile`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the new `backend/Dockerfile` with the correct build context. All user story deployments depend on this file existing and building successfully.

**⚠️ CRITICAL**: No user story can be verified in Railway until this phase is complete and the local Docker build passes.

- [x] T003 Create `backend/Dockerfile` — multi-stage build using `mcr.microsoft.com/dotnet/sdk:9.0-alpine` (build stage) and `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` (runtime stage); COPY all five `src/` projects from the `backend/` build context; publish to `/publish` with `dotnet publish -c Release -o /publish`
- [x] T004 Add `ENV PORT=80` (Railway default fallback) and `ENV ASPNETCORE_URLS=http://+:$PORT` to the runtime stage of `backend/Dockerfile` so Kestrel binds to Railway's dynamically assigned port
- [x] T005 Verify the Docker build succeeds locally: `cd backend && docker build -t mzansi-legal-backend:local .` — confirm image is produced without errors before proceeding

**Checkpoint**: Local Docker build passes → user story implementation can now begin

---

## Phase 3: User Story 1 — Initial Backend Deployment to Railway (Priority: P1) 🎯 MVP

**Goal**: The backend is running in Railway at a public URL, the health check endpoint returns 200, and Railway auto-redeploys on push to `main`.

**Independent Test**: `curl https://<service>.railway.app/api/health` returns `{"status":"healthy"}` with HTTP 200.

### Implementation for User Story 1

- [x] T006 [P] [US1] Create `backend/src/backend.Web.Host/Controllers/HealthController.cs` — extends `backendControllerBase`, decorated with `[AllowAnonymous]`, route `[Route("api/[controller]")]`, single `[HttpGet]` action returning `Ok(new { status = "healthy" })`; add class and method purpose comments per `docs/RULES.md`
- [x] T007 [P] [US1] Create `backend/railway.toml` — set `[build] builder = "dockerfile"`, `dockerfilePath = "Dockerfile"`, `[deploy] healthcheckPath = "/api/health"`, `healthcheckTimeout = 60`, `restartPolicyType = "ON_FAILURE"`, `restartPolicyMaxRetries = 3`
- [ ] T008 [US1] Create Railway project: run `railway login && railway init` from `backend/` directory; connect GitHub repository; set **Root Directory** to `backend/` in Railway dashboard → Settings → Source
- [ ] T009 [US1] Configure the minimum required Railway environment variables needed for the service to start (from data-model.md): `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_URLS=http://+:${{PORT}}`, and placeholder values for `Authentication__JwtBearer__SecurityKey`, `Authentication__JwtBearer__Issuer`, `Authentication__JwtBearer__Audience` — full secrets configuration is covered in US2
- [ ] T010 [US1] Trigger first Railway deployment: `railway up` from `backend/` — monitor build logs in Railway dashboard; confirm the service reaches **Active** status and the health check at `/api/health` returns 200

**Checkpoint**: `curl https://<service>.railway.app/api/health` → `{"status":"healthy"}` ✅ — User Story 1 is complete

---

## Phase 4: User Story 2 — Environment Variables and Secrets Configuration (Priority: P2)

**Goal**: All runtime secrets (DB, JWT, OpenAI) are configured in Railway; no secrets appear in source code or build logs; the deployed backend correctly reads all configuration.

**Independent Test**: Call an authenticated API endpoint (e.g., `/api/services/app/account/isTenantAvailable`) from the Railway URL — it must respond correctly, proving JWT config is loaded. Confirm OpenAI key is not visible in Railway build logs.

### Implementation for User Story 2

- [x] T011 [US2] Update `backend/src/backend.Web.Host/appsettings.json` — remove the `Kestrel:Endpoints:Http:Url` hardcoded HTTPS section (conflicts with `ASPNETCORE_URLS`); replace the `Authentication:JwtBearer:SecurityKey` placeholder with an empty string (real key injected via env var); set `Logging:LogLevel:Default` to `Information` (not `Debug`) for production noise reduction
- [ ] T012 [US2] Configure the full set of Railway environment variables in the Railway dashboard (Variables tab) per the table in `specs/013-railway-docker-deploy/data-model.md`: `ConnectionStrings__Default`, `App__ServerRootAddress`, `App__ClientRootAddress`, `App__CorsOrigins`, `Authentication__JwtBearer__SecurityKey` (32+ char secret), `Authentication__JwtBearer__Issuer`, `Authentication__JwtBearer__Audience`, `OpenAI__ApiKey`, `OpenAI__EmbeddingModel`, `OpenAI__EnrichmentModel`, `OpenAI__BaseUrl`
- [ ] T013 [US2] Redeploy the service after env var changes (`railway up` or trigger via GitHub push) — verify in Railway logs that the service starts without configuration errors and that no secret values appear in plain text in the log output

**Checkpoint**: Authenticated API endpoint responds correctly; no secrets in build logs ✅ — User Story 2 is complete

---

## Phase 5: User Story 3 — Persistent Database Connectivity (Priority: P3)

**Goal**: The deployed backend connects to a persistent Railway PostgreSQL instance; all EF Core migrations are applied automatically on startup; data survives container restarts and redeployments.

**Independent Test**: Create a record via the API (e.g., POST to any entity endpoint), redeploy the service, then GET the same record — it must still exist, proving persistence across deployments.

### Implementation for User Story 3

- [ ] T014 [US3] Provision Railway PostgreSQL plugin: in Railway dashboard → your project → **New** → **Database** → **Add PostgreSQL**; copy the connection details and convert from `postgresql://` URL format to Npgsql format: `Host=<host>;Port=<port>;Database=<db>;Username=postgres;Password=<pw>;SSL Mode=Require;Trust Server Certificate=true`
- [ ] T015 [US3] Update `ConnectionStrings__Default` in Railway Variables tab with the Npgsql-formatted connection string from T014; redeploy and confirm the service starts without a database connection error in the logs
- [x] T016 [US3] Add `Database.MigrateAsync()` call in `backend/src/backend.Web.Host/Startup/Startup.cs` — invoke within `Configure()` after `app.UseAbp()`, using `app.ApplicationServices.CreateScope()` to resolve `backendDbContext`; wrap in try/catch that logs and rethrows on failure (fail fast)
- [ ] T017 [US3] Deploy and verify migration runs: check Railway logs for EF Core migration output confirming all migrations applied successfully on first startup against the Railway PostgreSQL database
- [ ] T018 [US3] Verify data persistence: use the Swagger UI at `https://<service>.railway.app/swagger` to create a test record; trigger a redeploy via `railway up`; retrieve the same record and confirm it persists

**Checkpoint**: Data created before redeploy is still accessible after redeploy ✅ — User Story 3 is complete

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finalise CI/CD automation, clean up, and ensure the deployment is production-ready.

- [ ] T019 [P] Configure automatic GitHub-triggered deployments in Railway dashboard → service Settings → Source: set branch to `main` so every push triggers a redeploy without manual `railway up`
- [ ] T020 [P] Update `App__ServerRootAddress` in Railway Variables to the actual assigned Railway public URL (e.g., `https://mzansi-legal-backend.railway.app/`) and redeploy — this ensures ABP's internal URL generation and Swagger UI work correctly
- [ ] T021 Validate the full deployment by working through the checklist in `specs/013-railway-docker-deploy/quickstart.md` Step 6 (Troubleshooting section) — confirm health check, Swagger UI, authenticated endpoint, and CORS all work from the frontend origin
- [x] T022 [P] Update `backend/src/backend.Web.Host/appsettings.Staging.json` to use the Railway PostgreSQL connection string pattern (replacing the SQL Server connection string) so Staging config is consistent with Production

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately (T001, T002 can run in parallel)
- **Foundational (Phase 2)**: Depends on Phase 1 completion — **BLOCKS all user stories**
- **User Stories (Phase 3–5)**: All depend on Foundational phase (Phase 2) completion
  - US1 (Phase 3) must complete before US2 (Phase 4) and US3 (Phase 5) — Railway project must exist
  - US2 and US3 can proceed in parallel once US1 is complete (different concerns: config vs database)
- **Polish (Phase 6)**: Depends on all three user stories being complete

### User Story Dependencies

- **US1 (P1)**: Requires Phase 2 complete. No dependency on US2 or US3.
- **US2 (P2)**: Requires US1 complete (Railway project must exist to set Variables). No dependency on US3.
- **US3 (P3)**: Requires US1 complete (Railway project must exist). Can run in parallel with US2.
- **Polish**: Requires US1 + US2 + US3 complete.

### Within Each User Story

- T006 (HealthController) and T007 (railway.toml) within US1 are parallel — different files
- T008 (Railway project creation) depends on T006 and T007 existing in the repo
- T009 and T010 are sequential within US1 — env vars must exist before deploy

### Parallel Opportunities

- T001 and T002 (Phase 1) can run in parallel
- T003 and T004 (Phase 2) must be sequential — T004 modifies what T003 creates
- T006 and T007 (US1) can run in parallel
- Once US1 is complete: US2 (T011–T013) and US3 (T014–T018) can run in parallel across two developers
- T019, T020, T022 (Polish) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Run T006 and T007 simultaneously (different files, no shared dependency):
Task T006: "Create HealthController.cs in backend/src/backend.Web.Host/Controllers/"
Task T007: "Create railway.toml in backend/"

# Then sequentially:
T008 → T009 → T010
```

## Parallel Example: US2 + US3 (after US1 complete)

```bash
# Developer A: US2 environment variables
T011 → T012 → T013

# Developer B: US3 database connectivity
T014 → T015 → T016 → T017 → T018
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T002)
2. Complete Phase 2: Foundational (T003–T005) — **must pass locally**
3. Complete Phase 3: User Story 1 (T006–T010)
4. **STOP and VALIDATE**: `curl https://<service>.railway.app/api/health` → 200
5. Backend is live — demo-able even without full secrets or database

### Incremental Delivery

1. Phase 1 + Phase 2 → Docker build works locally
2. Phase 3 (US1) → Backend reachable in Railway at public URL (MVP)
3. Phase 4 (US2) → Secrets configured, authenticated endpoints work
4. Phase 5 (US3) → Database connected, migrations applied, data persists
5. Phase 6 (Polish) → CI/CD automated, fully production-ready

### Parallel Team Strategy

With two developers after Phase 2 (Foundational) is done:

1. Developer A completes US1 (Phase 3) alone
2. Once Railway project exists (T008 done):
   - Developer A: US2 (Phase 4) — env vars and config cleanup
   - Developer B: US3 (Phase 5) — PostgreSQL provisioning and migrations

---

## Notes

- [P] tasks = different files, no blocking dependencies between them
- [Story] label maps each task to its user story for traceability
- US1 is the only true MVP gate — the backend can be demoed at a Railway URL even without database or full secrets
- T005 (local Docker build verification) is a mandatory gate before any Railway deployment attempt — a broken local build will always fail in Railway
- Railway uses `${{PORT}}` syntax in variable values (double braces) when referencing other Railway variables; use `http://+:${{PORT}}` literally in the `ASPNETCORE_URLS` variable value in the dashboard
- The old `backend/src/backend.Web.Host/Dockerfile` (T002) references .NET 8 — deleting it prevents accidental use and reduces confusion
