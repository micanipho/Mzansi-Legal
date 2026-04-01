# Contract: Legislation Seed Manifest

**Feature**: 012-legislation-seed-data
**Date**: 2026-03-28

This document defines the contracts that the seed process must satisfy. These are not API contracts (no new endpoints are introduced) but behavioural contracts that implementation must honour and tests must verify.

---

## Contract 1: Category Seed Contract

The seed process MUST produce exactly 9 categories. Any implementation that creates a different number is non-conformant.

**Expected state after seed (idempotent)**:

| Name | Domain | SortOrder |
|------|--------|-----------|
| Employment & Labour | Legal | 1 |
| Housing & Eviction | Legal | 2 |
| Consumer Rights | Legal | 3 |
| Debt & Credit | Legal | 4 |
| Privacy & Data | Legal | 5 |
| Safety & Harassment | Legal | 6 |
| Insurance & Retirement | Financial | 7 |
| Tax | Financial | 8 |
| Contract Analysis | Legal | 9 |

**Idempotency guarantee**: Running the seeder N times must produce exactly 9 categories — never more, never fewer (assuming no manual deletions).

---

## Contract 2: LegalDocument Stub Contract

The seed process MUST register exactly 13 `LegalDocument` records with `IsProcessed = false` (prior to ETL). Uniqueness is enforced by `(ShortName, Year)`.

**Required fields at stub creation**:

| Field | Required | Notes |
|-------|----------|-------|
| Title | Yes | Full official name |
| ShortName | Yes | Abbreviation (used as idempotency key) |
| ActNumber | Yes | Official act number, or `"N/A"` for guides/materials |
| Year | Yes | Year of enactment or publication |
| FileName | Yes | Expected PDF filename (must match file on disk) |
| CategoryId | Yes | FK to the corresponding seeded Category |
| IsProcessed | Yes | Must be `false` at stub creation |
| TotalChunks | Yes | Must be `0` at stub creation |

---

## Contract 3: ETL Ingestion Contract (per document)

For each document where the PDF file exists locally, the seed process MUST invoke the ETL pipeline and produce:

| Outcome | Expected |
|---------|---------|
| IngestionJob created | 1 per document |
| IngestionJob.Status | `Completed` on success, `Failed` on error |
| DocumentChunk records | ≥ 1 per successfully processed document |
| ChunkEmbedding records | 1 per DocumentChunk |
| LegalDocument.IsProcessed | `true` after successful ETL |
| LegalDocument.TotalChunks | Count of DocumentChunk records created |

---

## Contract 4: File Lookup Convention

PDF files MUST be resolvable by the existing `FindSeedDataFile` lookup in `EtlPipelineAppService`:

- Legal documents (Domain = Legal): `{repo-root}/seed-data/legislation/{FileName}`
- Financial documents (Domain = Financial): `{repo-root}/seed-data/financial/{FileName}`

Files NOT present on disk MUST be skipped with a logged warning. The seed process MUST NOT throw on a missing file — it logs and continues.

---

## Contract 5: Error Isolation Contract

A failure to process document N MUST NOT prevent documents N+1 through 13 from being processed.

Per-document error format in logs:
```
[WARN] LegislationIngestionRunner: Failed to ingest '{DocumentTitle}' — {ExceptionMessage}
```

Per-document success format in logs:
```
[INFO] LegislationIngestionRunner: '{DocumentTitle}' — {ChunksLoaded} chunks, {EmbeddingsGenerated} embeddings
```

---

## Contract 6: Idempotency Contract

| Scenario | Expected Behaviour |
|----------|--------------------|
| Category already exists (same Name) | Skip insert, no duplicate |
| LegalDocument already exists (same ShortName + Year) | Skip insert, no duplicate |
| LegalDocument.IsProcessed = true | Skip ETL pipeline call |
| IngestionJob already Completed for document | ETL service's active-job guard prevents duplicate; seeder checks IsProcessed first so ETL is never called |

---

## Contract 7: Admin Session Contract (Auth)

The ETL pipeline MUST be invoked in the context of the host admin user (Id = 1). The seeder MUST use `IAbpSession.Use(tenantId: null, userId: 1)` before calling `IEtlPipelineAppService.TriggerAsync`. This user is guaranteed to exist after `HostRoleAndUserCreator.Create()` runs in Phase A.
