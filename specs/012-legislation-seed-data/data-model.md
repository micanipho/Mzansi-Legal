# Data Model: Legislation Seed Data Pipeline

**Feature**: 012-legislation-seed-data
**Date**: 2026-03-28

---

## Summary

No new database entities or migrations are required for this feature. All schema (Category, LegalDocument, DocumentChunk, ChunkEmbedding, IngestionJob) was established in prior features. This document describes how the seed data maps onto the existing model and what data is created.

---

## Existing Entities Used

### Category (`backend.Domains.LegalDocuments.Category`)

```
Category
├── Id           : Guid         (PK, auto-generated)
├── Name         : string(200)  (unique by convention; checked by seeder)
├── Icon         : string(100)  (set to a placeholder icon key per category)
├── Domain       : DocumentDomain enum (Legal=1 or Financial=2)
├── SortOrder    : int          (1–9 in the order below)
└── [Audit fields from FullAuditedEntity<Guid>]
```

**9 Categories seeded (in sort order)**:

| SortOrder | Name | Domain | Icon |
|-----------|------|--------|------|
| 1 | Employment & Labour | Legal | briefcase |
| 2 | Housing & Eviction | Legal | home |
| 3 | Consumer Rights | Legal | shield |
| 4 | Debt & Credit | Legal | credit-card |
| 5 | Privacy & Data | Legal | lock |
| 6 | Safety & Harassment | Legal | alert-triangle |
| 7 | Insurance & Retirement | Financial | umbrella |
| 8 | Tax | Financial | file-text |
| 9 | Contract Analysis | Legal | clipboard |

---

### LegalDocument (`backend.Domains.LegalDocuments.LegalDocument`)

```
LegalDocument
├── Id             : Guid         (PK, auto-generated)
├── Title          : string(500)  (full official name)
├── ShortName      : string(100)  (abbreviation used in citations)
├── ActNumber      : string(50)   (official act number, or "N/A" for guides)
├── Year           : int          (year of enactment/publication)
├── FileName       : string(300)  (expected PDF filename under seed-data/legislation/ or seed-data/financial/)
├── OriginalPdfId  : Guid?        (null — file is local, not stored in BinaryObject for seed)
├── CategoryId     : Guid         (FK to Category)
├── IsProcessed    : bool         (false on creation; set true after ETL completes)
├── TotalChunks    : int          (0 on creation; updated by ETL pipeline)
├── FullText       : string       (populated by PDF ingestion service during ETL)
└── [Audit fields from FullAuditedEntity<Guid>]
```

**13 LegalDocuments seeded**:

| ShortName | Title | Act No. | Year | FileName | Category | Domain |
|-----------|-------|---------|------|----------|----------|--------|
| Constitution | Constitution of the Republic of South Africa | 108 | 1996 | constitution-1996.pdf | Contract Analysis | Legal |
| BCEA | Basic Conditions of Employment Act | 75 | 1997 | bcea-1997.pdf | Employment & Labour | Legal |
| CPA | Consumer Protection Act | 68 | 2008 | cpa-2008.pdf | Consumer Rights | Legal |
| LRA | Labour Relations Act | 66 | 1995 | lra-1995.pdf | Employment & Labour | Legal |
| POPIA | Protection of Personal Information Act | 4 | 2013 | popia-2013.pdf | Privacy & Data | Legal |
| RHA | Rental Housing Act | 50 | 1999 | rental-housing-act-1999.pdf | Housing & Eviction | Legal |
| PHA | Protection from Harassment Act | 17 | 2011 | protection-harassment-act-2011.pdf | Safety & Harassment | Legal |
| NCA | National Credit Act | 34 | 2005 | nca-2005.pdf | Debt & Credit | Legal |
| FAIS | Financial Advisory and Intermediary Services Act | 37 | 2002 | fais-2002.pdf | Insurance & Retirement | Financial |
| TAA | Tax Administration Act | 28 | 2011 | tax-admin-act-2011.pdf | Tax | Financial |
| PFA | Pension Funds Act | 24 | 1956 | pension-funds-act-1956.pdf | Insurance & Retirement | Financial |
| SARS Guide | SARS Tax Guide | N/A | 2024 | sars-tax-guide-2024.pdf | Tax | Financial |
| FSCA Materials | FSCA Regulatory Framework | N/A | 2024 | fsca-regulatory-2024.pdf | Insurance & Retirement | Financial |

---

### DocumentChunk & ChunkEmbedding

These are created by the ETL pipeline during Phase B of the seed. No additional data modelling is required. The ETL pipeline (`EtlPipelineAppService`) produces:

- One `DocumentChunk` per section/chunk extracted by `PdfIngestionAppService`
- One `ChunkEmbedding` (1536-dimensional float vector) per `DocumentChunk`
- One `IngestionJob` per document (tracks all ETL stages)

Expected outcomes:
- Total chunks across all 13 documents: **500–1,000**
- Total embeddings: **500–1,000** (one per chunk)
- Total ingestion jobs: **13** (one per document)

---

## New Source Files (No DB changes)

| File | Location | Purpose |
|------|----------|---------|
| `LegislationManifest.cs` | `backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/` | Static constant definitions for 9 categories and 13 documents |
| `DefaultCategoriesCreator.cs` | `backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/` | Idempotent Category seed (direct DbContext write) |
| `LegalDocumentRegistrar.cs` | `backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/` | Idempotent LegalDocument stub seed (direct DbContext write) |
| `LegislationIngestionRunner.cs` | `backend.EntityFrameworkCore/EntityFrameworkCore/Seed/Host/` | Phase B: resolves ETL service and triggers pipeline per unprocessed document |

---

## Seed Infrastructure Wiring

### `InitialHostDbBuilder.Create()` — Phase A additions

```
InitialHostDbBuilder.Create()
  ├── DefaultEditionCreator.Create()      [existing]
  ├── DefaultLanguagesCreator.Create()    [existing]
  ├── HostRoleAndUserCreator.Create()     [existing]
  ├── DefaultSettingsCreator.Create()     [existing]
  ├── DefaultCategoriesCreator.Create()   [NEW — idempotent]
  └── LegalDocumentRegistrar.Create()     [NEW — idempotent]
  └── context.SaveChanges()
```

### `SeedHelper.SeedHostDb(IIocResolver)` — Phase B addition

```
SeedHelper.SeedHostDb(IIocResolver iocResolver)
  ├── [existing] WithDbContext → InitialHostDbBuilder.Create() + DefaultTenantBuilder + TenantRoleAndUserBuilder
  └── [NEW] LegislationIngestionRunner(iocResolver).RunAsync()
       ├── Resolve IEtlPipelineAppService
       ├── Resolve IAbpSession → Use(tenantId: null, userId: 1 [admin])
       └── For each LegalDocument where IsProcessed = false AND FileName exists as local file:
             TriggerAsync(document.Id)  → logs result per document
```
