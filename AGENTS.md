# Mzansi-legal Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-04-02

## Active Technologies
- PostgreSQL 15+ via Npgsql; no new migrations planned, reusing `LegalDocuments`, `Categories`, `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, and `AnswerCitations` (feat/021-intent-aware-rag)
- C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for response-consumer updates + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; no new NuGet or npm packages planned (feat/021-intent-aware-rag)
- C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for Ask-page updates + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; existing manifest and ETL pieces `LegislationManifest`, `LegalDocumentRegistrar`, and `IngestionJob` remain the path for any later corpus additions (feat/021-intent-aware-rag)
- PostgreSQL 15+ via Npgsql; current feature slice reuses `LegalDocuments`, `Categories`, `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, `AnswerCitations`, and `IngestionJobs`; no new migration planned for retrieval hardening (feat/021-intent-aware-rag)
- C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for the contracts experience + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `UglyToad.PdfPig`, `next-intl`, Ant Design, current RAG helpers `RagIndexStore`, `RagSourceHintExtractor`, `RagRetrievalPlanner`, `RagConfidenceEvaluator`, and current OpenAI-compatible chat + embedding configuration (feat/022-contract-analysis)
- PostgreSQL 15+ via Npgsql; reuse existing `ContractAnalyses`, `ContractFlags`, `LegalDocuments`, `DocumentChunks`, `ChunkEmbeddings`, and ABP binary object storage; no contract-specific persistence expansion is planned for MVP follow-up Q&A (feat/022-contract-analysis)

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
- feat/022-contract-analysis: Added C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for the contracts experience + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `UglyToad.PdfPig`, `next-intl`, Ant Design, current RAG helpers `RagIndexStore`, `RagSourceHintExtractor`, `RagRetrievalPlanner`, `RagConfidenceEvaluator`, and current OpenAI-compatible chat + embedding configuration
- feat/021-intent-aware-rag: Added C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for Ask-page updates + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; existing manifest and ETL pieces `LegislationManifest`, `LegalDocumentRegistrar`, and `IngestionJob` remain the path for any later corpus additions
- feat/021-intent-aware-rag: Added C# on .NET 9.0 + ABP Zero 10.x; TypeScript 5 with Next.js 16 App Router for response-consumer updates + Existing `IEmbeddingAppService`, `ILanguageAppService`, `IHttpClientFactory`, `Ardalis.GuardClauses`, `next-intl`, plus current RAG helpers `RagIndexStore`, `RagDocumentProfileBuilder`, `RagSourceHintExtractor`, `RagQueryFocusBuilder`, `RagRetrievalPlanner`, and `RagConfidenceEvaluator`; no new NuGet or npm packages planned


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
