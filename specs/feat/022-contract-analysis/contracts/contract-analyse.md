# Contract: `POST /api/app/contract/analyse`

**Feature**: `feat/022-contract-analysis` | **Date**: 2026-04-02

## Purpose

Accept an authenticated PDF upload, extract readable text, classify the contract, retrieve relevant South African legislation, generate a structured analysis, and return the saved result.

## Request

### Content type

`multipart/form-data`

### Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `file` | `file` | Yes | Contract PDF to analyze |
| `responseLanguageCode` | `string` | No | Preferred output language: `en`, `zu`, `st`, or `af`; if omitted, backend may fall back to current locale or English |

## Success Response

### Body

```json
{
  "id": "3a18f4b6-0c57-4d92-9139-8143de62e001",
  "displayTitle": "Lease agreement - 42 Maple Street",
  "contractType": "lease",
  "healthScore": 62,
  "summary": "This lease is mostly standard, but several clauses shift too much power to the landlord and need review before signing.",
  "language": "en",
  "analysedAt": "2026-04-02T11:42:00Z",
  "redFlagCount": 3,
  "amberFlagCount": 2,
  "greenFlagCount": 8,
  "flags": [
    {
      "severity": "red",
      "title": "Early cancellation notice appears too long",
      "description": "The contract requires far more notice than consumers usually receive under the current legal baseline.",
      "clauseText": "The tenant must give three calendar months' written notice...",
      "legislationCitation": "Consumer Protection Act 68 of 2008, section 14"
    }
  ]
}
```

### Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `id` | `string` | Yes | Saved analysis identifier |
| `displayTitle` | `string` | Yes | User-facing file or contract title |
| `contractType` | `"employment" \| "lease" \| "credit" \| "service"` | Yes | Supported contract family |
| `healthScore` | `number` | Yes | 0-100 score |
| `summary` | `string` | Yes | Plain-language summary in the selected output language |
| `language` | `string` | Yes | Output language code |
| `analysedAt` | `string` | Yes | UTC timestamp |
| `redFlagCount` | `number` | Yes | Count of `red` flags |
| `amberFlagCount` | `number` | Yes | Count of `amber` flags |
| `greenFlagCount` | `number` | Yes | Count of `green` flags |
| `flags` | `ContractFlag[]` | Yes | Full saved findings |

## Failure Behavior

The transport may still use the ABP error envelope. User-facing failures should distinguish:

- unreadable or empty PDF
- password-protected or invalid PDF
- unsupported or unclassifiable contract type
- analysis unavailable because grounded support is too weak

The backend must not return a fake analysis just to satisfy the shape.

## Contract Rules

- Authentication is required.
- A readable scanned PDF should succeed through OCR fallback before unreadable failure is returned.
- Definitive legal findings must include grounded legislation citations.
- Unsupported or weakly grounded issues must be downgraded into cautionary wording or analysis failure instead of fabricated legal certainty.

## ContractFlag Object

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `severity` | `"red" \| "amber" \| "green"` | Yes | Finding severity |
| `title` | `string` | Yes | Short finding title |
| `description` | `string` | Yes | Plain-language explanation |
| `clauseText` | `string` | Yes | Relevant clause excerpt from the contract |
| `legislationCitation` | `string \| null` | Yes | Citation for grounded legal claims; may be `null` for standard notes or clearly labeled cautionary items |
