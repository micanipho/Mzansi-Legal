# Quickstart: Implementing Multilingual RAG Q&A

**Feature**: feat/020-multilingual-rag  
**Branch**: `feat/020-multilingual-rag`  
**Date**: 2026-04-01

---

## Prerequisites

- Working English RAG Q&A pipeline (`feat/014-rag-qa-service` merged)
- OpenAI API key in `appsettings.json` under `OpenAI:ApiKey` and `OpenAI:ChatModel`
- Named `"OpenAI"` `HttpClient` already registered in `Startup.cs`

---

## Step 1: Create LanguageAppService

Create two files in a new folder `backend/src/backend.Application/Services/LanguageService/`:

**`ILanguageAppService.cs`** — interface with two methods:
- `Task<Language> DetectLanguageAsync(string text)`
- `Task<string> TranslateToEnglishAsync(string text, Language sourceLanguage)`

**`LanguageAppService.cs`** — implementation:
- Extends `ApplicationService` (not `DomainService`)
- Injects `IHttpClientFactory` and `IConfiguration`
- Uses the named `"OpenAI"` client
- Uses private `sealed record` types for OpenAI JSON serialisation (same pattern as `RagAppService`)
- Guard clauses at the top of each public method
- Returns `Language.English` as fallback for any unrecognised detection result

See [data-model.md](data-model.md) for the exact prompts and logic flow.

---

## Step 2: Register the Service

In `backend/src/backend.Web.Host/Startup/Startup.cs`, add alongside the existing embedding service registration:

```csharp
services.AddTransient<ILanguageAppService, LanguageAppService>();
```

---

## Step 3: Modify RagPromptBuilder

In `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs`:

1. Add `Language language = Language.English` parameter to `BuildSystemPrompt()` and `BuildFallbackSystemPrompt()`
2. Add a static helper `GetLanguageDirective(Language language)` that returns the directive string for non-English languages
3. Append the directive as rule 6 when `language != Language.English`

Default parameter value (`Language.English`) means all existing call sites continue to compile with no changes needed.

---

## Step 4: Modify RagAppService

In `backend/src/backend.Application/Services/RagService/RagAppService.cs`:

1. **Constructor**: inject `ILanguageAppService _languageService`
2. **`AskAsync()` — add before embedding**:
   ```
   var detectedLanguage = await _languageService.DetectLanguageAsync(request.QuestionText);
   var translatedText = await _languageService.TranslateToEnglishAsync(request.QuestionText, detectedLanguage);
   ```
3. **`AskAsync()` — use `translatedText`** for embedding and context search instead of `request.QuestionText`
4. **`AskAsync()` — pass language** to both `BuildSystemPrompt(detectedLanguage)` and `BuildFallbackSystemPrompt(detectedLanguage)`
5. **`AskAsync()` — update `PersistQaAsync` call** to pass `request.QuestionText`, `translatedText`, `detectedLanguage`
6. **`AskAsync()` — set `DetectedLanguageCode`** on `RagAnswerResult`
7. **`PersistQaAsync()` — update signature** and set `Language` on Conversation, Question, and Answer instead of hardcoding `Language.English`

---

## Step 5: Update RagAnswerResult DTO

In `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs`:

Add:
```csharp
public string DetectedLanguageCode { get; set; } = "en";
```

---

## Step 6: No Migration Required

The `Questions`, `Answers`, and `Conversations` tables already have `Language` and `OriginalText`/`TranslatedText` columns from migration `20260328104812_AddQADomainModel`. Do not create a new migration.

---

## Step 7: Manual Test

Submit a POST request:

```json
POST /api/app/qa/ask
{
  "questionText": "Ingabe umnikazi wendlu angangixosha?"
}
```

**Expected**:
- `detectedLanguageCode`: `"zu"`
- `answerText`: Starts in isiZulu
- `citations[0].actName`: In English (e.g., "Rental Housing Act 50 of 1999")

---

## Files Changed Summary

| Action | File |
|--------|------|
| CREATE | `backend/src/backend.Application/Services/LanguageService/ILanguageAppService.cs` |
| CREATE | `backend/src/backend.Application/Services/LanguageService/LanguageAppService.cs` |
| MODIFY | `backend/src/backend.Application/Services/RagService/RagPromptBuilder.cs` |
| MODIFY | `backend/src/backend.Application/Services/RagService/RagAppService.cs` |
| MODIFY | `backend/src/backend.Application/Services/RagService/DTO/RagAnswerResult.cs` |
| MODIFY | `backend/src/backend.Web.Host/Startup/Startup.cs` |
| NO CHANGE | Database schema (migration already done) |
| NO CHANGE | `AskQuestionRequest.cs` (auto-detect, no client field needed) |
