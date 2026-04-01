# Mzansi-legal Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-04-01

## Active Technologies
- PostgreSQL 15+ via Npgsql; no new migrations planned, reusing `LegalDocuments`, `Categories`, `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, and `AnswerCitations` (feat/021-intent-aware-rag)
- C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for response-consumer updates + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; no new NuGet or npm packages planned (feat/021-intent-aware-rag)

- TypeScript 5, React 19, Next.js 16 App Router + Next.js, next-intl, Ant Design, @ant-design/icons, @ant-design/nextjs-registry, lucide-react, @ant-design/charts (011-frontend-shell-polish)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

npm test; npm run lint

## Code Style

TypeScript 5, React 19, Next.js 16 App Router: Follow standard conventions

## Recent Changes
- feat/021-intent-aware-rag: Added C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for response-consumer updates + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; no new NuGet or npm packages planned

- 011-frontend-shell-polish: Added TypeScript 5, React 19, Next.js 16 App Router + Next.js, next-intl, Ant Design, @ant-design/icons, @ant-design/nextjs-registry, lucide-react, @ant-design/charts

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
