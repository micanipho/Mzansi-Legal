# Research: OpenAI Embedding Service

**Feature**: 009-openai-embedding-service
**Date**: 2026-03-28

---

## Decision 1: Layer Placement

**Decision**: `EmbeddingAppService` lives in `backend.Application/Services/EmbeddingService/`.

**Rationale**: The existing `PdfIngestionAppService` (feature 008) sets the precedent — services that call external tools or APIs belong in the Application layer. There is no separate Infrastructure project in this ABP Zero solution; the Application layer is the correct home for orchestration and external integrations. The domain layer (`backend.Core`) must remain free of HTTP or I/O concerns.

**Alternatives considered**:
- Domain service (`backend.Core`) — rejected; domain layer MUST NOT reference HTTP or external APIs per `docs/BACKEND_STRUCTURE.md`.
- Separate Infrastructure project — rejected; no such project exists in this solution and adding one would be a significant structural change for a single service.

---

## Decision 2: HTTP Client Strategy

**Decision**: Inject `IHttpClientFactory` and create a named client in `backendApplicationModule.cs`. Use `System.Net.Http.Json` for request serialisation/deserialisation.

**Rationale**: `IHttpClientFactory` is the ABP/ASP.NET Core recommended approach for managing `HttpClient` lifecycles (avoids socket exhaustion). It allows the named client to be pre-configured with a base address and default headers in the module once, keeping the service lean. `System.Net.Http.Json` is available in .NET 9 and eliminates boilerplate JSON serialisation.

**Alternatives considered**:
- Raw `new HttpClient()` — rejected; creates socket exhaustion in long-lived services.
- Refit or RestSharp — rejected; third-party dependency not justified for a single-endpoint integration.
- `HttpClient` injected directly — acceptable, but less testable than factory-based approach.

---

## Decision 3: Configuration Binding

**Decision**: Read `OpenAI:ApiKey` and `OpenAI:EmbeddingModel` via `IConfiguration` injected directly into `EmbeddingAppService`. Validate both values in the service constructor using `Guard.Against.NullOrWhiteSpace`.

**Rationale**: No `IOptions<T>` usage exists elsewhere in the project. Injecting `IConfiguration` directly is consistent with the existing ABP patterns in this codebase. Constructor-time validation ensures configuration failures are surfaced at startup rather than on first use.

**Alternatives considered**:
- Strongly typed `IOptions<OpenAiOptions>` — clean but inconsistent with existing codebase patterns; deferred to a future refactor.
- Environment variables only — rejected; `appsettings.json`-based config is the project standard.

---

## Decision 4: CosineSimilarity Placement

**Decision**: Create a static class `EmbeddingHelper` (in `backend.Application/Services/EmbeddingService/EmbeddingHelper.cs`) with a `CosineSimilarity(float[] a, float[] b)` static method.

**Rationale**: Mirrors the `PdfChunkingHelper` static helper pattern introduced in feature 008. Keeps the service class focused on the API call while isolating the pure mathematical computation in a separately testable unit.

**Alternatives considered**:
- Static method directly on `EmbeddingAppService` — possible, but mixes infrastructure and math concerns.
- Extension methods on `float[]` — less discoverable; a named helper is more explicit.

---

## Decision 5: Token-Limit Truncation

**Decision**: Truncate input text to 30,000 characters before sending to the API. Character count is used as the proxy; no tokeniser library is added.

**Rationale**: `text-embedding-ada-002` supports ~8,191 tokens (~32,000 characters). Using 30,000 characters as a safe ceiling is consistent with the spec. Adding a tokeniser library (e.g., `Microsoft.ML.Tokenizers`) for precise token counting is disproportionate for this milestone; character-count truncation is sufficient for legislation chunks which are already section-sized.

**Alternatives considered**:
- Tokeniser-based truncation — more precise but requires an additional NuGet dependency not yet in the project.
- No truncation — rejected; API calls would fail silently or throw for oversized inputs.

---

## Decision 6: OpenAI API Contract

**Decision**: Call `POST https://api.openai.com/v1/embeddings` with `{ "input": "<text>", "model": "<model>" }`. Parse the `data[0].embedding` field of the response as `float[]`.

**Rationale**: This is the standard OpenAI embeddings endpoint used by `text-embedding-ada-002`. No SDK needed — a single `HttpPost` with JSON serialisation covers the full integration.

**Request body**:
```json
{ "input": "...", "model": "text-embedding-ada-002" }
```

**Response shape** (relevant fields only):
```json
{
  "data": [
    { "embedding": [0.123, -0.456, ...], "index": 0 }
  ],
  "model": "text-embedding-ada-002",
  "usage": { "prompt_tokens": 8, "total_tokens": 8 }
}
```

**Alternatives considered**:
- `OpenAI` .NET SDK (community or official) — adds a NuGet dependency; direct REST call is simpler and more explicit for a single endpoint.
- Azure OpenAI endpoint — swappable by changing base URL; not in scope for this milestone.

---

## Decision 7: Error Handling

**Decision**: Throw `InvalidOperationException` for configuration errors (null/empty API key or model), `ArgumentException` for invalid inputs (null or empty text), and let `HttpRequestException` propagate naturally for network/API failures.

**Rationale**: Follows the `PdfIngestionAppService` approach of propagating exceptions to the caller rather than swallowing them. ABP's global exception handler will convert unhandled exceptions into consistent error envelopes at the API boundary.

---

## Decision 8: Testing

**Decision**: Unit-test `EmbeddingHelper.CosineSimilarity` directly (pure math, no mocking needed). Integration-test `EmbeddingAppService` by mocking `IHttpClientFactory` (or using `MockHttpMessageHandler`).

**Rationale**: The existing test projects use `xUnit` (via ABP test helpers). `EmbeddingHelper` can be tested without mocking. The service's HTTP dependency should be tested with a handler mock to avoid real API calls in CI.

---

## No NEEDS CLARIFICATION Items

All technical unknowns were resolved during research. No items require clarification before implementation.
