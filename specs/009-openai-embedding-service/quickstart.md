# Quickstart: OpenAI Embedding Service

**Feature**: 009-openai-embedding-service
**Date**: 2026-03-28

---

## Prerequisites

- ABP backend project is running (feature 003-abp-backend-setup complete)
- `ChunkEmbedding` entity and migration already applied (feature 004-rag-domain-model)
- An OpenAI API key with access to `text-embedding-ada-002`

---

## 1. Add Configuration

In `backend/src/backend.Web.Host/appsettings.json`, add the `OpenAI` section:

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-...",
    "EmbeddingModel": "text-embedding-ada-002"
  }
}
```

**Never commit real API keys.** Use `appsettings.Development.json` (git-ignored) or environment variables for local development.

---

## 2. Register the HTTP Client

In `backend/src/backend.Application/backendApplicationModule.cs`, inside `PreInitialize()` or `Initialize()`:

```csharp
services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

---

## 3. New Files to Create

```
backend/src/backend.Application/Services/EmbeddingService/
├── IEmbeddingAppService.cs       # Interface
├── EmbeddingAppService.cs        # Implementation
├── EmbeddingHelper.cs            # Static CosineSimilarity + TruncateToLimit
└── DTO/
    └── EmbeddingResult.cs        # Output model
```

---

## 4. Verify It Works

Run the unit tests for `EmbeddingHelper`:

```bash
cd backend
dotnet test test/backend.Tests/backend.Tests.csproj --filter "EmbeddingHelper"
```

Expected: cosine similarity of two identical vectors returns `1.0 ± 0.001`.

For a live integration smoke test, call `GenerateEmbeddingAsync` in a test and assert:
- `result.Vector.Length == 1536`
- All values are in `[-1.0, 1.0]`
- Cosine similarity of the vector with itself is `≈ 1.0`

---

## 5. Using the Service in the Pipeline

The `PdfIngestionAppService` (or the caller responsible for the Loading stage) injects `IEmbeddingAppService` and calls it per chunk:

```csharp
var result = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);

var embedding = new ChunkEmbedding
{
    ChunkId = chunk.Id,
    Vector  = result.Vector
};
await _chunkEmbeddingRepository.InsertAsync(embedding);
```

After all chunks are embedded, update the `IngestionJob` to `Completed`.

---

## Cost Reference

| Unit | Approximate Cost (USD) |
|------|----------------------|
| 1 chunk (~500 tokens) | ~$0.0001 |
| 1,000 chunks | ~$0.10 |
| Full Act (100 chunks) | ~$0.01 |

Costs are based on OpenAI's `text-embedding-ada-002` pricing at time of writing. Verify current pricing before large batch runs.
