# Implementation Plan: Legislation Seed Data Pipeline

**Branch**: `012-legislation-seed-data` | **Date**: 2026-03-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/012-legislation-seed-data/spec.md`

## Summary

Implement an idempotent legislation seed pipeline that pre-populates the system with 9 categories and 13 legislation documents, then runs the full ETL pipeline (extract, transform, embed) for each document. The seed runs as part of the existing `backend.Migrator` process, adding two new phases: a DbContext-level record creator (Phase A) and an IoC-resolved ETL runner (Phase B). No new database migrations or domain entities are required — all schema is already in place.

## Technical Context

**Language/Version**: C# on .NET 9.0
**Primary Dependencies**: ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses, UglyToad.PdfPig, System.Net.Http.Json (in-box)
**Storage**: PostgreSQL 15+ via Npgsql; existing schema (no new migrations)
**Testing**: xUnit via ABP test helpers
**Target Platform**: .NET 9.0 console (Migrator) + ABP Web.Host
**Project Type**: CLI seed step within existing Migrator console app
**Performance Goals**: Seed completes within a reasonable time for a one-time operation (13 documents × ~30–60 s per embedding batch = ~10 min total acceptable)
**Constraints**: Must be idempotent; single document failure must not abort remaining 12; PDF files must be present in `seed-data/legislation/` and `seed-data/financial/`
**Scale/Scope**: 13 documents, 9 categories, 500–1,000 expected chunks

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Layer Gate**: No new entities. Seed classes live in `backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/` (EFCore layer — correct). `LegislationManifest.cs` is a constants file in the same layer.
- [x] **Naming Gate**: New classes follow the existing `{Purpose}Creator` / `{Purpose}Builder` naming convention: `DefaultCategoriesCreator`, `LegalDocumentRegistrar`, `LegislationIngestionRunner`, `LegislationManifest`.
- [x] **Coding Standards Gate**: All planned classes will comply with `docs/RULES.md` — guard clauses via Ardalis, no magic strings (manifest constants), methods within scroll limit, nesting ≤ 2 levels, purpose comments on all public methods.
- [x] **Skill Gate**: No applicable skills for this feature. `add-endpoint` does not apply (no new CRUD service). `follow-git-workflow` and `speckit.implement` are mandatory and will be used.
- [x] **Multilingual Gate**: No user-facing outputs. Seed is operator-facing; console log messages are English-only (acceptable for admin tooling). No translation required.
- [x] **Citation Gate**: No AI-facing endpoints introduced by this feature.
- [x] **Accessibility Gate**: No frontend components introduced by this feature.
- [x] **ETL/Ingestion Gate**: ✅ The existing `EtlPipelineAppService.TriggerAsync` creates an `IngestionJob` per document and tracks all stages (Queued → Extracting → Transforming → Loading → Completed/Failed) with timing, chunk counts, and error messages. The seed runner calls this method — IngestionJob tracking is fully covered.

## Project Structure

### Documentation (this feature)

```text
specs/012-legislation-seed-data/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── seed-manifest-contract.md   # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
backend/
└── src/
    └── backend.EntityFrameworkCore/
        └── EntityFrameworkCore/
            └── Seed/
                └── Host/
                    ├── InitialHostDbBuilder.cs          [MODIFY — add Phase A calls]
                    ├── LegislationManifest.cs           [NEW — constants: 9 categories + 13 documents]
                    ├── DefaultCategoriesCreator.cs      [NEW — idempotent category seed]
                    └── LegalDocumentRegistrar.cs        [NEW — idempotent document stub seed]
    └── backend.EntityFrameworkCore/
        └── EntityFrameworkCore/
            └── Seed/
                ├── SeedHelper.cs                        [MODIFY — add Phase B ETL runner call]
                └── Host/
                    └── LegislationIngestionRunner.cs    [NEW — resolves ETL service, runs per document]

seed-data/
├── legislation/                                         [NEW folder — 8 legal PDFs dropped here]
│   ├── constitution-1996.pdf
│   ├── bcea-1997.pdf
│   ├── cpa-2008.pdf
│   ├── lra-1995.pdf
│   ├── popia-2013.pdf
│   ├── rental-housing-act-1999.pdf
│   ├── protection-harassment-act-2011.pdf
│   └── nca-2005.pdf
└── financial/                                           [NEW folder — 5 financial PDFs dropped here]
    ├── fais-2002.pdf
    ├── tax-admin-act-2011.pdf
    ├── pension-funds-act-1956.pdf
    ├── sars-tax-guide-2024.pdf
    └── fsca-regulatory-2024.pdf

tests/
└── backend.Tests/
    └── Seed/
        └── LegislationSeedTests.cs                     [NEW — unit tests for seeder classes]
```

**Structure Decision**: Single backend project (Option 2 backend only). Seed classes follow the established pattern in `backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/`. No frontend changes. No new migrations.

## Design Decisions (from research.md)

1. **Two-phase seed**: Phase A (DbContext direct writes — categories + document stubs) runs inside `InitialHostDbBuilder`. Phase B (IoC-resolved ETL pipeline) runs in `SeedHelper.SeedHostDb(IIocResolver)` after Phase A.
2. **Auth for ETL**: `LegislationIngestionRunner` uses `IAbpSession.Use(tenantId: null, userId: 1)` to run as the host admin user before calling `IEtlPipelineAppService.TriggerAsync`.
3. **File lookup**: Uses existing `seed-data/legislation/` and `seed-data/financial/` convention already coded in `EtlPipelineAppService.FindSeedDataFile` — no changes to ETL service.
4. **Manifest**: `LegislationManifest.cs` holds all 9 category definitions and 13 document definitions as static readonly records — no JSON parsing, no magic strings.
5. **Idempotency**: Categories checked by Name; Documents checked by (ShortName, Year); ETL skipped when `IsProcessed = true`.
6. **Error isolation**: Per-document try/catch in `LegislationIngestionRunner`; log and continue on failure.

## Complexity Tracking

No Constitution Check violations. No complexity justification required.
