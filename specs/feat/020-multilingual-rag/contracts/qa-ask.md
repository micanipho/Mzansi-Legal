# Contract: POST /api/app/qa/ask

**Feature**: feat/020-multilingual-rag  
**Controller**: `QaController` → `RagAppService.AskAsync`  
**Authentication**: Optional (persists history only when authenticated)

---

## Request

```json
POST /api/app/qa/ask
Content-Type: application/json

{
  "questionText": "Ingabe umnikazi wendlu angangixosha?"
}
```

| Field | Type | Required | Constraints | Notes |
|-------|------|----------|-------------|-------|
| `questionText` | string | Yes | 1–30,000 chars | Any supported language; server auto-detects |

No `language` field — language is detected server-side (FR-001, research Decision 6).

---

## Response (success — legislation found)

```json
HTTP 200 OK
{
  "answerText": "Umnikazi wendlu angakuxosha kuphela ...",
  "isInsufficientInformation": false,
  "detectedLanguageCode": "zu",
  "citations": [
    {
      "chunkId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "actName": "Rental Housing Act 50 of 1999",
      "sectionNumber": "§ 4(5)(c)",
      "excerpt": "A landlord may not evict a tenant without ...",
      "relevanceScore": 0.87
    }
  ],
  "chunkIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"],
  "answerId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

## Response (fallback — no legislation found)

```json
HTTP 200 OK
{
  "answerText": "⚠️ *Alukho ulwazi...*\n\n[General AI answer in isiZulu]",
  "isInsufficientInformation": true,
  "detectedLanguageCode": "zu",
  "citations": [],
  "chunkIds": [],
  "answerId": null
}
```

---

## Response Fields (after this feature)

| Field | Type | Notes |
|-------|------|-------|
| `answerText` | string | Response in the detected input language |
| `isInsufficientInformation` | bool | True when no legislation chunks met the similarity threshold |
| `detectedLanguageCode` | string | **NEW** — ISO 639-1 code: "en", "zu", "st", or "af" |
| `citations` | array | Act name and section always in English (FR-006) |
| `chunkIds` | array | UUIDs of the chunks used for this answer |
| `answerId` | uuid \| null | Non-null only when user is authenticated |

---

## Citation Format (unchanged)

All citations appear as `[Act Name, Section X]` inline in `answerText`, regardless of response language. The `citations` array provides structured metadata with the same English identifiers.

---

## Language Behaviour

| Input Language | Detection | Translation | Response |
|----------------|-----------|-------------|----------|
| English | Detected as `en` | No translation | Answer in English |
| isiZulu | Detected as `zu` | Translated to English for search | Answer in isiZulu, citations in English |
| Sesotho | Detected as `st` | Translated to English for search | Answer in Sesotho, citations in English |
| Afrikaans | Detected as `af` | Translated to English for search | Answer in Afrikaans, citations in English |
| Unknown/unsupported | Falls back to `en` | No translation | Answer in English |

---

## Fallback Behaviour

- Unrecognised language code from detection → treated as English (FR-009)
- Translation API error → log error, use original text for search, return English response
- Language detection API error → log error, treat as English, continue Q&A flow
- Zero search results after translation → fallback prompt in detected language with disclaimer

---

## Error Responses

| Code | Condition |
|------|-----------|
| 400 | `questionText` missing or empty |
| 400 | `questionText` exceeds 30,000 characters |
| 500 | OpenAI API unreachable for embeddings or chat completions |
