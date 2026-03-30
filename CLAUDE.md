# Mzansi-legal Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-03-30

## Active Technologies
- C# / .NET 9.0 + ABP Zero 10.x, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4, EF Core 9.0.5 (003-abp-backend-setup)
- PostgreSQL 15+ via Npgsql (003-abp-backend-setup)
- [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION] + [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION] (004-rag-domain-model)
- [if applicable, e.g., PostgreSQL, CoreData, files or N/A] (004-rag-domain-model)
- C# on .NET 9.0 + ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x (004-rag-domain-model)
- PostgreSQL 15+ via Npgsql; float vectors stored as `real[]` (PostgreSQL array type) (004-rag-domain-model)
- C# on .NET 9.0 + ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses (005-qa-domain-model)
- C# on .NET 9.0 + ABP Zero 10.x + Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses (007-contract-analysis-domain)
- C# on .NET 9.0 + ABP Zero 10.x, EF Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses, UglyToad.PdfPig (NEW — `dotnet add package UglyToad.PdfPig`) (008-pdf-section-chunking)
- C# on .NET 9.0 + ABP Zero 10.x + `System.Net.Http.Json` (in-box with .NET 9), `Ardalis.GuardClauses` (already in project), `IHttpClientFactory` (ASP.NET Core built-in) (009-openai-embedding-service)
- No new tables — `ChunkEmbedding` entity and `real[]` column already exist in PostgreSQL via prior migration (009-openai-embedding-service)
- C# on .NET 9.0 + ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql 9.0.x, Ardalis.GuardClauses, UglyToad.PdfPig, System.Net.Http.Json (in-box) (010-etl-ingestion-pipeline)
- PostgreSQL 15+ via Npgsql; `float[]` stored as `real[]`; new columns on `IngestionJobs` and `DocumentChunks` (010-etl-ingestion-pipeline)
- C# on .NET 9.0 + ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses, UglyToad.PdfPig, System.Net.Http.Json (in-box) (012-legislation-seed-data)
- PostgreSQL 15+ via Npgsql; existing schema (no new migrations) (012-legislation-seed-data)
- C# on .NET 9.0 + ABP Zero 10.x, EF Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, ASP.NET Core 9.0 (013-railway-docker-deploy)
- PostgreSQL 15+ — provisioned as Railway PostgreSQL plugin (013-railway-docker-deploy)
- C# on .NET 9.0 + ABP Zero 10.x + `System.Net.Http.Json` (in-box with .NET 9), `Ardalis.GuardClauses` (already in project), `IHttpClientFactory` (ASP.NET Core built-in), existing `IEmbeddingAppService` and `EmbeddingHelper.CosineSimilarity` (feat/014-rag-qa-service)
- PostgreSQL 15+ via Npgsql — no new migrations; reuses `DocumentChunks`, `ChunkEmbeddings`, `Conversations`, `Questions`, `Answers`, `AnswerCitations` tables from prior features (005, 009) (feat/014-rag-qa-service)

- C# on .NET 9.0 + ABP Zero 10.x, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, EF Core 9.0.5 (003-abp-backend-setup)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

# Add commands for C# on .NET 9.0

## Code Style

C# on .NET 9.0: Follow standard conventions

## Recent Changes
- feat/014-rag-qa-service: Added C# on .NET 9.0 + ABP Zero 10.x + `System.Net.Http.Json` (in-box with .NET 9), `Ardalis.GuardClauses` (already in project), `IHttpClientFactory` (ASP.NET Core built-in), existing `IEmbeddingAppService` and `EmbeddingHelper.CosineSimilarity`
- 013-railway-docker-deploy: Added C# on .NET 9.0 + ABP Zero 10.x, EF Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, ASP.NET Core 9.0
- 012-legislation-seed-data: Added C# on .NET 9.0 + ABP Zero 10.x, Entity Framework Core 9.0.5, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x, Ardalis.GuardClauses, UglyToad.PdfPig, System.Net.Http.Json (in-box)


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
