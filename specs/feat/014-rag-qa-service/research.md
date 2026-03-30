# Research: RAG Question-Answering Service

**Feature**: `feat/014-rag-qa-service` | **Date**: 2026-03-30

## Decision Log

---

### R-001: Vector Search Strategy — In-Memory vs pgvector

**Decision**: In-memory cosine similarity search

**Rationale**: The legislation corpus is ~1,000 chunks. At 1,536 floats per chunk, the full vector store is approximately 6 MB — negligible for a .NET process. In-memory search eliminates a round-trip to PostgreSQL per query and requires no new infrastructure (pgvector is a PostgreSQL extension that needs explicit installation and EF Core integration). The `EmbeddingHelper.CosineSimilarity` method already exists in the project and is already tested.

**Alternatives considered**:
- *pgvector*: Superior at scale (10k+ chunks); requires `pgvector` extension on the Railway PostgreSQL instance, a new EF Core mapping via `Pgvector.EntityFrameworkCore`, and a migration to change `real[]` → `vector(1536)`. Overkill for MVP.
- *Full DB scan*: Re-loading embeddings from DB on every query; correct but 50–200× slower than in-memory.

---

### R-002: LLM Model — GPT-4o vs GPT-4o-mini

**Decision**: GPT-4o (temperature 0.2) for question answering

**Rationale**: Legal content requires high factual accuracy. GPT-4o produces more reliable citation adherence and better instruction-following than GPT-4o-mini (already used for chunk enrichment). The temperature 0.2 setting (rather than 0.0) preserves natural language readability while staying highly accurate — confirmed appropriate for legal Q&A in OpenAI's guidance.

**Alternatives considered**:
- *GPT-4o-mini*: Faster and cheaper but higher hallucination rate on instruction constraints ("only answer from context"). Risk to citation accuracy unacceptable for legal use.
- *Temperature 0.0*: Maximum determinism; slightly robotic phrasing. 0.2 accepted as the better balance per user requirement.

---

### R-003: Similarity Threshold and Top-K

**Decision**: Threshold 0.7, top-5 chunks

**Rationale**: Cosine similarity of 0.7 is an empirically established minimum for high-precision semantic retrieval — below this threshold, retrieved passages are typically topically adjacent but not directly relevant. Top-5 chunks at ~200 tokens each ≈ 1,000 context tokens, comfortably within GPT-4o's 128k context window while staying focused. The existing `EmbeddingHelper.CosineSimilarity` returns values in [-1, 1]; the 0.7 threshold is directly applicable.

**Alternatives considered**:
- *Threshold 0.75*: Higher precision but risks missing relevant sections when legislation uses formal/archaic language. 0.7 gives better recall.
- *Top-10*: More context but increases prompt length, LLM cost, and risk of diluting the most relevant chunks.

---

### R-004: Q&A Persistence Strategy

**Decision**: Persist to existing `Conversation → Question → Answer → AnswerCitation` domain entities immediately after generating the answer

**Rationale**: The domain model was specifically designed for this purpose (feature 005). Persisting immediately enables the admin review workflow (`Answer.IsAccurate`), FAQ promotion (`Conversation.IsPublicFaq`), citation traceability (via `AnswerCitation.ChunkId`), and future analytics — all required by the constitution. The answer ID is returned to the caller for referencing.

**Alternatives considered**:
- *No persistence (stateless)*: Simpler service, no DB writes — but loses all admin review, analytics, and audit capability. Not compatible with the constitution's audit requirements.
- *Async persistence (fire-and-forget)*: Returns faster but risks data loss on failure. Not appropriate for a legal platform.

---

### R-005: Startup Embedding Load Mechanism

**Decision**: Load embeddings at application startup via an ABP `IApplicationService`-backed `IHostedService`

**Rationale**: ASP.NET Core's `IHostedService` (`BackgroundService`) is the idiomatic pattern for startup work in .NET 9. It runs before the app serves traffic (when using `RunConsecutively`). ABP's `ITransientDependency` + service resolution from the hosted service keeps DI consistent. This avoids lazy-loading per request and keeps `_loadedChunks` immutable after startup.

**Alternatives considered**:
- *Lazy load on first request*: First user pays the load cost; unacceptable per SC-001 (≤8s) since the first request would take much longer.
- *ABP `IApplicationInitializationContext`*: Valid but tightly coupled to ABP bootstrap; `IHostedService` is more testable.

---

### R-006: Prompt Contract for Citation Grounding

**Decision**: Three-part prompt: (1) system message establishing the assistant identity and citation rules, (2) numbered context blocks per chunk labelled with Act name and section number, (3) user question

**Rationale**: Separating system identity from context grounding and the user question gives GPT-4o clear role boundaries. The system message MUST include the instruction "ONLY answer from the context below" and "ALWAYS include [Act Name, Section X] citations". This is the minimum prompt contract required by constitution gate 6. If the context block is empty, the system message includes "respond with exactly: I don't have enough information to answer this question."

**Alternatives considered**:
- *Single-turn prompt*: Simpler but less reliable instruction-following from GPT-4o.
- *Function calling*: Would enforce structured citation output as JSON — considered but adds latency and complexity. The context-grounded natural-language approach is sufficient for MVP.

---

### R-007: HTTP Call Pattern for OpenAI Chat Completions

**Decision**: Reuse `IHttpClientFactory` with a named `"OpenAI"` client, following the pattern established in `EmbeddingAppService`

**Rationale**: `EmbeddingAppService` already configures an `IHttpClientFactory` client with the OpenAI base URL and API key. Reusing the same factory and configuration pattern avoids duplicating credential wiring. The chat completions endpoint (`/v1/chat/completions`) is on the same base URL.

**Alternatives considered**:
- *OpenAI .NET SDK*: Would simplify the HTTP call but adds a NuGet dependency. The project already uses raw `HttpClient` for embeddings — consistency wins.
- *Shared base service*: Creating a shared `OpenAiClientBase` class for reuse — valid, but scope-creep for this feature. Left for a future refactor milestone.
