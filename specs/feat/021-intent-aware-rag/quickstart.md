# Quickstart: Refining Intent-Aware Legal Retrieval

**Feature**: `feat/021-intent-aware-rag`  
**Branch**: `feat/021-intent-aware-rag`  
**Date**: 2026-04-01

## Goal

Use the existing RAG service as the baseline and refine it in small, verifiable slices. This feature is no longer a greenfield build; the backend already has document-aware retrieval, answer modes, and supporting tests. The quickstart below focuses on calibration, hardening, and verification.

## Prerequisites

- Existing multilingual RAG Q&A flow is working
- Legal corpus has already been ingested and embedded
- `OpenAI:ApiKey`, `OpenAI:BaseUrl`, and `OpenAI:ChatModel` are configured
- Backend tests can run from `backend/test/backend.Tests`
- Ask page currently consumes `POST /api/app/qa/ask`

## Bit-by-Bit Order

1. Capture the current baseline and benchmark prompts
2. Verify the startup index and document profiles
3. Refine query focus, source hints, and document ranking
4. Recalibrate confidence and response behavior
5. Verify contract, persistence rules, and Ask-page state handling
6. Expand regression coverage and re-run the benchmark pack

## Step 0 - Capture the Baseline

Before tuning anything:

1. Run the existing RAG unit tests.
2. Record the benchmark prompt pack for this feature:
   - plain-language questions without Act names
   - semantically equivalent variants
   - wrong explicit Act hints
   - multi-source questions
   - ambiguous short prompts
   - unsupported questions
3. Note the current expected primary source family and expected answer mode for each case.

Result: tuning work becomes repeatable instead of memory-based.

## Step 1 - Verify Startup Index and Document Profiles

Review these existing files first:

- `backend/src/backend.Application/Services/RagService/RagAppService.cs`
- `backend/src/backend.Application/Services/RagService/RagIndexStore.cs`
- `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs`

Checks:

1. `InitialiseAsync()` includes `Document` and `Document.Category`.
2. Each chunk is loaded into `IndexedChunk` with:
   - document title
   - short name
   - act number
   - year
   - category name
   - section title/number
   - parsed keywords
   - topic classification
3. `RagDocumentProfileBuilder` produces stable metadata terms, metadata phrases, and centroid vectors.
4. `RagIndexStore.Replace()` updates chunks and document profiles together.

Result: request-time retrieval stays in memory and deterministic.

## Step 2 - Refine Query Focus and Source Hints

Review:

- `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs`
- `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs`

Responsibilities:

1. Keep `RagQueryFocusBuilder` focused on stripping generic legal filler terms without losing the user's actual issue.
2. Keep `RagSourceHintExtractor` responsible for:
   - Act title matches
   - short-name matches
   - Act-number/year matches
   - category matches
3. Treat every hint as a boost only. Never let a hint become a hard filter.

Result: users can name an Act if they know it, but they do not have to.

## Step 3 - Calibrate Document Ranking

Review:

- `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs`

Tune or verify:

1. Candidate generation starts from semantic chunk similarity.
2. Document ranking blends:
   - chunk semantic strength
   - document centroid similarity
   - metadata alignment
   - keyword overlap
   - semantic breadth
   - source hint boost
3. Supporting documents are included only when they add real legal coverage.
4. Per-document chunk caps keep the prompt focused.

Result: the assistant finds the right Act from meaning, not only from vocabulary overlap.

## Step 4 - Recalibrate Confidence and Mode Selection

Review:

- `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs`

Checks:

1. `Direct` is reserved for clearly grounded, non-ambiguous cases.
2. `Cautious` covers grounded but non-decisive cases.
3. `Clarification` is used when the likely legal area is visible but a decisive fact is missing.
4. `Insufficient` is used when the corpus cannot responsibly answer.
5. `Clarification` and `Insufficient` are treated as safe, correct outcomes where appropriate.

Result: the system becomes more careful as certainty drops.

## Step 5 - Verify Prompt and Non-Grounded Behavior

Review:

- `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`
- `backend/src/backend.Application/Services/RagService/RagAppService.cs`

Confirm:

1. Prompt instructions remain mode-aware.
2. Temperature policy stays bounded:
   - direct `0.2`
   - cautious `0.1`
   - clarification `0.0`
   - insufficient `0.0` with deterministic non-grounded response
3. Clarification prompts ask one focused question.
4. Non-grounded responses do not fabricate citations.
5. General-knowledge legal fallback stays removed.

Result: response tone tracks retrieval certainty and stays legally conservative.

## Step 6 - Confirm API Contract and Persistence Rules

Review:

- `backend/src/backend.Application/Services/RagService/IRagAppService.cs`
- `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`
- `backend/src/backend.Web.Host/Controllers/QaController.cs`
- `specs/feat/021-intent-aware-rag/contracts/qa-ask.md`

Verify:

1. `RagAnswerResult` remains append-only for consumers.
2. `answerMode`, `confidenceBand`, and `clarificationQuestion` are documented and serialized as expected.
3. `Direct` and `Cautious` answers keep citations.
4. Only grounded direct/cautious answers are persisted.
5. Clarification/insufficient responses return `answerId = null`.

Result: backend behavior, contract docs, and persistence policy remain aligned.

## Step 7 - Validate the Ask-Page Consumer

Review:

- `frontend/src/services/qa.service.ts`
- `frontend/src/hooks/useChat.ts`
- `frontend/src/providers/chat-provider/context.tsx`
- `frontend/src/providers/chat-provider/index.tsx`
- `frontend/src/components/chat/ChatMessage.tsx`
- `frontend/src/messages/en.json`
- `frontend/src/messages/zu.json`
- `frontend/src/messages/st.json`
- `frontend/src/messages/af.json`

Checks:

1. Response types include `answerMode`, `confidenceBand`, and `clarificationQuestion`.
2. Chat state carries those fields end to end.
3. The UI surfaces:
   - direct answer
   - cautious answer
   - clarification needed
   - insufficient information
4. Status presentation stays semantic and accessible.
5. All four locale files contain clear state copy.

Result: users can immediately tell why the system is being careful.

## Step 8 - Expand Regression Coverage

Focus test files:

- `backend/test/backend.Tests/RagServiceTests/RagRetrievalPlannerTests.cs`
- `backend/test/backend.Tests/RagServiceTests/RagConfidenceEvaluatorTests.cs`
- `backend/test/backend.Tests/RagServiceTests/RagPromptBuilderTests.cs`
- `backend/test/backend.Tests/RagServiceTests/RagQueryFocusBuilderTests.cs`
- `backend/test/backend.Tests/RagServiceTests/RagDocumentProfileBuilderTests.cs`
- `backend/test/backend.Tests/RagServiceTests/RagAppServiceTests.cs`

Scenarios to cover:

- plain-language eviction question chooses the correct source
- semantically equivalent variants keep the same primary source family
- wrong explicit Act hint does not override stronger evidence
- multi-source question keeps both relevant sources
- ambiguous broad question returns clarification mode
- unsupported topic returns insufficiency mode
- clarification and insufficiency are never persisted as answers

Result: behavior remains stable after tuning.

## Step 9 - Run the Benchmark Prompt Pack

Use the prompt pack captured in Step 0 and verify:

1. Plain-language direct cases map to the expected Act or Act set.
2. Equivalent phrasings converge on the same primary source family.
3. Wrong-source hints do not dominate stronger factual evidence.
4. Ambiguous prompts route to clarification instead of overconfident answers.
5. Unsupported prompts route to insufficiency with no general legal advice.

Result: the system is calibrated with repeatable evidence, not just a smoke test.

## Step 10 - Manual Smoke and Sign-Off

### Direct answer

Submit:

```json
{
  "questionText": "Can my landlord evict me without a court order?"
}
```

Expected:

- `answerMode = "direct"`
- `confidenceBand = "high"` or strong `medium`
- citations include the governing housing / constitutional source family

### Clarification

Submit:

```json
{
  "questionText": "Can they evict me?"
}
```

Expected:

- `answerMode = "clarification"`
- `clarificationQuestion` is populated
- no definitive legal conclusion is given

### Wrong explicit source hint

Submit a prompt that mentions the wrong Act name but describes a housing eviction fact pattern.

Expected:

- the stronger factual housing source still wins
- the named but weaker Act does not become a hard filter

### Insufficient grounding

Submit:

```json
{
  "questionText": "What are the rules for commercial drone flight corridors?"
}
```

Expected:

- `answerMode = "insufficient"`
- `isInsufficientInformation = true`
- no general legal advice is produced

## Deferred Follow-Ons

Keep these outside this milestone unless scope changes intentionally:

- court-hierarchy weighting once judgments are part of the indexed corpus
- human-review sampling and operational analytics persistence
- broader legal-domain rollout beyond the current legislation-first corpus

## Files Changed Summary

| Action | File |
|--------|------|
| REFINE | `backend/src/backend.Application/Services/RagService/RagAppService.cs` |
| REFINE | `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs` |
| REFINE | `backend/src/backend.Application/Services/RagService/RagQueryFocusBuilder.cs` |
| REFINE | `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs` |
| REFINE | `backend/src/backend.Application/Services/RagService/RagDocumentProfileBuilder.cs` |
| REFINE | `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs` |
| REFINE | `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs` |
| VERIFY | `backend/src/backend.Application/Services/RagService/RagIndexStore.cs` |
| VERIFY/REFINE | `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs` |
| VERIFY | `backend/src/backend.Application/Services/RagService/IRagAppService.cs` |
| VERIFY | `backend/src/backend.Web.Host/Controllers/QaController.cs` |
| VERIFY/REFINE | `frontend/src/services/qa.service.ts` |
| VERIFY/REFINE | `frontend/src/hooks/useChat.ts` |
| VERIFY/REFINE | `frontend/src/providers/chat-provider/context.tsx` |
| VERIFY/REFINE | `frontend/src/providers/chat-provider/index.tsx` |
| VERIFY/REFINE | `frontend/src/components/chat/ChatMessage.tsx` |
| VERIFY/REFINE | `frontend/src/messages/en.json` |
| VERIFY/REFINE | `frontend/src/messages/zu.json` |
| VERIFY/REFINE | `frontend/src/messages/st.json` |
| VERIFY/REFINE | `frontend/src/messages/af.json` |
