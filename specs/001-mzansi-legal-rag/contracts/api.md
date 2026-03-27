# API Contract: MzansiLegal

## Base URL: `/api/app`

### Auth and Profile
- **`POST /auth/login`**: Authenticate user and return JWT.
- **`POST /auth/register`**: Register new Citizen user.
- **`PUT /user/preferences`**: Update language, dyslexia mode, etc.

### Q&A and FAQs
- **`POST /question/ask`**
  - **Auth**: Citizen | Admin
  - **Request**: `{ conversationId?: Guid, language: string, text: string }`
  - **Response**: `QuestionWithAnswerDto` (includes citations)
- **`GET /question/history`**
  - **Auth**: Citizen | Admin (scoped)
- **`GET /question/faqs`**
  - **Auth**: Anonymous
  - **Query**: `categoryId?: Guid`

### Voice
- **`POST /voice/transcribe`**
  - **Auth**: Citizen | Admin
  - **Request**: `multipart/form-data` (audio file)
  - **Response**: `{ text: string, language: string }`
- **`POST /voice/speak`**
  - **Auth**: Citizen | Admin
  - **Request**: `{ text: string, language: string }`
  - **Response**: `audio/mpeg` stream

### Contracts
- **`POST /contract/analyse`**
  - **Auth**: Citizen | Admin
  - **Request**: `multipart/form-data` (file)
  - **Response**: `ContractAnalysisDto`
- **`GET /contract/{id}`**
  - **Auth**: Owner | Admin
- **`POST /contract/{id}/ask`**
  - **Auth**: Owner | Admin
  - **Request**: `{ text: string }`
  - **Response**: `QuestionWithAnswerDto` (context-aware)

### Admin Analytics and Moderation
- **`GET /admin/stats`**: Aggregate counts and trends.
- **`GET /admin/review-queue`**: List answers pending moderation.
- **`PUT /admin/answer/{id}/review`**: Set `isAccurate` and `adminNotes`.
- **`PUT /admin/faq/{conversationId}/publish`**: Set `isPublicFaq=true`.

### Knowledge Base (Admin only)
- **`POST /admin/document/upload`**: Upload legislation PDF and trigger ingestion.
- **`POST /admin/document/reindex`**: Clear and rebuild embeddings for a document.

## Schema Highlights

### `QuestionWithAnswerDto`
```json
{
  "id": "guid",
  "text": "string",
  "language": "string",
  "answer": {
    "text": "string",
    "citations": [
      {
        "actName": "string",
        "section": "string",
        "excerpt": "string",
        "relevance": 0.95
      }
    ]
  },
  "disclaimer": "string"
}
```

### `ContractAnalysisDto`
```json
{
  "id": "guid",
  "healthScore": 85,
  "summary": "string",
  "flags": [
    {
      "severity": "Red",
      "title": "string",
      "description": "string",
      "clauseText": "string",
      "legislationCitation": "string"
    }
  ]
}
```
