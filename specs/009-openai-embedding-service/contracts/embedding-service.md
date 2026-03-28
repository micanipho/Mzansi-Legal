# Contract: IEmbeddingAppService

**Feature**: 009-openai-embedding-service
**Layer**: Application (`backend.Application/Services/EmbeddingService/`)
**Date**: 2026-03-28

---

## Interface

```csharp
namespace backend.Services.EmbeddingService;

public interface IEmbeddingAppService
{
    /// <summary>
    /// Generates a 1,536-dimensional embedding vector for the provided text.
    /// Text exceeding 30,000 characters is silently truncated before the API call.
    /// </summary>
    /// <param name="text">The plain-text content to embed. Must not be null or empty.</param>
    /// <returns>
    /// An EmbeddingResult containing the float[1536] vector and diagnostic metadata.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when text is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    /// <exception cref="HttpRequestException">Propagated on network or API failure.</exception>
    Task<EmbeddingResult> GenerateEmbeddingAsync(string text);
}
```

---

## Static Helper

```csharp
namespace backend.Services.EmbeddingService;

public static class EmbeddingHelper
{
    /// <summary>
    /// Computes the cosine similarity between two float vectors of equal length.
    /// Returns a value in the range [-1.0, 1.0] where 1.0 = identical direction.
    /// </summary>
    /// <param name="a">First vector. Must have the same length as b.</param>
    /// <param name="b">Second vector. Must have the same length as a.</param>
    /// <returns>Cosine similarity scalar.</returns>
    /// <exception cref="ArgumentException">Thrown when vectors have different lengths or are null.</exception>
    public static float CosineSimilarity(float[] a, float[] b);

    /// <summary>
    /// Truncates text to maxCharacters if it exceeds the limit.
    /// Returns the original string if within the limit.
    /// </summary>
    public static string TruncateToLimit(string text, int maxCharacters = 30_000);
}
```

---

## EmbeddingResult DTO

```csharp
namespace backend.Services.EmbeddingService.DTO;

public class EmbeddingResult
{
    /// <summary>The 1,536-element embedding vector.</summary>
    public float[] Vector { get; init; }

    /// <summary>Model name echoed from the API response (e.g., "text-embedding-ada-002").</summary>
    public string Model { get; init; }

    /// <summary>Character count of the text after truncation.</summary>
    public int InputCharacterCount { get; init; }
}
```

---

## Caller Responsibilities

| Responsibility | Owner |
|---|---|
| Provide non-null, non-empty text | Caller |
| Validate `Vector.Length == ChunkEmbedding.EmbeddingDimension` before persisting | Caller |
| Set `ChunkEmbedding.Vector = result.Vector` and persist | Caller |
| Update `IngestionJob` to `Completed` after all chunks are embedded | Caller |
| Handle `HttpRequestException` for retry logic | Caller |

---

## Configuration Contract

The service reads the following keys from `IConfiguration` at construction time. Both keys must be present and non-empty:

| Key | Example Value | Required |
|-----|--------------|----------|
| `OpenAI:ApiKey` | `sk-proj-...` | Yes |
| `OpenAI:EmbeddingModel` | `text-embedding-ada-002` | Yes |

Missing or empty values raise `InvalidOperationException` with a descriptive message before any API call is attempted.

---

## External API Contract

**Endpoint**: `POST https://api.openai.com/v1/embeddings`

**Headers**:
- `Authorization: Bearer {ApiKey}`
- `Content-Type: application/json`

**Request**:
```json
{ "input": "<truncated text>", "model": "text-embedding-ada-002" }
```

**Response** (fields consumed):
```json
{
  "data": [{ "embedding": [<1536 floats>], "index": 0 }],
  "model": "text-embedding-ada-002"
}
```

**Error behaviour**: Non-2xx HTTP responses propagate as `HttpRequestException`. The service does NOT retry — retries are the caller's responsibility.

---

## Dependency Registration

Register in `backendApplicationModule.cs`:

```csharp
// Named HttpClient for OpenAI
services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

`EmbeddingAppService` is registered automatically by ABP's convention-based DI as the transient implementation of `IEmbeddingAppService`.
