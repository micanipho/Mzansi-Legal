# Contract: `POST /api/app/contract/{id}/ask`

**Feature**: `feat/022-contract-analysis` | **Date**: 2026-04-02

## Purpose

Ask a follow-up question about a previously analyzed contract using contract text plus grounded South African legislation as context.

## Request

### Body

```json
{
  "questionText": "Can the landlord really require three months' notice from me?",
  "responseLanguageCode": "en"
}
```

### Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `questionText` | `string` | Yes | Follow-up question about the selected contract |
| `responseLanguageCode` | `string` | No | Preferred output language: `en`, `zu`, `st`, or `af` |

## Response

### Body

```json
{
  "answerText": "A three-month cancellation notice may be difficult to justify if this agreement falls within the CPA consumer-cancellation framework. The safer reading is that the clause needs review before you rely on it as binding.",
  "answerMode": "cautious",
  "confidenceBand": "medium",
  "requiresUrgentAttention": false,
  "detectedLanguageCode": "en",
  "contractExcerpts": [
    "The tenant must give three calendar months' written notice..."
  ],
  "citations": [
    {
      "sourceTitle": "Consumer Protection Act",
      "sourceLocator": "s 14",
      "authorityType": "bindingLaw",
      "sourceRole": "primary",
      "excerpt": "A consumer may cancel a fixed-term agreement on 20 business days' notice..."
    }
  ]
}
```

### Top-level fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `answerText` | `string` | Yes | Contract-aware plain-language answer or limitation message |
| `answerMode` | `"direct" \| "cautious" \| "insufficient"` | Yes | Response posture |
| `confidenceBand` | `"high" \| "medium" \| "low"` | Yes | Retrieval-derived confidence |
| `requiresUrgentAttention` | `boolean` | Yes | Signals urgent-help copy for high-risk situations |
| `detectedLanguageCode` | `string` | Yes | Output or detected language code |
| `contractExcerpts` | `string[]` | Yes | Relevant contract snippets surfaced into the answer |
| `citations` | `ContractCitation[]` | Yes | Supporting legislation citations |

## ContractCitation Object

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `sourceTitle` | `string` | Yes | Legislation title |
| `sourceLocator` | `string` | Yes | Section or locator |
| `authorityType` | `"bindingLaw" \| "officialGuidance"` | Yes | Authority classification |
| `sourceRole` | `"primary" \| "supporting"` | Yes | Main or supporting source |
| `excerpt` | `string` | Yes | Supporting legal excerpt |

## Contract Rules

- Authentication is required.
- The selected contract analysis must belong to the requesting user.
- The endpoint must use the saved contract analysis as context, not just the follow-up question alone.
- If the available contract or legislation support is weak, the response must use `insufficient` instead of uncited legal advice.
- Output prose should honor the user's language preference, but legislation titles and locators stay in English.
