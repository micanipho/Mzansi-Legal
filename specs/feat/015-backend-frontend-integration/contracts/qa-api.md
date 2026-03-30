# Contract: Q&A API Integration

## Endpoint: Ask Question
**Path**: `POST /api/app/qa/ask`
**Authentication**: None (for this phase, `[AbpAuthorize]` to be bypassed)
**Request Header**: `Content-Type: application/json`

### Request Body (`AskQuestionRequest`)
```json
{
  "questionText": "string"
}
```

### Response Body (`RagAnswerResult`)
```json
{
  "answerText": "string",
  "citations": [
    {
      "actName": "string",
      "sectionNumber": "string",
      "excerpt": "string",
      "relevanceScore": "number"
    }
  ],
  "chunkIds": ["Guid"],
  "isInsufficientInformation": "boolean",
  "answerId": "Guid"
}
```

### Errors
- `400 Bad Request`: Empty or invalid question text.
- `500 Internal Server Error`: Backend processing or LLM failure.
