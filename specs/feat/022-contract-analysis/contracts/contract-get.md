# Contract: `GET /api/app/contract/{id}`

**Feature**: `feat/022-contract-analysis` | **Date**: 2026-04-02

## Purpose

Return a single saved contract analysis belonging to the authenticated user.

## Path Parameters

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `id` | `guid` | Yes | Contract analysis identifier |

## Success Response

The response shape matches the analysis result returned by `POST /api/app/contract/analyse`.

### Additional detail expectations

- `flags` must be returned in display order.
- The response should not expose raw full contract text by default.
- The response may include additional UI helper fields such as verdict or tag summaries if implementation needs them, as long as the core fields remain stable.

## Access Rules

- Authentication is required.
- Only the owning user may retrieve the analysis unless a separately authorized role is later introduced.
- Requests for another user's analysis must return denied/not found behavior rather than leaking existence.
