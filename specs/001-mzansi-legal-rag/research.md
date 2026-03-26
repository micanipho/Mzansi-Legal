# Research: MzansiLegal Platform Design

## Phase 0: Outline & Research

### Task 1: Research OpenAI SDK and Embedding Patterns in .NET 8
**Decision**: Use the new official `OpenAI` library (v2+) for OpenAI-compatible services. For the RAG pipeline, use `Azure.AI.OpenAI` or the standard `OpenAI` client if targeting standard OpenAI endpoints. Given the .NET 8 stack, `Microsoft.Extensions.AI` (preview) can be considered for abstraction, but for stability, the official OpenAI client is preferred.
**Rationale**: These libraries are modern, provide clean async/await patterns, and support the required `text-embedding-ada-002` and `gpt-4o` models with high reliability.

### Task 2: Research PdfPig Section-Level Chunking for SA Legislation
**Decision**: Develop a custom `SouthAfricanLegislationParser` that uses PdfPig's `GetWords()` and layout analysis to detect headers (e.g., "CHAPTER 2", "Section 12", "12.") and create chunks bounded by these markers.
**Rationale**: General text splitting is insufficient for legal citations. Structured chunking is necessary to meet the mandatory Act/section citation requirement (FR-003).

### Task 3: Research Whisper/TTS Integration in ABP/ASP.NET Core
**Decision**: Implement `VoiceAppService` in the backend. Use `multipart/form-data` for Whisper transcription and `FileStreamResult` with the appropriate MIME type (e.g., `audio/mpeg`) for TTS playback. TTS output should be cached in local storage or Azure Blob Storage to minimize API costs for repeated common phrases.
**Rationale**: This pattern provides a clean separation of concerns and leverages standard ASP.NET Core file handling features.

### Task 4: Research Next.js 14 Multilingual Voice Interaction
**Decision**: Use the `MediaRecorder` API for audio capture on the client side. Integrate with `next-intl` to manage locale-specific labels and labels for accessibility (ARIA). For the dyslexia mode, use CSS custom properties to switch to the `OpenDyslexic` font and adjust `letter-spacing` globally.
**Rationale**: Native browser APIs provide better performance and compatibility. CSS custom properties allow for a near-instant UI shift without a full reload.

### Task 5: Research In-memory Vector Search for MVP Scale
**Decision**: Use a simple cosine similarity function in C# over `List<float[]>` for the 13 documents (~5k chunks).
**Rationale**: Minimal setup time. Performance will be well within the median 8s latency target for this volume. Migration to `pgvector` will be handled once the knowledge base expands beyond 20 documents.
