# Implementation Plan: MzansiLegal Platform

**Branch**: `001-mzansi-legal-rag` | **Date**: 2026-03-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification for MzansiLegal — a multilingual AI-powered legal and financial rights assistant.

## Summary
Build MzansiLegal, a full-stack RAG-based platform using .NET 8 (ABP Framework) and Next.js 14. The platform will provide multilingual legal/financial Q&A, voice interaction, contract analysis, and an admin moderation workflow. The technical approach leverages OpenAI for embeddings, generation, and speech services, with structured PDF ingestion using PdfPig and in-memory vector search for the MVP.

## Technical Context

**Language/Version**: .NET 8 (Backend), TypeScript / Next.js 14 (Frontend)
**Primary Dependencies**: ABP Framework, Next.js, Ant Design, next-intl, OpenAI SDK, PdfPig
**Storage**: PostgreSQL
**Testing**: xUnit (Backend), Jest/Playwright (Frontend)
**Target Platform**: Azure App Service, Azure Static Web Apps
**Project Type**: Full-stack web application (Modular Monolith)
**Performance Goals**: Text Q&A median <= 8s, Voice Q&A median <= 12s, Contract analysis <= 30s
**Constraints**: WCAG 2.1 AA compliance, RAG-only facts, 4-language support (en, zu, st, af)
**Scale/Scope**: 5 main pages, 13 core legislation documents, Citizen/Admin roles

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

1. **Multilingual-First**: **PASS**. Support for en, zu, st, af is baked into functional requirements and architecture principles.
2. **Citation-Grounded AI**: **PASS**. RAG architecture is mandatory; FR-003 enforces Act/section citations.
3. **Accessibility**: **PASS**. WCAG 2.1 AA and specific features (dyslexia mode, voice I/O) are prioritized in the spec.
4. **Domain Integrity**: **PASS**. DDD principles, PartOf composition, and RefList enums are explicitly required.
5. **Responsible Delivery**: **PASS**. Success criteria are measurable; CI/CD and disclaimers are included.

## Project Structure

### Documentation (this feature)

```text
specs/001-mzansi-legal-rag/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── MzansiLegal.Domain/           # Entities, RefLists
│   ├── MzansiLegal.Application/      # AppServices, DTOs
│   ├── MzansiLegal.EntityFrameworkCore/ # DB Context, Migrations
│   └── MzansiLegal.HttpApi.Host/     # API Host
└── test/
    ├── MzansiLegal.Domain.Tests/
    └── MzansiLegal.Application.Tests/

frontend/
├── src/
│   ├── app/                          # App Router Pages
│   ├── components/                   # Ant Design components
│   ├── services/                     # API clients
│   └── messages/                     # next-intl translations
└── tests/
    └── e2e/                          # Playwright tests
```

**Structure Decision**: Option 2: Web application (frontend + backend). Paths aligned with standard ABP and Next.js layouts.

## Complexity Tracking

*No current violations.*
