# Quickstart: RAG Question-Answering Service

**Feature**: `feat/014-rag-qa-service` | **Date**: 2026-03-30

## Prerequisites

Before testing this feature, ensure:

1. **Database populated**: ETL pipeline (feature 010) and legislation seed data (feature 012) have been run, so `DocumentChunks` and `ChunkEmbeddings` exist in PostgreSQL.
2. **OpenAI API key**: `OpenAI__ApiKey` is set in environment variables or `appsettings.Development.json`.
3. **Backend running**: `dotnet run --project backend/src/backend.Web.Host` (or Railway deployment).

---

## Step 1 — Authenticate

Obtain a JWT token for an existing user:

```bash
curl -X POST https://localhost:5001/api/TokenAuth/Authenticate \
  -H "Content-Type: application/json" \
  -d '{
    "userNameOrEmailAddress": "admin",
    "password": "123qwe",
    "rememberClient": false
  }'
```

Copy the `accessToken` value from the response.

---

## Step 2 — Ask a Legal Question

```bash
curl -X POST https://localhost:5001/api/app/qa/ask \
  -H "Authorization: Bearer {YOUR_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "questionText": "Can my landlord evict me without a court order?"
  }'
```

### Expected Response (success)

```json
{
  "answerText": "No, your landlord cannot evict you without a court order. According to Section 26(3) of the Constitution of the Republic of South Africa, no one may be evicted from their home without an order of court made after considering all relevant circumstances...",
  "isInsufficientInformation": false,
  "citations": [
    {
      "chunkId": "...",
      "actName": "Constitution of the Republic of South Africa",
      "sectionNumber": "§ 26(3)",
      "excerpt": "No one may be evicted from their home...",
      "relevanceScore": 0.91
    }
  ],
  "chunkIds": ["..."],
  "answerId": "..."
}
```

---

## Step 3 — Test the Insufficient Information Fallback

Ask about a topic not covered by any loaded legislation:

```bash
curl -X POST https://localhost:5001/api/app/qa/ask \
  -H "Authorization: Bearer {YOUR_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "questionText": "What is the maximum altitude allowed for commercial drones in South Africa?"
  }'
```

### Expected Response (fallback)

```json
{
  "answerText": null,
  "isInsufficientInformation": true,
  "citations": [],
  "chunkIds": [],
  "answerId": null
}
```

---

## Step 4 — Verify Persistence

After a successful answer, confirm the Q&A chain was persisted to PostgreSQL:

```bash
# Connect to the database
docker exec -it mzansi-pg psql -U postgres -d mzansi

# Check the persisted answer
SELECT a."Id", a."Text", a."Language", a."CreationTime"
FROM "Answers" a
ORDER BY a."CreationTime" DESC
LIMIT 5;

# Check citations
SELECT ac."ChunkId", ac."SectionNumber", ac."RelevanceScore"
FROM "AnswerCitations" ac
ORDER BY ac."CreationTime" DESC
LIMIT 10;
```

---

## Startup Verification

On application startup, check logs for the embedding load confirmation:

```
[RagService] Loaded 987 chunk embeddings into memory.
```

If this line is missing or shows 0, the ETL pipeline has not populated `ChunkEmbeddings`. Run:

```bash
# Trigger ETL for all legislation documents
curl -X POST https://localhost:5001/api/app/admin/etl/trigger/{documentId} \
  -H "Authorization: Bearer {YOUR_TOKEN}"
```

---

## Common Issues

| Symptom | Cause | Fix |
|---------|-------|-----|
| `401 Unauthorized` | Token expired or missing | Re-authenticate in Step 1 |
| `isInsufficientInformation: true` for all questions | No embeddings loaded | Run ETL pipeline; check startup log for loaded count |
| Slow first response (>10s) | OpenAI API latency spike | Normal under high load; check OpenAI status page |
| `500 Internal Server Error` | OpenAI `ApiKey` missing | Set `OpenAI__ApiKey` in environment or appsettings |
| Answer text does not cite legislation | LLM ignored prompt instructions | Check `ChatModel` is `gpt-4o` (not gpt-3.5); verify system prompt in `RagPromptBuilder` |
