# API Contracts: RAG Domain Model

**Branch**: `004-rag-domain-model` | **Date**: 2026-03-28

This feature exposes standard ABP CRUD endpoints for `Category` and `LegalDocument`. `DocumentChunk` and `ChunkEmbedding` are accessed by pipeline services (future feature) and are not exposed publicly at this stage.

---

## Category Endpoints

ABP auto-exposes these via `ICategoryAppService`.

### `GET /api/services/app/category/getAll`

Returns a paged list of categories.

**Query params**: `MaxResultCount`, `SkipCount`

**Response**:
```json
{
  "result": {
    "totalCount": 2,
    "items": [
      {
        "id": "guid",
        "name": "Labour Law",
        "icon": "gavel",
        "domain": 1,
        "sortOrder": 1
      }
    ]
  }
}
```

---

### `GET /api/services/app/category/get?id={guid}`

Returns a single category by Id.

---

### `POST /api/services/app/category/create`

Creates a new category.

**Request body**:
```json
{
  "name": "Labour Law",
  "icon": "gavel",
  "domain": 1,
  "sortOrder": 1
}
```

**Domain enum values**: `1 = Legal`, `2 = Financial`

---

### `PUT /api/services/app/category/update`

Updates an existing category.

**Request body**: same as create + `"id": "guid"`

---

### `DELETE /api/services/app/category/delete?id={guid}`

Soft-deletes a category. Will fail with FK constraint error if LegalDocuments reference it.

---

## LegalDocument Endpoints

ABP auto-exposes these via `ILegalDocumentAppService`.

### `GET /api/services/app/legalDocument/getAll`

Returns a paged list of legal documents.

**Response item shape**:
```json
{
  "id": "guid",
  "title": "Labour Relations Act",
  "shortName": "LRA",
  "actNumber": "66",
  "year": 1995,
  "fileName": "lra-1995.pdf",
  "categoryId": "guid",
  "isProcessed": false,
  "totalChunks": 0
}
```

**Note**: `FullText` is excluded from list DTOs (too large). Use `get` endpoint for full text.

---

### `GET /api/services/app/legalDocument/get?id={guid}`

Returns a single document including `FullText`.

---

### `POST /api/services/app/legalDocument/create`

Creates a new legal document record.

**Request body**:
```json
{
  "title": "Labour Relations Act",
  "shortName": "LRA",
  "actNumber": "66",
  "year": 1995,
  "fullText": "...",
  "fileName": "lra-1995.pdf",
  "categoryId": "guid",
  "isProcessed": false,
  "totalChunks": 0
}
```

**Validation errors**:
- 409 Conflict if `ActNumber + Year` combination already exists.
- 400 Bad Request if `CategoryId` does not reference a valid Category.

---

### `PUT /api/services/app/legalDocument/update`

Updates a legal document record. Used by the pipeline to mark `IsProcessed = true` and set `TotalChunks`.

---

### `DELETE /api/services/app/legalDocument/delete?id={guid}`

Soft-deletes the document. Cascades to `DocumentChunk` and `ChunkEmbedding` (hard cascade via EF, soft-delete from ABP on the root).

---

## Error Contract

All endpoints return ABP's standard error envelope:

```json
{
  "error": {
    "code": 0,
    "message": "Human-readable message",
    "details": "...",
    "validationErrors": []
  }
}
```

---

## Authentication

All endpoints require a valid JWT Bearer token in the `Authorization` header (`[AbpAuthorize]` applied to all service classes).
