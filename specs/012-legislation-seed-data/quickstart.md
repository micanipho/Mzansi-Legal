# Quickstart: Legislation Seed Data Pipeline

**Feature**: 012-legislation-seed-data
**Date**: 2026-03-28

---

## Prerequisites

1. PostgreSQL running (Docker: `mzansi-pg` container, or local)
2. All prior migrations applied (`backend.Migrator` run at least once)
3. OpenAI API key configured in `backend.Web.Host/appsettings.json` (used by EmbeddingAppService)
4. PDF files placed in the correct seed-data folders (see below)

---

## Step 1: Place the PDF Files

Place all 13 legislation PDFs in the following directories under the repo root:

```
seed-data/
├── legislation/               ← Legal domain PDFs
│   ├── constitution-1996.pdf
│   ├── bcea-1997.pdf
│   ├── cpa-2008.pdf
│   ├── lra-1995.pdf
│   ├── popia-2013.pdf
│   ├── rental-housing-act-1999.pdf
│   ├── protection-harassment-act-2011.pdf
│   └── nca-2005.pdf
└── financial/                 ← Financial domain PDFs
    ├── fais-2002.pdf
    ├── tax-admin-act-2011.pdf
    ├── pension-funds-act-1956.pdf
    ├── sars-tax-guide-2024.pdf
    └── fsca-regulatory-2024.pdf
```

**Source URLs** (all free/public):
- Constitution, BCEA, LRA, CPA, POPIA, NCA, PHA, RHA: https://www.justice.gov.za
- TAA, SARS Guide: https://www.sars.gov.za
- FAIS, PFA, FSCA Materials: https://www.fsca.co.za

---

## Step 2: Run the Migrator

```bash
cd backend
dotnet run --project src/backend.Migrator -- -q
```

The Migrator will:
1. Apply any pending database migrations
2. Seed the 9 categories (idempotent)
3. Register the 13 LegalDocument stubs (idempotent)
4. Run the ETL pipeline for each document whose PDF is present and `IsProcessed = false`

---

## Step 3: Verify the Seed

Connect to the database and run:

```sql
-- Check categories
SELECT name, domain, sort_order FROM "Categories" ORDER BY sort_order;

-- Check documents
SELECT short_name, title, year, is_processed, total_chunks FROM "LegalDocuments" ORDER BY creation_time;

-- Check chunks and embeddings
SELECT d.short_name, COUNT(c.id) AS chunks, COUNT(e.id) AS embeddings
FROM "LegalDocuments" d
LEFT JOIN "DocumentChunks" c ON c.document_id = d.id
LEFT JOIN "ChunkEmbeddings" e ON e.chunk_id = c.id
GROUP BY d.short_name
ORDER BY d.short_name;

-- Check ingestion jobs
SELECT d.short_name, j.status, j.chunks_loaded, j.embeddings_generated, j.error_message
FROM "IngestionJobs" j
JOIN "LegalDocuments" d ON d.id = j.document_id
ORDER BY j.creation_time;
```

**Expected results**:
- 9 categories
- 13 documents with `is_processed = true` (for documents whose PDFs were present)
- 500–1,000 total chunks with matching embeddings
- 13 ingestion jobs with status `Completed`

---

## Re-running the Seed

The seed is fully idempotent. Running the Migrator again will:
- Skip categories that already exist
- Skip LegalDocument stubs that already exist
- Skip documents where `IsProcessed = true`
- Only re-process documents that are still `IsProcessed = false` (e.g., if a PDF was missing on the first run)

---

## Troubleshooting

| Problem | Resolution |
|---------|-----------|
| Document shows `IsProcessed = false` after seed | PDF file was missing or unreadable — check `seed-data/` paths and re-run Migrator |
| Embedding error in logs | OpenAI API key missing or rate-limited — check `appsettings.json`, wait and re-run |
| Zero chunks for a document | PDF is a scanned image with no extractable text — obtain a text-layer PDF |
| Category or document missing | Seed step not running — check that `InitialHostDbBuilder` calls `DefaultCategoriesCreator` and `LegalDocumentRegistrar` |
