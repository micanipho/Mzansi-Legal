# Quickstart: Implementing Intent-Aware Legal Retrieval

**Feature**: `feat/021-intent-aware-rag`  
**Branch**: `feat/021-intent-aware-rag`  
**Date**: 2026-04-01

## Prerequisites

- Existing multilingual RAG Q&A flow is working
- Legal corpus has already been ingested and embedded
- `OpenAI:ApiKey`, `OpenAI:BaseUrl`, and `OpenAI:ChatModel` are configured
- Backend tests can run from `backend/test/backend.Tests`
- Ask page currently consumes `POST /api/app/qa/ask`

## Step 1 - Expand Startup Metadata Load

In `RagAppService.InitialiseAsync()`:

1. Keep loading embeddings into memory at startup.
2. Extend the include chain to load:
   - `Document`
   - `Document.Category`
3. Convert each chunk into a richer in-memory indexed model that includes:
   - document title
   - short name
   - act number
   - category name
   - section title
   - parsed keywords
   - topic classification

Result: `AskAsync()` can reason about source meaning without extra database calls.

## Step 2 - Add Source Hint Extraction

Create `RagSourceHintExtractor.cs` in `backend/src/backend.Application/Services/RagService/`.

Responsibilities:

- inspect the translated question text
- detect explicit mentions of:
  - full Act names
  - short names
  - act-number/year phrases
  - category names
- return hints with additive boost values

Important rule: a hint must never become a hard filter.

## Step 3 - Add Hybrid Retrieval Planner

Create `RagRetrievalPlanner.cs`.

Flow:

1. Start from semantic chunk similarity scores.
2. Keep a wider candidate pool than the final context size.
3. Group by document.
4. Score each document from:
   - strongest chunk score
   - average of strongest supporting chunks
   - topic match
   - keyword match
   - source hint boost
5. Select:
   - strongest primary document
   - supporting documents only when they materially add legal coverage
6. Cap chunks per document so one source does not monopolize context.

Result: the assistant can find the right Act from meaning, not just from explicit source names.

## Step 4 - Add Confidence and Mode Evaluation

Create `RagConfidenceEvaluator.cs`.

Compute:

- `RagConfidenceBand`: `High`, `Medium`, `Low`
- `RagAnswerMode`: `Direct`, `Cautious`, `Clarification`, `Insufficient`

Recommended mapping:

- strong aligned evidence -> `Direct`
- grounded but not decisive -> `Cautious`
- likely domain but missing facts -> `Clarification`
- no responsible grounding -> `Insufficient`

## Step 5 - Update Prompt Builder

Modify `RagPromptBuilder.cs` so prompt behavior depends on answer mode.

Add or update:

- mode-aware system prompt builder
- clarification-specific prompt instructions
- deterministic insufficiency message path
- temperature selection:
  - direct `0.2`
  - cautious `0.1`
  - clarification `0.0`

Do not generate general legal advice when grounding is absent.

## Step 6 - Refactor `RagAppService.AskAsync()`

Refactor the main flow to:

1. detect language
2. translate to English for search
3. embed translated question
4. build semantic candidate scores
5. extract source hints
6. run retrieval planner
7. run confidence evaluator
8. choose prompt/mode
9. either:
   - generate grounded direct answer
   - generate grounded cautious answer
   - generate clarification response
   - return insufficiency response

Persistence rule for this milestone:

- persist direct and cautious grounded answers
- do not persist clarification or insufficient responses

## Step 7 - Extend API DTOs

Update `RagAnswerResult.cs` with:

- `AnswerMode`
- `ConfidenceBand`
- `ClarificationQuestion`

Keep existing fields untouched so current frontend consumers do not break.

## Step 8 - Update Frontend Ask Consumer

Modify:

- `frontend/src/services/qa.service.ts`
- `frontend/src/hooks/useChat.ts`
- `frontend/src/providers/chat-provider/context.tsx`
- `frontend/src/providers/chat-provider/index.tsx`
- `frontend/src/components/chat/ChatMessage.tsx`

Add localized message keys in all four locale files for:

- cautious answer label/body
- clarification label/body
- insufficient information label/body

Goal: users should immediately understand why the system is being careful.

## Step 9 - Add Tests

Backend tests to add:

- `RagRetrievalPlannerTests.cs`
- `RagConfidenceEvaluatorTests.cs`

Backend tests to update:

- `RagAppServiceTests.cs`
- `RagPromptBuilderTests.cs`

Scenarios to cover:

- plain-language eviction question chooses the correct source
- semantically equivalent variants keep the same primary Act
- wrong explicit Act hint does not override stronger evidence
- ambiguous broad question returns clarification mode
- unsupported topic returns insufficiency mode
- no general-knowledge fallback path remains

## Step 10 - Manual Smoke Test

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
- citations include the governing housing / constitutional source

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

### Semantically equivalent variants

Submit multiple variants such as:

- `"Can my landlord evict me without court?"`
- `"Can a property owner throw me out if I rent from them?"`
- `"Do I need a court order before I can be evicted?"`

Expected:

- same primary legal source family appears across variants

### Insufficient grounding

Submit an unsupported question:

```json
{
  "questionText": "What are the rules for commercial drone flight corridors?"
}
```

Expected:

- `answerMode = "insufficient"`
- `isInsufficientInformation = true`
- no general legal advice is produced

## Files Changed Summary

| Action | File |
|--------|------|
| CREATE | `backend/src/backend.Application/Services/RagService/RagRetrievalPlanner.cs` |
| CREATE | `backend/src/backend.Application/Services/RagService/RagConfidenceEvaluator.cs` |
| CREATE | `backend/src/backend.Application/Services/RagService/RagSourceHintExtractor.cs` |
| CREATE | `backend/src/backend.Application/Services/RagService/DTO/RagAnswerMode.cs` |
| CREATE | `backend/src/backend.Application/Services/RagService/DTO/RagConfidenceBand.cs` |
| MODIFY | `backend/src/backend.Application/Services/RagService/RagAppService.cs` |
| MODIFY | `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs` |
| MODIFY | `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs` |
| MODIFY | `backend/src/backend.Web.Host/Controllers/QaController.cs` |
| MODIFY | `frontend/src/services/qa.service.ts` |
| MODIFY | `frontend/src/hooks/useChat.ts` |
| MODIFY | `frontend/src/providers/chat-provider/context.tsx` |
| MODIFY | `frontend/src/providers/chat-provider/index.tsx` |
| MODIFY | `frontend/src/components/chat/ChatMessage.tsx` |
| MODIFY | `frontend/src/messages/en.json` |
| MODIFY | `frontend/src/messages/zu.json` |
| MODIFY | `frontend/src/messages/st.json` |
| MODIFY | `frontend/src/messages/af.json` |
