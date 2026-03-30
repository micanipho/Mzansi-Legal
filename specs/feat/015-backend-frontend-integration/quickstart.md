# Quickstart: Backend-Frontend Integration

## Prerequisites
- Backend: .NET 9 running (`backend.Web.Host` project).
- Frontend: Next.js running (`frontend/`).
- Knowledge Base: ETL pipeline must have ingested at least some legislation chunks (e.g., `012-legislation-seed-data`).

## Implementation Steps

### 1. Backend Bypassing Auth (Temporary)
- Open `backend/src/backend.Web.Host/Controllers/QaController.cs`.
- Comment out the `[AbpAuthorize]` attribute on the `Ask` method or the controller class.
- Re-run the backend.

### 2. Frontend API Service
- Create `frontend/src/services/qa.service.ts` to handle the `POST /api/app/qa/ask` request.
- Use the standard `fetch` API.

### 3. Frontend Chat UI
- Update the Q&A page/component to use the `qa.service.ts`.
- Implement a message thread state (array of messages).
- Use Ant Design's `List`, `Input`, and `Typography` components to display answers and citations.

### 4. Testing
- Submit "What are my rights regarding eviction?".
- Verify that a bot response appears with citations.
- Check the browser network tab for the API call to `localhost:{port}/api/app/qa/ask`.
