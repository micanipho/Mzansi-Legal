# Tasks: Multilingual RAG Q&A (isiZulu, Sesotho, Afrikaans)

**Input**: Design documents from `/specs/feat/020-multilingual-rag/`  
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/qa-ask.md ✅ | quickstart.md ✅

**Tests**: Not explicitly requested — no test tasks generated.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared dependencies)
- **[Story]**: Which user story this task belongs to
- All paths are relative to the repository root

## Path Conventions

- **Backend Application layer**: `backend/src/backend.Application/Services/`
- **Backend Web.Host layer**: `backend/src/backend.Web.Host/Startup/`
- **DTOs**: `backend/src/backend.Application/Services/RagService/DTO/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify prerequisites are in place. This feature adds files to an existing project — no new build tooling or project initialization is needed.

- [x] T001 Confirm branch `feat/020-multilingual-rag` is checked out and `backend.sln` builds without errors (`dotnet build backend/backend.sln`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the `LanguageAppService` and update shared contracts. All user stories depend on this phase.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T002 Create `backend/src/backend.Application/Services/LanguageService/ILanguageAppService.cs` — declare interface with `Task<Language> DetectLanguageAsync(string text)` and `Task<string> TranslateToEnglishAsync(string text, Language sourceLanguage)` in namespace `backend.Services.LanguageService`; add XML doc comments on both methods as per data-model.md contracts

- [x] T003 Create `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` — implement `DetectLanguageAsync`: extends `ApplicationService`, implements `ILanguageAppService`; inject `IHttpClientFactory` and `IConfiguration`; guard against null/whitespace (return `Language.English`); build detection prompt (`system`: identifier role, `user`: constrained to exactly one of `en zu st af`, fallback to `en`); POST to `/v1/chat/completions` via named `"OpenAI"` HttpClient; parse trimmed lowercase response against `{ "en" → English, "zu" → Zulu, "st" → Sesotho, "af" → Afrikaans }`; return `Language.English` for any unrecognised code; use private `sealed record` types for OpenAI JSON serialisation (same pattern as `RagAppService` lines 308–327)

- [x] T004 Add `TranslateToEnglishAsync` to `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` — guard against null/whitespace; early-return `text` unchanged when `sourceLanguage == Language.English` (no API call); build translation prompt (`system`: translator role — translate faithfully, no commentary; `user`: "Translate the following {languageName} text to English. Return only the translation.\nText: {text}"); resolve language display names via a private `const` or `switch` (`Zulu → "isiZulu"`, `Sesotho → "Sesotho"`, `Afrikaans → "Afrikaans"`); POST to `/v1/chat/completions`; return trimmed response content

- [x] T005 [P] Update `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs` — add `Language language = Language.English` parameter to both `BuildSystemPrompt()` and `BuildFallbackSystemPrompt()`; add a private `static string GetLanguageDirective(Language language)` switch that returns `"Respond in {name}. Keep all Act names, section numbers, and legal citations in English."` for Zulu/Sesotho/Afrikaans and `string.Empty` for English; append directive as `"\n\n6. {directive}"` when non-empty; default parameter value must preserve backward compatibility with callers that pass no argument; update XML doc comments

- [x] T006 [P] Update `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs` — add `public string DetectedLanguageCode { get; set; } = "en";` with XML doc comment "ISO 639-1 code of the detected input language (e.g. \"zu\", \"en\")"; do not rename or remove any existing properties

**Checkpoint**: `LanguageAppService` compiles, `RagPromptBuilder` compiles with language-aware prompts, `RagAnswerResult` has `DetectedLanguageCode` — ready to wire into `RagAppService`

---

## Phase 3: User Story 1 — isiZulu Q&A (Priority: P1) 🎯 MVP

**Goal**: A user submits "Ingabe umnikazi wendlu angangixosha?" and receives a correct, relevant answer in isiZulu with English Act citations.

**Independent Test**: `POST /api/app/qa/ask` with `{ "questionText": "Ingabe umnikazi wendlu angangixosha?" }` → response body has `detectedLanguageCode: "zu"`, `answerText` starts in isiZulu, `citations[0].actName` is in English.

### Implementation for User Story 1

- [x] T007 [US1] Inject `ILanguageAppService` into `RagAppService` constructor in `backend/src/backend.Application/Services/RagService/RagAppService.cs` — add `private readonly ILanguageAppService _languageService;` field; update constructor signature and body to accept and assign the new dependency; add XML doc on the new field

- [x] T008 [US1] Add language detection and translation to `AskAsync` in `backend/src/backend.Application/Services/RagService/RagAppService.cs` — immediately after the guard clauses, add: `var detectedLanguage = await _languageService.DetectLanguageAsync(request.QuestionText);` then `var translatedText = await _languageService.TranslateToEnglishAsync(request.QuestionText, detectedLanguage);`; replace every occurrence of `request.QuestionText` used as the search/embedding input with `translatedText`; pass `detectedLanguage` to `BuildSystemPrompt(detectedLanguage)` and `BuildFallbackSystemPrompt(detectedLanguage)`; set `DetectedLanguageCode` on both return statements (`RagAnswerResult`) to the ISO string (`"en"`, `"zu"`, `"st"`, or `"af"`) derived from `detectedLanguage`; add an inline comment before the detect/translate block: `// Multilingual: detect input language and translate to English for knowledge-base search.`

- [x] T009 [US1] Update `PersistQaAsync` in `backend/src/backend.Application/Services/RagService/RagAppService.cs` — add `string translatedText` and `Language language` to the method signature (after existing `questionText` param); replace `Language.English` hard-codes with `language` on `Conversation`, `Question`, and `Answer` entities; set `Question.OriginalText = questionText` (unchanged), `Question.TranslatedText = translatedText` (new param); update the single call site in `AskAsync` to pass `request.QuestionText`, `translatedText`, and `detectedLanguage`

- [x] T010 [P] [US1] Register `ILanguageAppService` in `backend/src/backend.Web.Host/Startup/Startup.cs` — add `services.AddTransient<ILanguageAppService, LanguageAppService>();` adjacent to (and following the same pattern as) the existing `IEmbeddingAppService` registration; add the required `using backend.Services.LanguageService;` directive if not already present

**Checkpoint**: Build the solution (`dotnet build backend/backend.sln`). Submit `POST /api/app/qa/ask` with the isiZulu question above. Verify `detectedLanguageCode == "zu"`, the answer is in isiZulu, and at least one citation `actName` is in English.

---

## Phase 4: User Story 2 — Sesotho Q&A (Priority: P2)

**Goal**: A user submits a legal question in Sesotho and receives a relevant answer in Sesotho with English citations.

**Independent Test**: `POST /api/app/qa/ask` with a Sesotho legal question → `detectedLanguageCode: "st"`, `answerText` is in Sesotho, citation `actName` values are in English.

### Implementation for User Story 2

- [x] T011 [US2] Verify Sesotho routing end-to-end in `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` and `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs` — confirm: (1) `DetectLanguageAsync` maps `"st"` response → `Language.Sesotho`; (2) `TranslateToEnglishAsync` uses `"Sesotho"` as the language name in the translation prompt when `sourceLanguage == Sesotho`; (3) `GetLanguageDirective` returns the Sesotho directive string for `Language.Sesotho`; (4) `PersistQaAsync` stores `Language.Sesotho` on the question record; fix any gap found

**Checkpoint**: Submit a Sesotho legal question. Verify `detectedLanguageCode == "st"` and the answer is in Sesotho.

---

## Phase 5: User Story 3 — Afrikaans Q&A (Priority: P3)

**Goal**: A user submits a legal question in Afrikaans and receives a relevant answer in Afrikaans with English citations.

**Independent Test**: `POST /api/app/qa/ask` with an Afrikaans legal question → `detectedLanguageCode: "af"`, `answerText` is in Afrikaans, citation `actName` values are in English.

### Implementation for User Story 3

- [x] T012 [US3] Verify Afrikaans routing end-to-end in `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` and `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs` — confirm: (1) `DetectLanguageAsync` maps `"af"` → `Language.Afrikaans`; (2) `TranslateToEnglishAsync` uses `"Afrikaans"` in the translation prompt; (3) `GetLanguageDirective` returns the Afrikaans directive for `Language.Afrikaans`; (4) `PersistQaAsync` stores `Language.Afrikaans`; fix any gap found

**Checkpoint**: Submit an Afrikaans legal question. Verify `detectedLanguageCode == "af"` and the answer is in Afrikaans.

---

## Phase 6: User Story 4 — English No-Regression (Priority: P4)

**Goal**: An English question follows the same path as before this feature — no translation call is made, answer is in English, behaviour is unchanged.

**Independent Test**: `POST /api/app/qa/ask` with an English legal question → `detectedLanguageCode: "en"`, answer is in English, `TranslateToEnglishAsync` returns the original text without an API call.

### Implementation for User Story 4

- [x] T013 [US4] Verify English passthrough in `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` — confirm `TranslateToEnglishAsync` returns `text` immediately (no HTTP call) when `sourceLanguage == Language.English`; confirm `RagAppService.AskAsync` uses the untranslated text for embedding when language is English; confirm `DetectedLanguageCode` in the response is `"en"`; confirm `PersistQaAsync` stores `Language.English` with `OriginalText == TranslatedText` for English inputs; fix any gap found

**Checkpoint**: Submit an English legal question. Verify no regression in answer quality and `detectedLanguageCode == "en"`.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Hardening and cleanup across all stories.

- [x] T014 [P] Review all new and modified files for `docs/RULES.md` compliance — classes ≤ 350 lines, methods ≤ 1 screen, nesting ≤ 2 levels, all public methods have XML doc comments, no magic strings (language names are `const` or `enum`-driven), no dead code; fix any violations in `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` and `backend/src/backend.Application/Services/RagService/RagAppService.cs`

- [x] T015 [P] Add error-resilience to `LanguageAppService` in `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` — wrap each OpenAI HTTP call in a try/catch; on exception, log via ABP `Logger.Warn(...)` and return the safe fallback (`Language.English` for detection, original `text` for translation) so the RAG pipeline is never blocked by a language service failure; document the fallback behaviour in XML comments

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — **BLOCKS all user stories**
- **Phase 3 (US1 — isiZulu)**: Depends on Phase 2 completion
- **Phase 4 (US2 — Sesotho)**: Depends on Phase 3 (same infrastructure)
- **Phase 5 (US3 — Afrikaans)**: Depends on Phase 3 (same infrastructure)
- **Phase 6 (US4 — English)**: Depends on Phase 3 (same infrastructure)
- **Phase 7 (Polish)**: Depends on all story phases being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — no other story dependencies
- **US2 (P2)**: Can start after US1 is complete (shares `RagAppService` wiring)
- **US3 (P3)**: Can start after US1 is complete (same reason)
- **US4 (P4)**: Can start after US1 is complete (same reason); US2/US3 not required

### Within Each Phase

- T002 before T003 (same file — DetectLanguage before TranslateToEnglish)
- T005 and T006 parallel with T002/T003/T004 (different files)
- T007 before T008 before T009 (same file — constructor before AskAsync before PersistQaAsync)
- T010 parallel with T007/T008/T009 (different file — Startup.cs)

### Parallel Opportunities

```
Phase 2 parallel group A: T002 → T003 (sequential, same file)
Phase 2 parallel group B: T005, T006 (can run simultaneously, different files)
Groups A and B can run concurrently.

Phase 3 parallel group A: T007 → T008 → T009 (sequential, same file)
Phase 3 parallel group B: T010 (different file, can run alongside A)
```

---

## Parallel Example: Phase 2 Foundational

```text
# Start group A (LanguageAppService — sequential within group):
Task T002: Create LanguageAppService.cs with DetectLanguageAsync
  ↓ (same file)
Task T003: Add TranslateToEnglishAsync to LanguageAppService.cs

# Start group B simultaneously with group A (different files):
Task T005: Update RagPromptBuilder.cs — language parameter
Task T006: Update RagAnswerResult.cs — DetectedLanguageCode

# Once both groups complete, proceed to Phase 3 (US1).
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete **Phase 1**: Build check
2. Complete **Phase 2**: Foundational — `LanguageAppService` + prompt builder + DTO (CRITICAL)
3. Complete **Phase 3**: US1 — Wire into `RagAppService`, register DI
4. **STOP and VALIDATE**: Submit isiZulu test question, confirm response in isiZulu with English citations
5. Deploy/demo — core multilingual value delivered

### Incremental Delivery

1. Phase 1 + 2 → Language infrastructure ready
2. Phase 3 → isiZulu works → **Demo/deploy** (MVP)
3. Phase 4 → Sesotho works → **Demo/deploy**
4. Phase 5 → Afrikaans works → **Demo/deploy**
5. Phase 6 → English regression confirmed
6. Phase 7 → Polish merged

### Parallel Team Strategy

With two developers:
- Developer A: T002 → T003 → T007 → T008 → T009
- Developer B: T005 → T006 → T010 (then T011, T012, T013 after US1 lands)

---

## Notes

- [P] tasks operate on different files with no shared dependency — safe to run simultaneously
- [Story] labels map each task to the user story it verifies or delivers
- No new EF Core migration is needed — `OriginalText`, `TranslatedText`, and `Language` columns already exist on the `Questions` table (migration `20260328104812_AddQADomainModel`)
- The `Language` enum (`English=0, Zulu=1, Sesotho=2, Afrikaans=3`) already exists in `backend/src/backend.Core/Domains/QA/Language.cs` — do not modify it
- Language display names for prompts: `Zulu → "isiZulu"`, `Sesotho → "Sesotho"`, `Afrikaans → "Afrikaans"`
- ISO code mapping: `"en" → English`, `"zu" → Zulu`, `"st" → Sesotho`, `"af" → Afrikaans`
- All error fallbacks must return `Language.English` to preserve the existing English Q&A path
