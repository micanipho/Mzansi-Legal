# Contract: QA RAG Service Endpoint

**Feature**: `feat/014-rag-qa-service` | **Date**: 2026-03-30

## HTTP Endpoint

```
POST /api/app/qa/ask
Authorization: Bearer {jwt-token}
Content-Type: application/json
```

### Request Body

```json
{
  "questionText": "Can my landlord evict me?"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `questionText` | string | Yes | Non-empty, max 30,000 characters |

### Success Response — Answer Found (HTTP 200)

```json
{
  "answerText": "Your landlord cannot evict you without a court order. Section 26(3) of the Constitution states that no one may be evicted from their home without an order of court made after considering all relevant circumstances.",
  "isInsufficientInformation": false,
  "citations": [
    {
      "chunkId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "actName": "Constitution of the Republic of South Africa",
      "sectionNumber": "§ 26(3)",
      "excerpt": "No one may be evicted from their home, or have their home demolished, without an order of court made after considering all the relevant circumstances.",
      "relevanceScore": 0.91
    }
  ],
  "chunkIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"],
  "answerId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

### Success Response — Insufficient Information (HTTP 200)

Returned when no `DocumentChunk` embedding scores ≥ 0.7 against the question embedding.

```json
{
  "answerText": null,
  "isInsufficientInformation": true,
  "citations": [],
  "chunkIds": [],
  "answerId": null
}
```

### Error Responses

| HTTP Status | Condition |
|-------------|-----------|
| 400 Bad Request | `questionText` is null, empty, or exceeds 30,000 characters |
| 401 Unauthorized | No valid JWT token provided |
| 500 Internal Server Error | OpenAI API unreachable or returned an error |

---

## RAG Prompt Contract

The following defines the exact prompt structure sent to the LLM. This is the full citation contract required by Constitution Gate 6.

### System Message

```
You are a South African legal and financial assistant. Your role is to help South African residents understand their legal rights and obligations.

CRITICAL RULES — follow these without exception:
1. You MUST ONLY answer using information from the legislation context provided below.
2. You MUST ALWAYS include a citation for every claim you make, in the format: [Act Name, Section X].
3. If the context does not contain sufficient information to answer the question, you MUST respond with exactly: "I don't have enough information in the available legislation to answer this question."
4. Do NOT speculate, infer, or draw on general knowledge outside the provided context.
5. Write in plain, accessible English. Avoid legal jargon where a simpler word exists.
```

### Context Block (injected per retrieved chunk)

```
[Constitution of the Republic of South Africa — § 26(3)]
No one may be evicted from their home, or have their home demolished, without an order of court made after considering all the relevant circumstances. No legislation may permit arbitrary evictions.

[Prevention of Illegal Eviction from and Unlawful Occupation of Land Act 19 of 1998 — § 4(1)]
Notwithstanding anything to the contrary contained in any law or the common law, the owner or person in charge of land may institute proceedings for the eviction of an unlawful occupier of that land...
```

### User Turn

```
Question: {questionText}

Answer (with citations):
```

### Fallback Behaviour

When `isInsufficientInformation` is `true`:
- No LLM call is made (short-circuit before the API call).
- `answerText` is `null`.
- No `Conversation`, `Question`, `Answer`, or `AnswerCitation` records are persisted.
- The caller receives HTTP 200 with the insufficient-information payload.

---

## Authentication

- All requests MUST include a valid ABP JWT token in the `Authorization: Bearer` header.
- The token is obtained via `POST /api/TokenAuth/Authenticate`.
- The `Answer` and `Conversation` entities are scoped to the authenticated user (`AbpSession.UserId`).
- Unauthenticated requests return HTTP 401 (enforced by `[AbpAuthorize]` on `QaController`).

---

## Non-Goals (Explicitly Out of Scope for This Feature)

- Multilingual question input (Zulu, Sesotho, Afrikaans) — future milestone
- Voice input / audio file attachment — future milestone
- Streaming responses (SSE) — future milestone
- Per-conversation session continuity (multi-turn) — future milestone
- Admin answer review endpoint — future milestone
