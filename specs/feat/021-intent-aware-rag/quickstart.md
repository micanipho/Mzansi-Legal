# Quickstart: Intent-Aware Legal Retrieval for RAG Answers

**Feature**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01

## Purpose

Use this guide to verify the refined RAG behavior after implementation. It covers:

- the current seeded corpus reality
- benchmark scenarios that should pass now
- scenarios that are intentionally deferred until later corpus expansion
- the minimum backend and frontend validation loop

## 1. Confirm Current Corpus Reality

Before tuning or testing, verify that the benchmark expectations match the documents that actually exist in the repo today.

### Seeded document baseline

The current manifest already includes:

- `Constitution`
- `BCEA`
- `LRA`
- `RHA`
- `POPIA`
- `PHA`
- `CPA`
- `NCA`
- `TAA`
- `SARS Guide`
- `FAIS`
- `PFA`
- `FSCA Materials`

### High-value bundles not yet in the seeded corpus

These scenarios should be treated as follow-on corpus work unless the bundle is explicitly added during implementation:

- `PIE`
- `CCMA` Gazette rules and forms
- `Domestic Violence Act` and DOJ forms
- `Small Claims Courts Act` and DOJ guidance
- `PAJA`
- `PAIA` forms and guide
- `Criminal Procedure Act`
- maintenance forms
- case-law bundles from the Constitutional Court or SCA

## 2. Run the Core Validation Loop

### Backend

```powershell
dotnet test backend/test/backend.Tests/backend.Tests.csproj --filter RagServiceTests --no-restore
```

### Frontend

```powershell
cd frontend
npm run lint
npx tsc --noEmit
```

## 3. Benchmark Matrix

Use the matrix below to separate current retrieval expectations from follow-on corpus coverage.

| Scenario | Example prompt | Coverage state | Expected response |
|----------|----------------|----------------|------------------|
| Plain-language housing question | `Can my landlord evict me without a court order?` | `InCorpusNow` | `direct` or `cautious`, with `RHA` as the primary source and visible citations |
| isiZulu housing question | `Ingabe umnikazi wendlu angangixosha ngaphandle komyalelo wenkantolo?` | `InCorpusNow` | `direct` or `cautious`, answer text in isiZulu, and citations still shown with English source names and locators |
| Housing ambiguity | `Can they evict me?` | `InCorpusNow` | `clarification`, asking what situation the user means |
| Labour dismissal phrasing | `My boss fired me with no hearing. What can I do?` | `InCorpusNow` | `direct` or `cautious`, grounded in `LRA` and optionally `BCEA` |
| Wrong source hint | `I think POPIA says my landlord can lock me out` | `InCorpusNow` | `cautious` or `clarification`, correcting the source family toward housing law |
| Law vs guidance distinction | `Is this SARS guide legally binding?` | `GuidanceOnly` | `cautious`, with `TAA` or statute-grounded explanation plus clear guidance labeling for the `SARS Guide` |
| Consumer or debt plain-language question | `Can a debt collector keep calling me at work?` | `InCorpusNow` | grounded answer from `NCA` or related current debt sources if supported; otherwise cautious limitation |
| Harassment plain-language question | `Someone keeps threatening me online. What can I do?` | `InCorpusNow` | grounded answer or cautious limitation using `PHA`, with escalation if the wording is urgent |
| Domestic violence protection-order flow | `How do I get a protection order against my partner?` | `NeedsCorpusExpansion` | currently should not be treated as a retrieval regression if the `Domestic Violence Act` bundle is not yet added |
| Small claims workflow | `Someone owes me R8,000. How do I take them to small claims?` | `NeedsCorpusExpansion` | currently should be deferred until the `Small Claims` bundle is ingested |
| PAIA request workflow | `How do I file a PAIA request?` | `NeedsCorpusExpansion` | currently should be deferred until the `PAIA` bundle is ingested |
| Criminal arrest rights | `My brother was arrested and police will not tell us where he is` | `Escalate` | `insufficient` or escalation-oriented response, never confident uncited legal advice |

## 4. Manual Ask-Page Smoke Checks

After backend and frontend checks pass, validate the chat experience manually:

1. Submit an `InCorpusNow` housing question and confirm the message shows:
   - answer text
   - citations
   - visible confidence
   - no misleading guidance label
2. Submit a clarification-style prompt and confirm:
   - `clarificationQuestion` is shown clearly
   - the message uses the clarification status treatment
3. Submit a guidance-vs-law prompt and confirm:
   - the answer explains that guidance is not binding law
   - source labels distinguish the two roles
4. Submit a high-risk prompt and confirm:
   - the UI shows limitation or escalation wording
   - no fabricated citations appear
   - the system does not present a confident legal conclusion

## 5. Pass and Fail Interpretation

### Treat as a retrieval or contract defect

- The question is clearly `InCorpusNow` and the system still selects the wrong source family.
- A direct answer is returned without citations.
- Guidance is presented as though it were binding law.
- A short ambiguous prompt receives a confident direct answer.
- A high-risk prompt receives uncited general legal advice.

### Treat as a corpus coverage gap or follow-on task

- The scenario depends on a source bundle that is not yet seeded.
- The response appropriately limits itself or escalates because the needed authority is absent.
- The benchmark outcome changes from `NeedsCorpusExpansion` to `InCorpusNow` only after the missing bundle is added through the manifest and ETL path.

## 6. Follow-On Corpus Expansion Order

If the team chooses to expand the corpus after the retrieval-hardening slice, add bundles in this order:

1. `PIE`
2. `CCMA` rules and forms
3. `Domestic Violence Act` and forms
4. `Small Claims` bundle
5. `PAJA` and `PAIA` bundle
6. `Criminal Procedure Act`

Each bundle should include:

- authoritative public source confirmation
- licensing review
- manifest registration
- ETL ingestion tracking through `IngestionJob`
- new benchmark cases unlocked by the bundle
