# Contract: POST /api/app/qa/ask

**Feature**: `feat/021-intent-aware-rag`  
**Controller**: `QaController` -> `RagAppService.AskAsync`  
**Authentication**: Optional (history persists only for grounded authenticated answers)

## Request

```json
POST /api/app/qa/ask
Content-Type: application/json

{
  "questionText": "Can my landlord evict me without a court order?"
}
```

| Field | Type | Required | Constraints | Notes |
|-------|------|----------|-------------|-------|
| `questionText` | string | Yes | 1-30,000 chars | Any supported language; server detects language and searches by meaning |

No `actName`, `documentId`, or `language` field is required from the client.

## Response Modes

The endpoint now returns one of four structured response modes:

| Mode | Meaning | `isInsufficientInformation` |
|------|---------|-----------------------------|
| `direct` | High-confidence grounded answer | `false` |
| `cautious` | Grounded answer with explicit limitations | `false` |
| `clarification` | Follow-up detail needed before a reliable answer | `true` |
| `insufficient` | Corpus cannot responsibly support the question | `true` |

## Response: Direct Answer

```json
HTTP 200 OK
{
  "answerText": "No. A landlord may not evict you without a court order, and a court must consider the relevant circumstances before eviction is granted. [Constitution of the Republic of South Africa, Section 26(3)]",
  "isInsufficientInformation": false,
  "detectedLanguageCode": "en",
  "answerMode": "direct",
  "confidenceBand": "high",
  "clarificationQuestion": null,
  "citations": [
    {
      "chunkId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "actName": "Constitution of the Republic of South Africa",
      "sectionNumber": "Section 26(3)",
      "excerpt": "No one may be evicted from their home ... without an order of court ...",
      "relevanceScore": 0.91
    }
  ],
  "chunkIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ],
  "answerId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

## Response: Cautious Answer

```json
HTTP 200 OK
{
  "answerText": "Based on the available legislation, your landlord cannot simply lock you out or evict you without a court process, but the exact process depends on the facts of the tenancy and the order sought. [Constitution of the Republic of South Africa, Section 26(3)] [Rental Housing Act 50 of 1999, Section 4]",
  "isInsufficientInformation": false,
  "detectedLanguageCode": "en",
  "answerMode": "cautious",
  "confidenceBand": "medium",
  "clarificationQuestion": null,
  "citations": [
    {
      "chunkId": "11111111-1111-1111-1111-111111111111",
      "actName": "Constitution of the Republic of South Africa",
      "sectionNumber": "Section 26(3)",
      "excerpt": "No one may be evicted from their home ...",
      "relevanceScore": 0.88
    },
    {
      "chunkId": "22222222-2222-2222-2222-222222222222",
      "actName": "Rental Housing Act 50 of 1999",
      "sectionNumber": "Section 4",
      "excerpt": "A landlord and tenant are bound by the rental agreement ...",
      "relevanceScore": 0.82
    }
  ],
  "chunkIds": [
    "11111111-1111-1111-1111-111111111111",
    "22222222-2222-2222-2222-222222222222"
  ],
  "answerId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
}
```

## Response: Clarification Needed

```json
HTTP 200 OK
{
  "answerText": "I can help with this, but I need one detail first before I give a reliable answer.",
  "isInsufficientInformation": true,
  "detectedLanguageCode": "en",
  "answerMode": "clarification",
  "confidenceBand": "low",
  "clarificationQuestion": "Are you asking about a court-ordered eviction from a rented home, or a landlord locking you out without a court order?",
  "citations": [],
  "chunkIds": [],
  "answerId": null
}
```

## Response: Insufficient Grounding

```json
HTTP 200 OK
{
  "answerText": "I don't have enough information in the available legislation to answer this question.",
  "isInsufficientInformation": true,
  "detectedLanguageCode": "en",
  "answerMode": "insufficient",
  "confidenceBand": "low",
  "clarificationQuestion": null,
  "citations": [],
  "chunkIds": [],
  "answerId": null
}
```

## Response Fields

| Field | Type | Notes |
|-------|------|-------|
| `answerText` | string \| null | User-facing response text |
| `isInsufficientInformation` | bool | True when the service cannot provide a definitive grounded answer |
| `detectedLanguageCode` | string | Existing field; `"en"`, `"zu"`, `"st"`, or `"af"` |
| `answerMode` | string | New; `direct`, `cautious`, `clarification`, or `insufficient` |
| `confidenceBand` | string | New; `high`, `medium`, or `low` |
| `clarificationQuestion` | string \| null | New; populated only for `clarification` mode |
| `citations` | array | Required for grounded claim-bearing responses |
| `chunkIds` | array | IDs of selected context chunks |
| `answerId` | uuid \| null | Non-null only for persisted grounded answers |

`answerMode` and `confidenceBand` are serialized as lower-case strings for frontend compatibility.

## Citation Rules

- `direct` and `cautious` responses must cite every material legal claim.
- `clarification` responses may omit citations because they do not provide a legal conclusion.
- `insufficient` responses must not fabricate citations.
- Act names and section identifiers remain in English even when the answer language is isiZulu, Sesotho, or Afrikaans.

## Fallback Behavior

- If the question clearly maps to a likely legal area but lacks a decisive fact, return `answerMode = clarification`.
- If the corpus cannot responsibly support the question, return `answerMode = insufficient`.
- The endpoint must not return a general-knowledge legal answer outside indexed and grounded material.

## Error Responses

| Code | Condition |
|------|-----------|
| 400 | `questionText` missing or empty |
| 400 | `questionText` exceeds 30,000 characters |
| 500 | Embedding or chat provider request fails unexpectedly |
