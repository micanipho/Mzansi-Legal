# Implementation Plan: Contract Analysis Domain Model

**Branch**: `007-contract-analysis-domain` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-contract-analysis-domain/spec.md`

## Summary

Create the `ContractAnalysis` and `ContractFlag` domain entities in the ABP backend Core layer, register them in `backendDbContext`, and apply an EF Core migration that produces the `ContractAnalyses` and `ContractFlags` PostgreSQL tables. Two new enums (`ContractType`, `FlagSeverity`) are added alongside the entities; the existing `Language` enum is reused. Application services and API endpoints are explicitly out of scope for this feature.

## Technical Context

**Language/Version**: C# on .NET 9.0 + ABP Zero 10.x
**Primary Dependencies**: Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses
**Storage**: PostgreSQL 15+ via Npgsql
**Testing**: xUnit via ABP test helpers (`backend.Tests`)
**Target Platform**: Linux server (Docker) / Windows dev machine
**Project Type**: Web service — domain model layer (no API endpoints in this feature)
**Performance Goals**: Indexes on `UserId`, `ContractAnalysisId`, and `Severity` to support expected query patterns
**Constraints**: `HealthScore` must be constrained 0–100 at DB level; `UserId` must be non-null FK
**Scale/Scope**: Same scale as existing Q&A domain (per-user record set)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: Both entities in `backend.Core/Domains/ContractAnalysis/` (Core = entity ✅); DbSets in `backendDbContext` (EFCore ✅); Application services deferred to next feature branch per scope decision.
- [x] **Naming Gate**: Entity names `ContractAnalysis`, `ContractFlag`; future service names will follow `ContractAnalysisAppService` / `IContractAnalysisAppService` / `ContractAnalysisDto` conventions per `docs/BACKEND_STRUCTURE.md`.
- [x] **Coding Standards Gate**: All entities extend `FullAuditedEntity<Guid>`; purpose comments on classes and public properties; `[Range]` for magic-number constraint; no duplication; ≤350 lines per file; ≤2 nesting levels.
- [x] **Skill Gate**: `add-endpoint` skill identified for the subsequent CRUD endpoint scaffold. This feature (domain model only) has no applicable skill — manual scaffolding of entity files is the correct approach.
- [x] **Multilingual Gate**: No user-facing outputs in this feature (domain model + migration only). The `Language` enum field stores the contract's language; no localized strings are generated. Gate is N/A for this feature.
- [x] **Citation Gate**: No AI-facing endpoints in this feature. Gate is N/A.
- [x] **Accessibility Gate**: No frontend components in this feature. Gate is N/A.

## Project Structure

### Documentation (this feature)

```text
specs/007-contract-analysis-domain/
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output (/speckit.plan)
├── data-model.md        # Phase 1 output (/speckit.plan)
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── backend.Core/
│   │   └── Domains/
│   │       └── ContractAnalysis/          ← NEW domain folder
│   │           ├── ContractAnalysis.cs    ← NEW entity (aggregate root)
│   │           ├── ContractFlag.cs        ← NEW entity (child of ContractAnalysis)
│   │           ├── ContractType.cs        ← NEW enum
│   │           └── FlagSeverity.cs        ← NEW enum
│   ├── backend.EntityFrameworkCore/
│   │   └── EntityFrameworkCore/
│   │       └── backendDbContext.cs        ← ADD DbSet<ContractAnalysis>, DbSet<ContractFlag>,
│   │                                          ConfigureContractAnalysisRelationships()
│   │       └── Migrations/                ← NEW migration: AddContractAnalysisDomain
│   └── (backend.Application/ — out of scope for this feature)
└── test/
    └── backend.Tests/                     ← Integration tests to verify table creation + constraints
```

**Structure Decision**: Option 2 (web application) — backend only. Frontend is out of scope. New files placed in the existing `Domains/ContractAnalysis/` subdomain folder, following the `LegalDocuments/` and `QA/` precedents.

## Complexity Tracking

> No constitution violations requiring justification.

---

## Implementation Notes

### Entity patterns to follow

- Study `backend/src/backend.Core/Domains/QA/Conversation.cs` and `Answer.cs` for the exact property/annotation/comment pattern to replicate.
- `UserId` is `long` — same as `Conversation.UserId` (ABP Zero User PK).
- `OriginalFileId` is `Guid?` — same as `LegalDocument.OriginalPdfId` (ABP BinaryObject pattern).
- `Language` property uses `backend.Domains.QA.Language` — import that namespace, do not create a duplicate enum.

### DbContext pattern to follow

- Study the existing `backendDbContext.cs` structure for:
  - Comment-header pattern for domain sections (`// ── Legal Documents domain ──`)
  - Private `Configure*Relationships()` method pattern
  - How cascade delete and indexes are registered

### Migration command

```bash
cd backend
dotnet ef migrations add AddContractAnalysisDomain \
  --project src/backend.EntityFrameworkCore \
  --startup-project src/backend.Web.Host
```

Apply with the ABP Migrator or `dotnet run --project src/backend.Migrator`.

### Out of scope (deferred to next feature)

- `ContractAnalysisAppService` and `IContractAnalysisAppService`
- `ContractFlagAppService` and `IContractFlagAppService`
- DTOs and AutoMapper profiles
- REST API endpoints
- Frontend pages
