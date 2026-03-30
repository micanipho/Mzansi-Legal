# Feature Specification: Deploy Backend to Railway via Docker

**Feature Branch**: `013-railway-docker-deploy`
**Created**: 2026-03-30
**Status**: Draft
**Input**: User description: "I want to deploy my backend to railway using docker"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Initial Backend Deployment to Railway (Priority: P1)

As a developer, I want to deploy the Mzansi Legal backend to Railway so that the application is accessible in a live hosted environment without managing my own infrastructure.

**Why this priority**: This is the foundational step — nothing else works until the backend is running in Railway. Completing this alone gives a fully functional hosted backend.

**Independent Test**: Can be fully tested by visiting the Railway-hosted service URL and receiving a valid HTTP response from the backend, delivering a live production-ready deployment.

**Acceptance Scenarios**:

1. **Given** the backend code is in a git repository with a Dockerfile, **When** the project is connected to Railway and deployed, **Then** the backend service starts and is reachable at a Railway-assigned public URL.
2. **Given** the backend service is deployed, **When** a health-check or API endpoint is called, **Then** a successful response is returned within an acceptable time frame.
3. **Given** a new commit is pushed to the main branch, **When** Railway detects the change, **Then** the service is automatically rebuilt and redeployed without manual intervention.

---

### User Story 2 - Environment Variables and Secrets Configuration (Priority: P2)

As a developer, I want to configure environment variables (database connection strings, API keys, etc.) securely in Railway so that the backend connects to its dependencies without exposing secrets in code.

**Why this priority**: The backend cannot function correctly without its configuration — database connectivity, external API credentials, and feature flags all depend on proper environment variable setup.

**Independent Test**: Can be fully tested by deploying the backend and confirming that it successfully connects to the PostgreSQL database and any external services, verified by a successful API response that exercises those connections.

**Acceptance Scenarios**:

1. **Given** environment variables are configured in Railway's dashboard, **When** the backend starts, **Then** it reads those values and connects to the database without errors.
2. **Given** a secret value (e.g., API key) is set in Railway, **When** the application logs are inspected, **Then** the secret value is never exposed in plain text.

---

### User Story 3 - Persistent Database Connectivity (Priority: P3)

As a developer, I want the deployed backend to connect to a persistent PostgreSQL database (existing or Railway-provisioned) so that data survives container restarts and redeployments.

**Why this priority**: Stateless container restarts must not cause data loss; ensuring the backend talks to a persistent data store is essential for production readiness.

**Independent Test**: Can be fully tested by creating a record via the API, redeploying the service, and then retrieving the same record — confirming data persists across deployments.

**Acceptance Scenarios**:

1. **Given** the backend is connected to a PostgreSQL database, **When** data is written through the API, **Then** the data is stored persistently and survives a service restart or redeployment.
2. **Given** the database connection string changes (e.g., credentials rotation), **When** the environment variable is updated in Railway, **Then** the backend reconnects without requiring code changes.

---

### Edge Cases

- What happens when the Docker build fails due to a missing dependency or misconfigured Dockerfile?
- How does the system handle environment variables that are missing at startup (backend should fail fast with a clear error message rather than silently misbehaving)?
- What happens if the Railway service runs out of memory or CPU limits are exceeded?
- How does the system behave if the database is temporarily unreachable during startup?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The backend MUST be containerised with a Dockerfile that produces a runnable image of the application.
- **FR-002**: The Railway project MUST be configured to build and deploy the backend using the Dockerfile.
- **FR-003**: The deployed service MUST expose at least one publicly reachable HTTP endpoint (e.g., health check) that confirms the backend is running.
- **FR-004**: All runtime configuration (database URL, API keys, secrets) MUST be supplied via environment variables configured in Railway — no secrets in source code.
- **FR-005**: The backend MUST successfully connect to its PostgreSQL database on startup using the configured connection string.
- **FR-006**: Railway MUST be configured to automatically redeploy the service when changes are pushed to the target branch.
- **FR-007**: The deployment pipeline MUST complete (build + deploy) within a reasonable time so developers receive timely feedback.
- **FR-008**: The deployed backend MUST run database migrations on startup (or as part of the deployment process) so the schema is always up to date.

### Key Entities

- **Railway Service**: The hosted container instance running the backend application, with assigned public URL and environment configuration.
- **Dockerfile**: The container build definition that packages the backend application and its runtime dependencies into a deployable image.
- **Environment Variable Set**: The collection of secrets and configuration values (DB connection string, API keys) stored securely in Railway and injected at runtime.
- **PostgreSQL Database**: The persistent data store the backend connects to — either an existing external instance or one provisioned within Railway.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The backend service starts successfully in the hosted environment within 5 minutes of a deployment being triggered.
- **SC-002**: All existing API endpoints return correct responses when called against the Railway-hosted URL, with no functional regression from the local environment.
- **SC-003**: A new deployment triggered by a code push completes automatically without any manual steps by the developer.
- **SC-004**: The backend remains continuously available for at least 24 hours after initial deployment without crashes or restarts caused by configuration errors.
- **SC-005**: No application secrets or credentials appear in source code, build logs, or public-facing responses.

## Assumptions

- The backend already has a working local Docker setup or the codebase is ready to be Dockerised.
- The target Railway environment will be a new project (not an existing one with conflicting configuration).
- The PostgreSQL database used in production is either an existing external instance (e.g., the current Docker-based `mzansi-pg`) or a Railway-provisioned PostgreSQL plugin — decision to be made during planning.
- Railway's free or hobby tier is sufficient for the initial deployment; scaling requirements are out of scope for this feature.
- Automatic deployments on push to the `main` branch is the desired CI/CD trigger; branch-based preview environments are out of scope.
- The backend exposes a `/health` or equivalent endpoint (or one will be added) to serve as the deployment verification target.
- EF Core migrations will be applied automatically at application startup using `database.MigrateAsync()` or equivalent.
