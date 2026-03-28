# Data Model: OpenAI Embedding Service

**Feature**: 009-openai-embedding-service
**Date**: 2026-03-28

---

## Overview

This feature introduces **no new domain entities**. The `ChunkEmbedding` entity (introduced in feature 004-rag-domain-model) already models the storage contract for embedding vectors. This service produces the `float[]` vector that populates `ChunkEmbedding.Vector`.

---

## Existing Entities Used

### `ChunkEmbedding` (backend.Core/Domains/LegalDocuments/)

The `EmbeddingAppService` is the producer of vectors stored in this entity.

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Primary key (from `FullAuditedEntity<Guid>`) |
| `ChunkId` | `Guid` | FK → `DocumentChunk.Id` |
| `Vector` | `float[]` | 1,536-element array; mapped to PostgreSQL `real[]` |
| `EmbeddingDimension` | `const int = 1536` | Dimension guard constant |

**Invariant enforced by caller**: `Vector.Length == ChunkEmbedding.EmbeddingDimension` before persisting.

---

## Application-Layer Models (not persisted)

These models are internal to `EmbeddingAppService` and its helpers. They are not DTOs exposed via HTTP.

### `EmbeddingResult`

Represents the output of a single embedding call.

| Field | Type | Notes |
|-------|------|-------|
| `Vector` | `float[]` | The 1,536-element embedding vector |
| `Model` | `string` | The model name used (echoed from API response) |
| `InputCharacterCount` | `int` | Character count after truncation (for diagnostics) |

---

## Internal Request/Response Models (HTTP serialisation only)

Used exclusively within `EmbeddingAppService` to serialise the OpenAI REST call. These are private to the service class.

### `OpenAiEmbeddingRequest`

```json
{
  "input": "...",
  "model": "text-embedding-ada-002"
}
```

### `OpenAiEmbeddingResponse` (relevant fields)

```json
{
  "data": [
    { "embedding": [0.123, -0.456, ...], "index": 0 }
  ],
  "model": "text-embedding-ada-002"
}
```

---

## No Migrations Required

No new tables, columns, or indexes are introduced by this feature. The `ChunkEmbeddings` table and its `real[]` column were created in a prior migration.

---

## Configuration Schema

Added to `backend.Web.Host/appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "EmbeddingModel": "text-embedding-ada-002"
  }
}
```

Both keys are read at service construction time. Missing or empty values raise `InvalidOperationException`.
