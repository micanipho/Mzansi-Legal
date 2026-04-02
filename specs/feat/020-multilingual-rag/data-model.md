# Data Model: Multilingual RAG Q&A

**Feature**: feat/020-multilingual-rag  
**Date**: 2026-04-01  
**Status**: No new migrations — all schema changes already exist

---

## Existing Entities (no structural change)

### Question
**File**: `backend/src/backend.Core/Domains/QA/Question.cs`  
**Migration**: `20260328104812_AddQADomainModel`

The `Question` entity already supports multilingual storage. No new columns or migrations are required.

| Field | Type | Constraint | Purpose |
|-------|------|------------|---------|
| `Id` | `Guid` | PK | Inherited from `FullAuditedEntity<Guid>` |
| `ConversationId` | `Guid` | FK (Cascade) | Parent conversation |
| `OriginalText` | `string` (text) | NOT NULL | User's question as submitted (any language) |
| `TranslatedText` | `string` (text) | NOT NULL | English translation used for embedding + search |
| `Language` | `Language` (int enum) | NOT NULL | Detected language of the original question |
| `InputMethod` | `InputMethod` (int enum) | NOT NULL | Text or Voice |
| `AudioFile` | `string` (varchar 500) | NULL | Audio reference for Voice inputs |
| Audit fields | — | — | `FullAuditedEntity<Guid>` inheritance |

**Current gap being fixed**: `PersistQaAsync()` sets `Language = Language.English` and `TranslatedText = OriginalText` for all questions. After this feature, both fields will reflect the actual detected language and its English translation.

---

### Conversation
**File**: `backend/src/backend.Core/Domains/QA/Conversation.cs`

The `Conversation.Language` field is hardcoded to `Language.English` in `PersistQaAsync()`. After this feature, it will be set to the user's detected language.

No schema change needed.

---

### Answer
**File**: `backend/src/backend.Core/Domains/QA/Answer.cs`

The `Answer.Language` field is hardcoded to `Language.English` in `PersistQaAsync()`. After this feature, it will match the response language (same as detected question language).

No schema change needed.

---

### Language Enum
**File**: `backend/src/backend.Core/Domains/QA/Language.cs`

```
English  = 0
Zulu     = 1
Sesotho  = 2
Afrikaans = 3
```

The ISO codes used for detection/prompt map to these values:

| ISO Code | Enum Value | Display Name (for prompt) |
|----------|-----------|--------------------------|
| `en`     | `English`  | English (no directive added) |
| `zu`     | `Zulu`     | isiZulu |
| `st`     | `Sesotho`  | Sesotho |
| `af`     | `Afrikaans`| Afrikaans |

---

## New Application Service Contracts

### ILanguageAppService
**File to create**: `backend/src/backend.Application/Services/LanguageService/ILanguageAppService.cs`  
**Namespace**: `backend.Services.LanguageService`

```csharp
public interface ILanguageAppService
{
    /// Detects the ISO language of the input text.
    /// Returns Language.English if the language is unrecognised or unsupported.
    Task<Language> DetectLanguageAsync(string text);

    /// Translates the given text to English.
    /// Returns the original text unchanged if sourceLanguage is Language.English.
    Task<string> TranslateToEnglishAsync(string text, Language sourceLanguage);
}
```

---

### LanguageAppService
**File to create**: `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs`  
**Namespace**: `backend.Services.LanguageService`  
**Base class**: `ApplicationService`  
**Implements**: `ILanguageAppService`

**Dependencies injected**:
- `IHttpClientFactory` — reuses named "OpenAI" client from Startup.cs
- `IConfiguration` — reads `OpenAI:ApiKey` and `OpenAI:ChatModel`

**Internal behaviour**:

`DetectLanguageAsync(string text)`:
1. Guard: null/whitespace → return `Language.English`
2. Build detection prompt (system: identifier role, user: constrained to {en, zu, st, af})
3. POST to `/v1/chat/completions` via "OpenAI" named client
4. Parse response: trim, lowercase, match against known ISO codes
5. Fallback: any unrecognised code → `Language.English`

`TranslateToEnglishAsync(string text, Language sourceLanguage)`:
1. Guard: null/whitespace → return empty string
2. If `sourceLanguage == Language.English` → return `text` immediately (no API call)
3. Build translation prompt (system: translator role, user: translate to English)
4. POST to `/v1/chat/completions`
5. Return trimmed response content

---

## Modified Service Contracts

### RagPromptBuilder (modified)
**File**: `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`

Changed signatures:
```csharp
// Before
public static string BuildSystemPrompt()
public static string BuildFallbackSystemPrompt()

// After
public static string BuildSystemPrompt(Language language = Language.English)
public static string BuildFallbackSystemPrompt(Language language = Language.English)
```

Language directive appended when `language != Language.English`:
```
"\n\n6. {directive}"
```
Where `{directive}` is:
- Zulu: `"Respond in isiZulu. Keep all Act names, section numbers, and legal citations in English."`
- Sesotho: `"Respond in Sesotho. Keep all Act names, section numbers, and legal citations in English."`
- Afrikaans: `"Respond in Afrikaans. Keep all Act names, section numbers, and legal citations in English."`

Default parameter preserves backward compatibility — existing callers with no argument continue to work.

---

### RagAppService.AskAsync (modified)
**File**: `backend/src/backend.Application/Services/RagService/RagAppService.cs`

New flow in `AskAsync()`:

```
1. Detect language of request.QuestionText → detectedLanguage
2. If detectedLanguage != English:
     translatedText = await TranslateToEnglishAsync(request.QuestionText, detectedLanguage)
   Else:
     translatedText = request.QuestionText
3. Embed translatedText (not original)
4. Score chunks against translatedText embedding
5. Build prompts with detectedLanguage parameter
6. If authenticated: PersistQaAsync(userId, request.QuestionText, translatedText, detectedLanguage, answerText, chunks)
7. Add DetectedLanguageCode to RagAnswerResult
```

---

### RagAppService.PersistQaAsync (modified)
**File**: `backend/src/backend.Application/Services/RagService/RagAppService.cs`

New signature:
```csharp
private async Task<Guid> PersistQaAsync(
    long userId,
    string originalText,
    string translatedText,
    Language language,
    string answerText,
    IEnumerable<RagPromptBuilder.ScoredChunk> usedChunks)
```

Sets `Conversation.Language`, `Question.OriginalText`, `Question.TranslatedText`, `Question.Language`, and `Answer.Language` from the parameters (no longer hardcodes `Language.English`).

---

### RagAnswerResult (modified)
**File**: `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`

Adds:
```csharp
/// ISO 639-1 code of the detected input language (e.g. "zu", "en").
public string DetectedLanguageCode { get; set; } = "en";
```
