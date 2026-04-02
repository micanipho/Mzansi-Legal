# Quickstart: Contract Analysis

**Feature**: `feat/022-contract-analysis` | **Date**: 2026-04-02

## Goal

Verify that contract upload, analysis, history, access control, OCR fallback, and contract-specific follow-up all work against the current codebase and current legal corpus.

## Prerequisites

- Backend configured with OpenAI-compatible chat, vision, and embedding settings
- Seeded legislation corpus loaded through the existing migrator and ETL flow
- Authenticated test user available in the frontend
- One readable PDF for each supported family where possible:
  - employment
  - lease
  - credit
  - service
- One scanned or image-heavy lease PDF
- One unreadable or protected PDF for failure testing

## Scenario 1 - Analyze a readable contract PDF

1. Sign in as a normal user.
2. Open the contracts page.
3. Upload a readable supported PDF.
4. Confirm the response shows:
   - a saved analysis id
   - contract type
   - health score
   - plain-language summary
   - ordered flags with clause text and citations where legal claims are made

**Expected**:
- Analysis completes without OCR fallback for normal text PDFs.
- Legal red flags cite current seeded legislation.
- The saved response matches the documented JSON contract with `healthScore`, `summary`, and ordered `flags[]`.
- The analysis appears in the user's history immediately after completion.

## Scenario 2 - Analyze a scanned PDF with OCR fallback

1. Upload a scanned or image-heavy supported contract.
2. Confirm direct extraction is insufficient and OCR fallback runs.
3. Confirm the final response still returns a normal saved analysis if OCR succeeds.

**Expected**:
- The request does not fail just because the PDF is image-heavy.
- The analysis still returns score, summary, and flags if OCR produces enough readable text.

## Scenario 3 - Fail safely on unreadable or unsupported contracts

1. Upload an unreadable, empty, password-protected, or unsupported PDF.
2. Confirm the API returns a clear user-facing failure.

**Expected**:
- No fake score or fake flags are returned.
- The user is told whether the problem is unreadable content or unsupported contract scope.

## Scenario 4 - Retrieve saved history and enforce privacy

1. Complete at least one successful analysis as User A.
2. Open `GET /api/app/contract/my` and confirm User A sees their saved analyses.
3. Open a saved analysis detail and confirm the full saved result loads.
4. Sign in as User B and try to open User A's analysis id directly.

**Expected**:
- User A sees only their own records.
- User B cannot retrieve User A's analysis.
- List items stay summary-only and do not expose the raw extracted contract text.

## Scenario 5 - Ask a follow-up question about a saved contract

1. Open a saved analysis with at least one meaningful flag.
2. Ask a clause-level follow-up question such as:
   - "Can the landlord really require three months' notice from me?"
   - "Is this overtime clause allowed?"
   - "Can they add these default fees?"
3. Confirm the answer uses the contract context and legislation citations.

**Expected**:
- Answer references the selected contract rather than answering generically.
- Grounded legal claims include citations.
- Weakly supported questions return cautionary or insufficient language instead of overclaiming.

## Coverage Matrix

| Scenario family | Current coverage state | Notes |
|-----------------|------------------------|-------|
| Employment contract terms | `InCorpusNow` | `BCEA` and `LRA` support many baseline employment clauses |
| Credit agreement fairness and default charges | `InCorpusNow` | `NCA` and `CPA` provide strong current grounding |
| Service-contract consumer fairness | `PartialCoverage` | `CPA` supports baseline consumer/service issues, but not all sector-specific contracts |
| Lease deposit, notice, and basic occupation issues | `PartialCoverage` | `RHA` and `Constitution` help, but some procedure-heavy outcomes still need `PIE` |
| Lease eviction procedure conclusions | `NeedsCorpusExpansion` | Do not overclaim until `PIE` is ingested |

## Verification Commands

```powershell
dotnet test backend/test/backend.Tests/backend.Tests.csproj
npm run lint --prefix frontend
npx tsc --noEmit --project frontend/tsconfig.json
```

## Manual Review Notes

- Check that localized contract summaries and errors read naturally in `en`, `zu`, `st`, and `af`.
- Check that clause excerpts are still legible after OCR fallback.
- Check that legal claims without citations are downgraded before the result is shown to users.
- Check that no raw contract text appears in server logs during upload or analysis.
