# Research: Legislation Seed Data Pipeline

**Feature**: 012-legislation-seed-data
**Date**: 2026-03-28

---

## Decision 1: Where to place the seed classes

**Decision**: New seed classes go in `backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/`, following the existing pattern of `DefaultEditionCreator`, `DefaultLanguagesCreator`, `HostRoleAndUserCreator`, and `DefaultSettingsCreator`.

**Rationale**: The existing `InitialHostDbBuilder.Create()` calls each creator in sequence. Category and document stubs can be seeded at DbContext level without HTTP dependencies. This is the established ABP pattern for host-level seed data.

**Alternatives considered**:
- Standalone console app: rejected — adds an extra project and deployment step; the Migrator already provides this capability.
- Application layer seeder: rejected for the record-creation step — Application services require an ABP session, adding complexity for what is a simple record-insert operation.

---

## Decision 2: How to run the ETL pipeline from seed context (auth challenge)

**Decision**: Split the seed into two phases:
1. **Phase A** (inside `InitialHostDbBuilder`) — `DefaultCategoriesCreator` and `LegalDocumentRegistrar` insert Category and LegalDocument stub records directly via `backendDbContext`. No auth required.
2. **Phase B** (inside `SeedHelper.SeedHostDb(IIocResolver)`) — `LegislationIngestionRunner` resolves `IEtlPipelineAppService` from IoC. Uses `IAbpSession` with `AbpSession.Use(tenantId: null, userId: adminUserId)` to impersonate the admin user (seeded by `HostRoleAndUserCreator`), then calls `TriggerAsync` for each unprocessed document.

**Rationale**: `EtlPipelineAppService` carries `[AbpAuthorize]` at class level (required for production use). ABP's `IAbpSession.Use(...)` is the standard approach for running background/batch operations as a specific user without a real HTTP context. The admin user (Id = 1 from `HostRoleAndUserCreator`) is always available after Phase A.

**Alternatives considered**:
- Remove `[AbpAuthorize]` from ETL service: rejected — weakens production security.
- New internal seeder interface without auth: viable but adds code duplication — the ETL pipeline logic already covers everything we need.
- Use `AbpSession.Override(null)` to bypass auth: ABP doesn't have this — `Use()` is the correct API.

---

## Decision 3: PDF file storage location

**Decision**: Place seed PDFs under `seed-data/legislation/` (legal acts) and `seed-data/financial/` (financial guidance) at the repository root. The existing `EtlPipelineAppService.FindSeedDataFile` method already walks up the directory tree looking for exactly these paths — no code change required.

**Rationale**: `FindSeedDataFile` in `EtlPipelineAppService` already searches for `seed-data/legislation/{fileName}` and `seed-data/financial/{fileName}`. Aligning the storage path with this existing lookup avoids any change to the ETL service.

**Alternatives considered**:
- `docs/legislation/`: requires updating `FindSeedDataFile` or adding a new path to the search list.
- Configurable via `appsettings.json`: the existing walkup-search strategy already provides implicit configurability by convention; explicit config is an over-engineering for MVP.

---

## Decision 4: Document-to-category and filename mapping

**Decision**: Define a static manifest of all 13 documents in a new `LegislationManifest.cs` constants class in the Seed folder. Each entry records: `Title`, `ShortName`, `ActNumber`, `Year`, `FileName`, `CategoryName`, and `Domain`.

| # | Title | ShortName | Act No. | Year | FileName | Category | Domain |
|---|-------|-----------|---------|------|----------|----------|--------|
| 1 | Constitution of the Republic of South Africa | Constitution | 108 | 1996 | constitution-1996.pdf | Contract Analysis | Legal |
| 2 | Basic Conditions of Employment Act | BCEA | 75 | 1997 | bcea-1997.pdf | Employment & Labour | Legal |
| 3 | Consumer Protection Act | CPA | 68 | 2008 | cpa-2008.pdf | Consumer Rights | Legal |
| 4 | Labour Relations Act | LRA | 66 | 1995 | lra-1995.pdf | Employment & Labour | Legal |
| 5 | Protection of Personal Information Act | POPIA | 4 | 2013 | popia-2013.pdf | Privacy & Data | Legal |
| 6 | Rental Housing Act | RHA | 50 | 1999 | rental-housing-act-1999.pdf | Housing & Eviction | Legal |
| 7 | Protection from Harassment Act | PHA | 17 | 2011 | protection-harassment-act-2011.pdf | Safety & Harassment | Legal |
| 8 | National Credit Act | NCA | 34 | 2005 | nca-2005.pdf | Debt & Credit | Legal |
| 9 | Financial Advisory and Intermediary Services Act | FAIS | 37 | 2002 | fais-2002.pdf | Insurance & Retirement | Financial |
| 10 | Tax Administration Act | TAA | 28 | 2011 | tax-admin-act-2011.pdf | Tax | Financial |
| 11 | Pension Funds Act | PFA | 24 | 1956 | pension-funds-act-1956.pdf | Insurance & Retirement | Financial |
| 12 | SARS Tax Guide | SARS Guide | N/A | 2024 | sars-tax-guide-2024.pdf | Tax | Financial |
| 13 | FSCA Regulatory Framework | FSCA Materials | N/A | 2024 | fsca-regulatory-2024.pdf | Insurance & Retirement | Financial |

**Rationale**: A constants class is discoverable, type-safe, and avoids a runtime JSON parsing step. It makes the manifest auditable in version control.

**Alternatives considered**:
- Embedded JSON manifest: adds a file parsing step with potential deserialization errors at startup.
- Hardcoded strings inline in seeder: makes the seeder harder to read and maintain.

---

## Decision 5: Idempotency strategy

**Decision**:
- Categories: check by `Name` (case-insensitive) before inserting.
- LegalDocuments: check by `ShortName + Year` before inserting.
- ETL ingestion: skip documents where `IsProcessed = true`.

**Rationale**: These are the natural unique keys for each entity. Using DB-level upsert would require raw SQL; the EF-level check is safer and consistent with the existing `DefaultEditionCreator` pattern.

---

## Decision 6: Error handling per document

**Decision**: Wrap each document's ETL pipeline call in a `try/catch`. On failure, log the error (document title + exception message) and continue to the next document. The overall seed step is considered successful even if some documents fail to embed — operators can re-run or use the admin UI retry later.

**Rationale**: Embedding failures are often transient (OpenAI rate limits, network timeouts). Blocking the entire seed on a single document failure would leave the system in an unusable state.

---

## Decision 7: Applicable skills (Skill Gate check)

From the Skill Usage Policy, the following apply:
- `add-endpoint` — NOT applicable; no new CRUD endpoint is being scaffolded. `LegislationDataSeeder` is a seed class, not an Application service.
- Other UI skills (`add-list-page`, `add-modal`, etc.) — NOT applicable; no frontend work.
- `follow-git-workflow` — MANDATORY for every feature.
- `speckit.implement` — MANDATORY for executing tasks.

No skill deviation requires documented justification.
