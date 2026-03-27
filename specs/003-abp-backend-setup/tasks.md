# Tasks: ABP Backend Foundation Setup

**Input**: Design documents from `/specs/003-abp-backend-setup/`
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/api.md ✅ | quickstart.md ✅

---

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Paths use `backend/src/` prefix relative to repo root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Replace the SQL Server EF Core provider with Npgsql and update all configuration
files. These tasks touch different files and can be done in parallel once started.

- [X] T001 [P] Swap `Microsoft.EntityFrameworkCore.SqlServer` for `Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4` and remove stale `<Compile Remove>` entries in `backend/src/backend.EntityFrameworkCore/backend.EntityFrameworkCore.csproj`
- [X] T002 [P] Replace both `builder.UseSqlServer(...)` calls with `builder.UseNpgsql(...)` in `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContextConfigurer.cs`; add an XML `///` class-level summary comment describing the class purpose and a `///` summary on each `Configure` overload (required by `docs/RULES.md`)
- [X] T003 [P] Update `ConnectionStrings.Default` to Npgsql format (`Host=localhost;Database=MzansiLegalDb;Username=postgres;Password=YOUR_LOCAL_PASSWORD`) in `backend/src/backend.Web.Host/appsettings.json` — use your actual local PostgreSQL password; never commit real credentials; override via a gitignored `appsettings.Development.json` or environment variable for shared environments
- [X] T004 [P] Update `ConnectionStrings.Default` to Npgsql format (`Host=localhost;Database=MzansiLegalDb;Username=postgres;Password=YOUR_LOCAL_PASSWORD`) in `backend/src/backend.Migrator/appsettings.json` — same credential handling rules as T003

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Delete all SQL Server migration files, restore packages, verify a clean build, and
regenerate the initial PostgreSQL migration. MUST complete before any user story work begins.

**⚠️ CRITICAL**: No user story validation can begin until this phase is complete.

- [X] T005 Delete every `.cs` and `.Designer.cs` file under `backend/src/backend.EntityFrameworkCore/Migrations/` including `backendDbContextModelSnapshot.cs` (all files — none excluded)
- [X] T006 Run `dotnet restore` then `dotnet build` from `backend/` and confirm output is `Build succeeded. 0 Error(s)` with no `SqlServer*` namespace errors (depends on T001–T005)
- [X] T007 Run `dotnet ef migrations add InitialCreate --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host` from `backend/` and verify the generated `InitialCreate.cs` uses PostgreSQL column types (`text`, `boolean`, `timestamp without time zone`) and `backendDbContextModelSnapshot.cs` references `NpgsqlModelBuilderExtensions` not `SqlServerModelBuilderExtensions` (depends on T006)

**Checkpoint**: Migration files regenerated for PostgreSQL — user story implementation can begin.

---

## Phase 3: User Story 1 — Developer Runs the Backend Locally (Priority: P1) 🎯 MVP

**Goal**: Application starts without errors, API explorer loads, and health check passes.

**Independent Test**: Run `dotnet run --project src/backend.Web.Host` and confirm Swagger UI
loads at `https://localhost:44311/swagger` and `GET /api/HealthCheck/Get` returns
`{ "success": true }`.

### Implementation for User Story 1

- [ ] T008 [US1] Run `dotnet run --project src/backend.Migrator` from `backend/` and confirm console output ends with `Host database seeds completed.` — verifies the migrator connects to PostgreSQL and applies `InitialCreate` successfully (depends on T007)
- [ ] T009 [US1] Run `dotnet run --project src/backend.Web.Host` from `backend/` and confirm the application starts without runtime exceptions and logs `Now listening on: https://localhost:44311` (depends on T008)
- [ ] T010 [US1] Open `https://localhost:44311/swagger` in a browser and confirm Swagger UI loads with ABP Zero endpoints (including `/api/TokenAuth/Authenticate` and `/api/HealthCheck/Get`) visible and invocable (depends on T009)
- [ ] T011 [US1] Call `GET /api/HealthCheck/Get` via Swagger UI and confirm HTTP 200 response with body `{ "success": true }` (depends on T010)
- [ ] T012 [US1] Call `POST /api/TokenAuth/Authenticate` via Swagger UI with `{ "userNameOrEmailAddress": "admin", "password": "123qwe", "rememberClient": false }` and confirm HTTP 200 response containing a non-empty `accessToken` field (depends on T010)

**Checkpoint**: User Story 1 complete — backend runs locally, Swagger loads, auth works end-to-end.

---

## Phase 4: User Story 2 — Developer Applies Migrations and Verifies the Database (Priority: P2)

**Goal**: PostgreSQL schema is fully created, seed data is present, and re-running migrations
is idempotent.

**Independent Test**: Connect to the `MzansiLegalDb` PostgreSQL database with a client and run
the SQL queries below to confirm tables and seed rows exist.

### Implementation for User Story 2

- [ ] T013 [US2] Connect to `MzansiLegalDb` with a PostgreSQL client (psql or pgAdmin) and run `SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename;` — confirm all ABP Zero tables (e.g. `AbpUsers`, `AbpRoles`, `AbpSettings`, `AbpTenants`) are present (depends on T008)
- [ ] T014 [US2] Run `SELECT "Name" FROM "AbpRoles";` and confirm rows `Admin` and `Citizen` exist — verifies ABP Zero role seeding (depends on T013)
- [ ] T015 [US2] Run `SELECT "UserName" FROM "AbpUsers";` and confirm the `admin` user exists — verifies host user seeding (depends on T013)
- [ ] T016 [US2] Run `dotnet run --project src/backend.Migrator` a second time and confirm it exits with `Host database seeds completed.` and no errors — verifies idempotent migration behaviour (depends on T008)

**Checkpoint**: User Story 2 complete — PostgreSQL schema and seed data verified, migrations are idempotent.

---

## Phase 5: User Story 3 — Developer Validates Folder Structure Compliance (Priority: P3)

**Goal**: Solution layer structure matches `docs/BACKEND_STRUCTURE.md` with no cross-layer
violations.

**Independent Test**: Open the solution and walk through each project layer confirming each
contains only permitted artifact types; confirm no violations require workarounds when adding
a new entity.

### Implementation for User Story 3

- [X] T017 [P] [US3] Verify `backend/src/backend.Core/` contains only domain entities, domain services, authorization definitions, localization sources, and ABP module config — no DTOs, no EF Core references, no HTTP references
- [X] T018 [P] [US3] Verify `backend/src/backend.Application/` contains only application services, interfaces, DTOs, and AutoMapper profiles — no domain logic, no direct EF Core DbContext usage
- [X] T019 [P] [US3] Verify `backend/src/backend.EntityFrameworkCore/` contains only DbContext, Configurer, migrations, repository base, and seed classes — no business logic
- [X] T020 [P] [US3] Verify `backend/src/backend.Web.Core/` contains only JWT configuration, external auth providers, base controller, and auth models — no business logic
- [X] T021 [P] [US3] Verify `backend/src/backend.Web.Host/` contains only startup wiring, host controllers (`HomeController`, `AntiForgeryController`), `appsettings.json`, and logging config — no business logic
- [X] T022 [US3] Confirm one-way dependency direction is intact: check that no project file in `backend.Core` references `backend.Application`, `backend.EntityFrameworkCore`, `backend.Web.Core`, or `backend.Web.Host` (read each `.csproj` `<ProjectReference>` list) (depends on T017–T021)

**Checkpoint**: User Story 3 complete — layer compliance confirmed, structure ready to scale.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and final validation across all stories.

- [X] T023 [P] Verify `backend/src/backend.Web.Host/appsettings.json` contains no hard-coded credentials — `SecurityKey`, `Username`, and `Password` values MUST be placeholders or loaded from environment; document expected environment variable names in a comment or README section
- [X] T024 Update [quickstart.md](quickstart.md) with any deviations discovered during implementation (exact Npgsql version used, PostgreSQL version tested, dev-certs command if needed)
- [X] T025 Run `dotnet build` one final time from `backend/` and confirm `Build succeeded. 0 Error(s)` with the regenerated migration files present
- [X] T026 Create `.github/workflows/backend-build.yml` — a minimal GitHub Actions workflow that triggers on `push` and `pull_request` to any branch, checks out the repo, sets up .NET 9 SDK, runs `dotnet restore` and `dotnet build --no-restore` from `backend/`, and exits non-zero on any build failure; satisfies the constitution Principle V CI/CD MUST (no secrets or deployment steps required for this initial workflow)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001–T004 have no inter-dependencies — start all in parallel immediately
- **Foundational (Phase 2)**: T005 depends on Phase 1 completing; T006 depends on T005; T007 depends on T006 — strictly sequential
- **User Story 1 (Phase 3)**: Depends on Phase 2 (T007); T008–T012 are sequential within the story
- **User Story 2 (Phase 4)**: Depends on T008 (migrator run); T013–T016 can begin in parallel with Phase 3 after T008 completes
- **User Story 3 (Phase 5)**: Depends only on the solution existing (T007); T017–T021 are fully parallel; T022 depends on T017–T021
- **Polish (Phase 6)**: Depends on all user stories being verified; T026 (CI/CD workflow) can be written any time after T006 (clean build confirmed) and does not block other tasks

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependency on US2 or US3
- **US2 (P2)**: Can start after T008 — shares the migrator run with US1 but verifies independently
- **US3 (P3)**: Can start after T007 — structure verification is independent of DB connectivity

### Within Each User Story

- Models before services → services before endpoints (standard layering order)
- Each story's verification is self-contained — completing one does not require another

### Parallel Opportunities

- T001, T002, T003, T004: All in parallel (different files)
- T017, T018, T019, T020, T021: All in parallel (different project folders)
- T013, T014, T015: All in parallel (read-only SQL queries against the same database)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (T001–T004) in parallel
2. Complete Phase 2 (T005 → T006 → T007) sequentially
3. Complete Phase 3: User Story 1 (T008 → T012)
4. **STOP AND VALIDATE**: Swagger loads, health check passes, auth returns token
5. Backend is running and ready for feature development

### Incremental Delivery

1. Setup + Foundational → PostgreSQL migration files in place
2. User Story 1 → Running API with Swagger (MVP)
3. User Story 2 → Database schema and seed data verified
4. User Story 3 → Architecture compliance confirmed
5. Polish → Docs updated, final clean build

---

## Notes

- No test tasks generated — not requested in the feature specification
- [P] tasks operate on different files and have no blocking dependencies between them
- [US] label maps each task to its user story for traceability
- Commit after T006 (clean build), after T007 (migrations regenerated), and after each story checkpoint
- The `admin` / `123qwe` default credentials are ABP Zero dev defaults — change before any shared environment
