# Contract: `GET /api/app/contract/my`

**Feature**: `feat/022-contract-analysis` | **Date**: 2026-04-02

## Purpose

Return the authenticated user's saved contract analyses, newest first.

## Success Response

### Body

```json
{
  "items": [
    {
      "id": "3a18f4b6-0c57-4d92-9139-8143de62e001",
      "displayTitle": "Lease agreement - 42 Maple Street",
      "contractType": "lease",
      "healthScore": 62,
      "summary": "Several clauses need review before signing.",
      "language": "en",
      "analysedAt": "2026-04-02T11:42:00Z",
      "redFlagCount": 3,
      "amberFlagCount": 2,
      "greenFlagCount": 8
    }
  ],
  "totalCount": 1
}
```

### Item fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `id` | `string` | Yes | Analysis identifier |
| `displayTitle` | `string` | Yes | User-facing title |
| `contractType` | `"employment" \| "lease" \| "credit" \| "service"` | Yes | Supported family |
| `healthScore` | `number` | Yes | 0-100 score |
| `summary` | `string` | Yes | Short preview text |
| `language` | `string` | Yes | Output language code |
| `analysedAt` | `string` | Yes | UTC timestamp |
| `redFlagCount` | `number` | Yes | Quick risk signal |
| `amberFlagCount` | `number` | Yes | Quick caution signal |
| `greenFlagCount` | `number` | Yes | Standard-clause count |

## Contract Rules

- Only the authenticated user's own analyses are returned.
- Items are sorted newest first.
- The list is a summary surface and must not expose raw contract text.
