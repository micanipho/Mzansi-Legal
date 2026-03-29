# Feature Specification: OpenAI Embedding Service

**Feature Branch**: `009-openai-embedding-service`
**Created**: 2026-03-28
**Status**: Draft
**Input**: User description: "Build an EmbeddingService that accepts a text string, calls OpenAI text-embedding-ada-002 model via REST API, returns a float[1536] vector, handles token limits, and includes a static CosineSimilarity method."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generate Embedding for Text (Priority: P1)

A developer or automated pipeline submits a legislation chunk (plain text) to the embedding service and receives a 1,536-dimensional numeric vector representing the semantic meaning of that text. This vector is then stored alongside the chunk for later semantic search.

**Why this priority**: Core capability. All downstream semantic search depends on embeddings being generated correctly. Without this, no other functionality in the RAG pipeline works.

**Independent Test**: Can be tested by submitting a sample text string and verifying a 1,536-element float array is returned with no zero vectors.

**Acceptance Scenarios**:

1. **Given** a non-empty text string under 30,000 characters, **When** the embedding service is called, **Then** a float array of exactly 1,536 elements is returned with values between -1.0 and 1.0.
2. **Given** a text string exceeding 30,000 characters, **When** the embedding service is called, **Then** the text is silently truncated to 30,000 characters before the API call and a valid 1,536-element float array is still returned.
3. **Given** a valid configuration with an API key and model name, **When** the embedding service is initialised, **Then** it successfully connects to the external embedding provider without errors.

---

### User Story 2 - Compare Two Vectors for Semantic Similarity (Priority: P2)

A developer or system component compares two previously generated embedding vectors to determine how semantically similar two pieces of text are. The result is a cosine similarity score.

**Why this priority**: Enables the retrieval step of the RAG pipeline. Without similarity comparison, the system cannot rank search results by relevance.

**Independent Test**: Can be tested by passing two identical vectors and verifying the result is ~1.0, and two unrelated text embeddings return a score below 0.5.

**Acceptance Scenarios**:

1. **Given** two identical float vectors of length 1,536, **When** cosine similarity is computed, **Then** the result is approximately 1.0 (within ±0.001 tolerance).
2. **Given** embeddings generated from two semantically unrelated texts, **When** cosine similarity is computed, **Then** the result is below 0.5.
3. **Given** two float arrays of different lengths, **When** cosine similarity is computed, **Then** an appropriate error is raised rather than returning a misleading result.

---

### User Story 3 - Configuration via Settings (Priority: P3)

A system administrator or developer configures the embedding service by providing the API key and model name in the application's configuration file, without modifying source code.

**Why this priority**: Enables environment-specific configuration (dev/staging/prod API keys) and future model swaps without redeployment.

**Independent Test**: Can be tested by changing the model name in configuration and confirming the service uses the updated value in its requests.

**Acceptance Scenarios**:

1. **Given** a configuration file with `OpenAI:ApiKey` and `OpenAI:EmbeddingModel` values, **When** the application starts, **Then** the embedding service reads these values and uses them for all API calls.
2. **Given** a missing or empty `ApiKey` in configuration, **When** the application starts or the service is first called, **Then** a clear configuration error is raised describing what is missing.

---

### Edge Cases

- What happens when the API key is invalid or expired? The service should surface a meaningful error rather than returning a null or empty vector.
- What happens when the external embedding API is temporarily unavailable? The call should fail with a descriptive error rather than hanging indefinitely.
- What happens when an empty string is submitted? The service should reject it with a clear validation error.
- What happens when the text is exactly 30,000 characters? No truncation should occur; the full text is sent.
- What happens when the text is 30,001 characters? Exactly one character is trimmed and the call proceeds normally.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST accept a plain text string as input and return a 1,536-element float vector representing its semantic embedding.
- **FR-002**: The system MUST call the configured embedding model via the external embedding provider REST API to generate vectors.
- **FR-003**: The system MUST truncate any input text exceeding 30,000 characters before sending to the external API.
- **FR-004**: The system MUST expose a utility method that computes the cosine similarity between two float vectors of equal length and returns a scalar value.
- **FR-005**: The system MUST read the API key and model name from application configuration rather than hardcoding credentials.
- **FR-006**: The system MUST raise a clear, descriptive error when the API key is missing or empty at startup or first use.
- **FR-007**: The system MUST raise an error when cosine similarity is requested for two vectors of differing lengths.
- **FR-008**: The system MUST NOT silently return a null or empty vector on API failure; errors must propagate to the caller.

### Key Entities

- **EmbeddingRequest**: Represents a request to convert a text string into a vector; contains the input text (post-truncation) and the model identifier.
- **EmbeddingResult**: Represents the output; contains the 1,536-element float array and metadata such as the model used.
- **EmbeddingConfiguration**: Holds the external API key and model name read from application settings.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The embedding service can generate a vector for any arbitrary text string in under 3 seconds under normal network conditions.
- **SC-002**: Cosine similarity between two embeddings of identical text returns a value of 1.0 ± 0.001.
- **SC-003**: Cosine similarity between embeddings of two semantically unrelated texts returns a value below 0.5.
- **SC-004**: Text inputs exceeding the character limit are processed without error; truncation is transparent to the caller.
- **SC-005**: Changing the model or API key in configuration takes effect on the next application start without code changes.
- **SC-006**: Invalid or missing configuration is detected at startup with a human-readable error message.

## Assumptions

- The external embedding provider used is OpenAI's direct REST API. Swapping to Azure OpenAI is a future concern and is out of scope for this feature.
- The 1,536-dimensional output size is fixed for `text-embedding-ada-002`; other models with different output sizes are not supported in this iteration.
- The 30,000-character truncation limit is a conservative approximation of the model's token limit and is acceptable for legislation chunks in this project.
- Token counting is not performed before truncation; character count is used as a simpler and sufficient proxy.
- The caller is responsible for storing and indexing the returned vector; this service only generates embeddings and does not persist them.
- The service operates in a server-side context within the existing backend; no browser or mobile client calls the embedding API directly.
- Retry logic and circuit-breaking for transient API failures are out of scope; the caller handles retries if needed.
- Cost management (rate limiting, batching) is out of scope; each call is made individually as chunks arrive.
- Blocked by ABP Backend Project Scaffold being in place, as this service will be registered within that project.
