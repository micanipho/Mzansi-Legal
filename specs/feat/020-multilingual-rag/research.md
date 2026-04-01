# Research: Multilingual RAG Q&A

**Feature**: feat/020-multilingual-rag  
**Date**: 2026-04-01  
**Status**: Complete — all decisions resolved

---

## Decision 1: Language Detection Method

**Decision**: Use a GPT-4o chat completion call with a tightly constrained prompt.

**Prompt used**:
```
system: "You are a language identification assistant. Reply only with a single ISO 639-1 code."
user:   "Identify the language of the following text. Reply with exactly one of: en, zu, st, af.
         If unsure or the language is not in this list, reply: en
         Text: {questionText}"
```

**Rationale**: GPT-4o has strong coverage of isiZulu and Sesotho — languages that dedicated libraries (langdetect, langid) handle poorly. Because the platform already calls GPT-4o for Q&A, there is no new dependency. The constrained reply format ("respond with only the ISO code") makes parsing trivial and eliminates ambiguous output.

**Alternatives considered**:
- `langdetect` / `langid` Python libraries: Not applicable (project is C#); SA language accuracy is poor.
- Whisper language tag: Whisper detects spoken language during transcription and already provides a language code. For voice input, the Whisper result SHOULD be reused (avoids a second API call). This service handles text input only.
- Browser `navigator.language`: Frontend locale, not the question language — unreliable for multilingual users.

**Fallback**: If the returned code is not one of {en, zu, st, af} (malformed response), default to `Language.English`.

---

## Decision 2: Translation Method

**Decision**: Use a GPT-4o chat completion call for English translation.

**Prompt used**:
```
system: "You are a professional translator. Translate text faithfully, preserving meaning and tone.
         Do not add commentary or explanations."
user:   "Translate the following {languageName} text to English. Return only the translation.
         Text: {questionText}"
```

**Rationale**: GPT-4o produces high-quality translations for isiZulu and Sesotho compared to available MT APIs. The call is lightweight (question text only, not legislation) and fits within the existing OpenAI client infrastructure. Single-point API means no additional secret management.

**Alternatives considered**:
- Azure Translator or Google Translate API: Adds a new dependency and secret; isiZulu/Sesotho quality is acceptable but lower than GPT-4o for domain-specific legal phrasing.
- LibreTranslate (self-hosted): Significant ops overhead; SA language coverage is weak.

**When to skip**: If `detectedLanguage == Language.English`, translation is skipped entirely (`TranslatedText = OriginalText`).

---

## Decision 3: Placement in Architecture

**Decision**: Create `LanguageAppService` in `backend.Application/Services/LanguageService/` following the existing `EmbeddingAppService` pattern exactly.

**Rationale**: Both services are thin wrappers around OpenAI REST calls with no domain state. The Application layer is the correct home for orchestration services per the constitution and `docs/BACKEND_STRUCTURE.md`. Domain layer (`backend.Core`) must stay free of external API calls.

**Pattern reference**: `EmbeddingAppService` (line 20 in `EmbeddingAppService.cs`) — extends `DomainService`, injects `IHttpClientFactory`, reads config from `IConfiguration`. `LanguageAppService` will extend `ApplicationService` (not `DomainService`) since it is orchestration, not domain logic.

---

## Decision 4: Prompt Language Directive

**Decision**: Append a language directive to the existing system prompt when the detected language is not English.

**Directive format**:
```
"Respond in {languageName}. Keep all Act names, section numbers, and legal citations in English."
```

Where `{languageName}` is the full language name matching the ISO code (isiZulu, Sesotho, Afrikaans).

**Rationale**: Appending to the existing system prompt preserves all citation and accuracy rules. The explicit instruction to keep citations in English is required by FR-006 and SC-002.

**Position**: Appended as a new paragraph after rule 5 in `BuildSystemPrompt()`. No change to citation format, similarity threshold, or context block.

---

## Decision 5: Data Model — No New Migration Required

**Decision**: The `Questions` table schema already contains `OriginalText` (text NOT NULL), `TranslatedText` (text NOT NULL), and `Language` (integer NOT NULL) as of migration `20260328104812_AddQADomainModel`. No EF Core migration is needed.

**Current gap**: `RagAppService.PersistQaAsync()` hardcodes `Language.English` for all three entities (Conversation, Question, Answer). The fix is a code change only — pass detected language through the call chain.

**Rationale**: The domain model was designed multilingual-first. The persistence layer is already capable; only the application service needs to route the detected language correctly.

---

## Decision 6: AskQuestionRequest DTO — No Change

**Decision**: Do not add a `Language` field to `AskQuestionRequest`. Language is auto-detected server-side.

**Rationale**: Auto-detection is consistent with FR-001 (system detects language of every incoming question). Allowing client-supplied language would introduce a validation surface and trust boundary issue. If a voice input path needs to reuse a Whisper-detected language, that can be added as an optional override in a follow-on feature.

---

## Decision 7: RagAnswerResult DTO — Add DetectedLanguage

**Decision**: Add `string DetectedLanguageCode` (e.g., "zu") to `RagAnswerResult` so the frontend can display a language indicator and pass the code back for TTS routing (future).

**Rationale**: Useful for the frontend to confirm what language was detected. Costs nothing at the service level since the language is already computed.

---

## Decision 8: Service Registration

**Decision**: Register `ILanguageAppService` → `LanguageAppService` as a transient service in `Startup.cs`, following the same pattern as `IEmbeddingAppService`.

**No change needed** to `IHttpClientFactory` setup — the named "OpenAI" client is already registered and reused.

---

## Summary of Changes

| Component | Change Type | File |
|-----------|-------------|------|
| `LanguageAppService` | NEW | `backend.Application/Services/LanguageService/LanguageAppService.cs` |
| `ILanguageAppService` | NEW | `backend.Application/Services/LanguageService/ILanguageAppService.cs` |
| `RagPromptBuilder` | MODIFIED | `BuildSystemPrompt(Language)`, `BuildFallbackSystemPrompt(Language)` — add language param |
| `RagAppService` | MODIFIED | `AskAsync` — detect + translate; `PersistQaAsync` — propagate language |
| `RagAnswerResult` | MODIFIED | Add `DetectedLanguageCode` property |
| `Startup.cs` | MODIFIED | Register `ILanguageAppService` |
| No new migration | — | Schema already supports multilingual |
