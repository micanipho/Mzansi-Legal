# Contract: `POST /api/app/qa/ask`

**Feature**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01

## Purpose

Submit a natural-language South African legal question and receive a structured RAG outcome that:

- grounds direct legal claims in cited sources
- distinguishes binding law from official guidance
- asks for clarification when material facts are missing
- escalates or limits the answer when support is weak or the stakes are high

The wire response remains append-only for existing consumers.

## Request

### Body

```json
{
  "questionText": "Can my landlord evict me without a court order?"
}
```

### Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `questionText` | `string` | Yes | Natural-language legal question from the user |

## Response

The backend may be wrapped by the ABP `result` envelope at transport level. The contract below describes the inner result object.

### Body

```json
{
  "answerText": "No. A landlord cannot lawfully evict you without following the legal process. Here is the general rule and what usually happens next.",
  "isInsufficientInformation": false,
  "detectedLanguageCode": "en",
  "answerMode": "cautious",
  "confidenceBand": "medium",
  "clarificationQuestion": null,
  "citations": [
    {
      "chunkId": "0f0be7df-c3b7-43af-b8e2-8f6f4d470001",
      "actName": "Rental Housing Act",
      "sectionNumber": "s 4",
      "sourceTitle": "Rental Housing Act",
      "sourceLocator": "s 4",
      "authorityType": "bindingLaw",
      "sourceRole": "primary",
      "excerpt": "A landlord and tenant have reciprocal rights and obligations ...",
      "relevanceScore": 0.88
    }
  ],
  "chunkIds": [
    "0f0be7df-c3b7-43af-b8e2-8f6f4d470001"
  ],
  "answerId": "811a88d4-f3ea-4d13-a30f-4eb5d5d0b999"
}
```

### Top-level fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `answerText` | `string \| null` | Yes | Plain-language answer, clarification preface, or deterministic limitation text |
| `isInsufficientInformation` | `boolean` | Yes | Legacy compatibility flag; `true` when the response is insufficient or escalation-oriented |
| `detectedLanguageCode` | `string` | Yes | Detected input language code |
| `answerMode` | `"direct" \| "cautious" \| "clarification" \| "insufficient"` | Yes | Structured response posture |
| `confidenceBand` | `"high" \| "medium" \| "low"` | Yes | Retrieval-derived confidence band |
| `clarificationQuestion` | `string \| null` | Yes | Focused follow-up question when `answerMode = "clarification"` |
| `citations` | `Citation[]` | Yes | Supporting citations or source references |
| `chunkIds` | `string[]` | Yes | Traceability IDs for retrieved chunks |
| `answerId` | `string \| null` | Yes | Persisted answer ID for grounded stored answers only |

## Citation Object

### Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `chunkId` | `string` | Yes | Referenced chunk identifier |
| `actName` | `string` | Yes | Legacy display title kept for compatibility; may contain a guidance title until all clients move to `sourceTitle` |
| `sectionNumber` | `string` | Yes | Legacy locator kept for compatibility; may contain a heading or form locator for non-Act sources |
| `sourceTitle` | `string` | No | Preferred generic display title for any cited source |
| `sourceLocator` | `string` | No | Preferred generic locator such as section, rule, form, or heading |
| `authorityType` | `"bindingLaw" \| "officialGuidance"` | No | Distinguishes controlling law from supporting guidance |
| `sourceRole` | `"primary" \| "supporting"` | No | Distinguishes the main source from supporting sources |
| `excerpt` | `string` | Yes | Supporting source text |
| `relevanceScore` | `number` | Yes | Retrieval relevance score |

## Response Invariants by Mode

### `direct`

- `answerText` must contain a grounded answer.
- `citations.length >= 1`.
- At least one citation should have `authorityType = "bindingLaw"` and `sourceRole = "primary"`.
- `answerId` should be populated when persistence succeeds.
- `clarificationQuestion` must be `null`.

### `cautious`

- `answerText` must contain a grounded but qualified answer.
- `citations.length >= 1`.
- At least one citation should have `authorityType = "bindingLaw"`.
- `answerId` may be populated when the answer is grounded enough to persist.
- `clarificationQuestion` must be `null`.

### `clarification`

- `clarificationQuestion` must be populated.
- `answerText` may contain a short explanation of what is missing.
- `answerId` must be `null`.
- `citations` may be empty or may contain limited supporting context if the system already knows the relevant legal area.

### `insufficient`

- `answerText` should contain a deterministic limitation or escalation-oriented message.
- `isInsufficientInformation` should be `true`.
- `answerId` must be `null`.
- `citations` may be empty when no grounded source is available, or may contain limited contextual sources when the system is explaining why it cannot answer fully.

## Contract Rules

- The endpoint must never return uncited general legal advice as a fallback.
- When both law and guidance appear, the response must distinguish them through `authorityType` and `sourceRole`.
- Guidance must never be presented as the controlling legal source when a binding legal source is available.
- Existing consumers that only understand `actName` and `sectionNumber` must continue to function.

## Example Outcomes

### Clarification example

```json
{
  "answerText": "I need one more detail before I can answer safely.",
  "isInsufficientInformation": false,
  "detectedLanguageCode": "en",
  "answerMode": "clarification",
  "confidenceBand": "low",
  "clarificationQuestion": "Are you asking about a court eviction notice, a landlord lockout, or a threat to evict you?",
  "citations": [],
  "chunkIds": [],
  "answerId": null
}
```

### Insufficient or escalation example

```json
{
  "answerText": "I cannot safely answer this from the sources I have right now. Because this sounds urgent, please seek legal or official help as soon as possible.",
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
