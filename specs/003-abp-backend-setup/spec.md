# Feature Specification: ABP Backend Foundation Setup

**Feature Branch**: `003-abp-backend-setup`
**Created**: 2026-03-27
**Status**: Draft
**Milestone**: Setup & Data Pipeline

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Developer Runs the Backend Locally (Priority: P1)

A developer clones the repository, configures the local connection string, and is able to build
and start the backend without errors. The running application is reachable and the API explorer
is accessible so the developer can inspect all available endpoints without writing any client
code.

**Why this priority**: Every subsequent feature depends on a running, connected backend. Nothing
else can be built or tested until this baseline works.

**Independent Test**: Clone the repo on a clean machine, set the connection string, run the
application, and confirm the API explorer loads with at least the built-in ABP health and auth
endpoints visible.

**Acceptance Scenarios**:

1. **Given** a developer has checked out the repository and configured a valid database
   connection, **When** they start the application, **Then** the application starts without
   runtime errors and responds to requests within 30 seconds.
2. **Given** the application is running, **When** the developer navigates to the API explorer
   URL, **Then** the API explorer page loads and all registered endpoints are listed and
   accessible for manual invocation.
3. **Given** the application is running, **When** a health check request is issued, **Then** the
   system returns a healthy status indicating the application and database connection are
   operational.

---

### User Story 2 - Developer Applies Migrations and Verifies the Database (Priority: P2)

A developer runs the database migration process and confirms that all required tables and seed
data are created in the PostgreSQL database, making the system ready to persist data.

**Why this priority**: Without a correctly seeded schema the application cannot store or retrieve
any data, blocking all downstream feature work.

**Independent Test**: Point the migrator at a fresh empty database, run the migration tool, then
connect to the database directly and confirm that the expected tables, default roles, and seed
records are present.

**Acceptance Scenarios**:

1. **Given** an empty PostgreSQL database with valid credentials, **When** the migration tool
   runs, **Then** all schema tables are created and the process exits without errors.
2. **Given** migrations have completed, **When** the developer inspects the database, **Then**
   default system roles (Admin, Citizen) and seed settings are present.
3. **Given** the migration has already been applied, **When** the tool runs again, **Then** it
   detects no pending migrations and exits cleanly with no destructive changes.

---

### User Story 3 - Developer Validates Project Folder Structure Compliance (Priority: P3)

A developer or reviewer opens the solution and confirms that every class and module is placed in
the correct layer according to the defined architecture and separation-of-concerns rules, giving
the team confidence that the foundation will not drift over time.

**Why this priority**: Structural correctness now prevents costly refactors later. It gates the
quality of all future features built on top of this foundation.

**Independent Test**: Open the solution in an IDE, review each project layer, and confirm that no
domain logic lives in the application layer, no DTOs are in the domain layer, and every module
is registered according to ABP conventions.

**Acceptance Scenarios**:

1. **Given** the solution is open, **When** a reviewer walks through the project layers, **Then**
   each layer (Domain, Application, Infrastructure, API Host) contains only the artifact types
   permitted by the architecture document.
2. **Given** a new entity needs to be added, **When** the developer follows the existing
   structure, **Then** the correct layer locations are unambiguous and no existing cross-layer
   violations need to be worked around.

---

### Edge Cases

- What happens when the database server is unreachable at startup? The application MUST log a
  clear error and exit rather than hang silently.
- What happens when migrations are run against a database that already has a later schema
  version? The tool MUST detect this and skip gracefully without data loss.
- What happens when the connection string is missing or malformed? The application MUST report
  a meaningful configuration error at startup, not a cryptic runtime exception.
- What happens when the API explorer is accessed before the application has fully started? The
  response MUST be an appropriate service-unavailable message, not a blank page.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The backend MUST be structured as a modular ABP solution with distinct Domain,
  Application, Infrastructure (EF Core / PostgreSQL), Web.Core, and Web.Host layers following
  the architecture defined in `docs/BACKEND_STRUCTURE.md`.
- **FR-002**: The application MUST connect to a PostgreSQL database using a connection string
  supplied via environment configuration, without any database credentials hard-coded in source.
- **FR-003**: The application MUST run EF Core migrations automatically on startup (or via a
  dedicated migration runner) to create or update the schema to the latest version.
- **FR-004**: The application MUST seed default system data (roles, settings, default language)
  on first run as defined by the ABP Zero seed infrastructure.
- **FR-005**: The application MUST expose an API explorer (Swagger / OpenAPI) that lists all
  registered endpoints and allows developers to invoke them manually from a browser.
- **FR-006**: The application MUST include a health check endpoint that returns the operational
  status of the application and the database connection.
- **FR-007**: The application MUST build from source without compilation errors on a clean
  environment with only the required SDK and toolchain installed.
- **FR-008**: The application MUST start and serve requests without runtime exceptions on a
  machine that has a reachable PostgreSQL instance and a valid connection string.
- **FR-009**: All ABP modules (Core, Application, EntityFrameworkCore, Web.Core, Web.Host) MUST
  be registered and wired correctly so that dependency injection resolves all service
  dependencies at startup.
- **FR-010**: Environment-specific settings (connection strings, JWT secrets, CORS origins) MUST
  be configurable via `appsettings.json` overrides without code changes.

### Key Entities

- **Tenant**: Top-level SaaS isolation unit; managed by ABP Zero multi-tenancy infrastructure.
- **User**: Application user; inherits from ABP Zero identity user with extended profile support.
- **Role**: Named permission group assigned to users; default roles (Admin, Citizen) seeded on
  first run.
- **Edition**: SaaS subscription tier; managed by ABP Zero edition infrastructure.
- **Setting**: Key-value configuration record; default settings seeded by ABP Zero on startup.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer who has never worked on the project can have the backend running
  locally within 15 minutes of cloning the repository, following only the setup steps in the
  quickstart guide.
- **SC-002**: The application starts and serves its first request in under 30 seconds on a
  standard development machine.
- **SC-003**: The migration process completes against a fresh database in under 60 seconds and
  leaves the schema in a fully initialized state without manual intervention.
- **SC-004**: 100% of registered endpoints are discoverable in the API explorer immediately
  after startup — no manual endpoint registration steps required.
- **SC-005**: Zero compilation errors and zero runtime exceptions occur on a clean build and
  first run with a valid configuration.
- **SC-006**: A structure compliance review takes under 10 minutes because every artifact is
  in its expected location with no ambiguous placements.

## Assumptions

- The project already has the ABP Zero solution scaffold in place; this feature focuses on
  verifying, configuring, and validating that scaffold rather than generating it from scratch.
- PostgreSQL is available in the target development environment; database server installation
  is out of scope for this feature.
- The JWT security key and SMTP settings do not need to be functional for this setup feature;
  only database connectivity and API surface are validated.
- A single-tenant (host) configuration is sufficient for the initial setup; multi-tenant
  isolation will be validated in later features.
- The API explorer is intended for developer use only and will be disabled or secured in
  production environments via environment configuration.
- No frontend integration is in scope for this feature; the backend is validated standalone.
