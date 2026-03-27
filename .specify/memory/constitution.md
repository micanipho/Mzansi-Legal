<!--
Sync Impact Report
- Version change: 1.0.0 -> 1.1.0
- Modified principles:
    - IV. Domain Integrity and Secure User Ownership: added explicit reference to BACKEND_STRUCTURE.md layer rules
    - V. Responsible Delivery and Operational Measurability: expanded with coding-standards compliance gate
- Added sections:
    - Mandatory Reference Documents (new section listing BACKEND_STRUCTURE.md, BP.md, RULES.md)
    - Skill Usage Policy (new section under Development Workflow)
- Removed sections: none
- Templates requiring updates:
    - .specify/templates/plan-template.md: Constitution Check gates now include BACKEND_STRUCTURE layer check (✅ updated by this amendment — gates listed below)
    - .specify/templates/spec-template.md: no structural change needed (⚠ verify Key Entities section references DDD layer)
    - .specify/templates/tasks-template.md: task categories already cover backend/frontend layers (✅ aligned)
- Follow-up TODOs: none — all placeholders resolved
-->

# MzansiLegal Constitution

## Core Principles

### I. Multilingual-First User Experience

Every user-facing capability MUST support English, isiZulu, Sesotho, and Afrikaans for MVP,
including text input, voice input, output responses, and UI labels where applicable. Retrieval and
citations MUST remain grounded in English source legislation, while answers MUST be returned in
the detected or selected user language. New language additions MUST NOT require re-indexing of
the existing knowledge base.

### II. Citation-Grounded AI Responses

All legal and financial answers MUST be generated through retrieval-augmented generation (RAG)
against approved knowledge sources. Responses MUST include verifiable citations (Act and section)
and MUST avoid unsupported legal claims when context is insufficient. Cross-domain questions
(legal + financial) MUST be answered holistically when relevant evidence exists in indexed sources.

### III. Accessibility Is Non-Negotiable

The platform MUST be usable by blind, visually impaired, dyslexic, and low-literacy users.
All major flows MUST support keyboard navigation, screen reader semantics, and voice interaction.
Dyslexia mode (font/spacing adjustments) and audio output MUST be treated as first-class features,
not optional polish. Accessibility regressions in critical user journeys MUST block release.

### IV. Domain Integrity and Secure User Ownership

The domain model MUST follow DDD composition rules as specified in `docs/BACKEND_STRUCTURE.md`:
child entities declare PartOf ownership, enumerations use controlled RefList values, and binary
assets use StoredFile patterns. Every domain entity MUST extend `FullAuditedEntity<Guid>`.
The strict one-way layer dependency (Web.Host → Web.Core → Application → Core/EFCore) MUST
never be violated — no layer may reference a layer above it. User-specific artifacts
(conversations, answers, contract analyses, uploads) MUST be authenticated and scoped to their
owning AppUser except explicitly published FAQs. Cross-aggregate references are allowed only for
traceability requirements such as citation linkage to document chunks.

### V. Responsible Delivery and Operational Measurability

Each feature MUST ship with measurable outcomes aligned to success metrics: citation quality,
latency, language detection accuracy, accessibility conformance, and ingestion reliability. CI/CD
automation MUST build, test, and deploy both backend and frontend through a repeatable pipeline.
Legal and financial disclaimers MUST be present in relevant user flows and localized for supported
languages. All C# code MUST comply with `docs/RULES.md` and `docs/BP.md` before a PR is merged —
non-compliant code MUST NOT be merged without a recorded exception.

## Mandatory Reference Documents

These documents are authoritative and MUST be consulted before writing or reviewing any code:

| Document | Scope | Authority |
|---|---|---|
| `docs/BACKEND_STRUCTURE.md` | .NET/ABP backend layer rules, entity patterns, DbContext, migrations | Architecture reference for all backend work |
| `docs/BP.md` | C# and Next.js coding best practices | Coding standards guide for all C# and frontend work |
| `docs/RULES.md` | Enforceable C# coding rules (length, naming, guard clauses, formatting) | Non-negotiable quality gate for all C# code reviews |

Rules derived from these documents that MUST be enforced in every PR:

**C# (from `docs/RULES.md` and `docs/BP.md`)**:
- All classes MUST have a purpose comment unless self-evident to a junior developer.
- All public methods MUST have a purpose comment.
- Non-obvious logic MUST be preceded by an explanatory comment.
- Classes MUST NOT exceed 350 lines; methods MUST NOT require vertical scrolling.
- Nesting MUST NOT exceed two levels deep — refactor with early returns or extracted methods.
- Guard clauses using `Ardalis.GuardClauses` MUST validate all preconditions at the top of
  complex methods.
- Variable names MUST be clear and correctly spelled; `var` MUST only be used when the type is
  obvious from the right-hand side.
- Magic numbers MUST be replaced with named `const` values or `enum` types.
- Duplicated logic MUST be extracted into reusable methods (DRY).
- Dead code (unused methods, variables, commented-out blocks) MUST be deleted before merging.
- Code MUST be formatted (`Ctrl+E, Ctrl+D`) before committing.
- Performance: loops MUST NOT cause multiple database calls; bulk operations are preferred.
- Database indexes MUST be added for frequently queried fields.

**Backend Architecture (from `docs/BACKEND_STRUCTURE.md`)**:
- Every domain entity MUST extend `FullAuditedEntity<Guid>`.
- Every domain entity MUST have a corresponding `DbSet<T>` in `backendDbContext`.
- Application services MUST extend `AsyncCrudAppService` (CRUD) or `ApplicationService` (custom).
- Every service class MUST have a corresponding interface (`I{Entity}AppService`).
- DTOs MUST be decorated with `[AutoMap(typeof(TEntity))]`.
- DTOs MUST NOT expose EF navigation properties directly.
- Domain logic MUST NOT be placed in the Application layer.
- DTO classes MUST NOT be placed in the domain layer (except domain-service parameters).
- All controllers MUST inherit from `backendControllerBase`.

**Frontend (from `docs/BP.md`)**:
- File-based routing conventions MUST be followed in Next.js.
- SSR (`getServerSideProps`) MUST only be used for pages that genuinely require real-time data.
- Next.js `Image` component MUST be used for all images.
- TypeScript MUST be used throughout the frontend.
- Global state MUST be kept minimal; prefer props over global state.
- Third-party dependencies MUST be justified by necessity.

## Skill Usage Policy

When implementing tasks, agents MUST use the available project skills where applicable rather than
writing boilerplate from scratch:

| Task Type | Skill to Use |
|---|---|
| Scaffold a new ABP CRUD endpoint | `add-endpoint` |
| Add a new list page | `add-list-page` |
| Add a new detail page | `add-detail-page` |
| Add columns to a table | `add-columns` |
| Add a CRUD modal | `add-modal` |
| Add a frontend API service | `add-service` |
| Add or update styles | `add-styles` / `add-styling` |
| Create or update auth provider | `add-auth-provider` |
| Apply provider/state pattern | `apply-provider-pattern` |
| Group providers | `group-providers` |
| Create or amend the constitution | `speckit.constitution` |
| Create feature specification | `speckit.specify` |
| Plan implementation | `speckit.plan` |
| Generate tasks | `speckit.tasks` |
| Execute implementation | `speckit.implement` |
| Follow git workflow | `follow-git-workflow` |

Using a skill when one exists is MANDATORY. Deviating requires documented justification in the PR.

## Technology & Delivery Constraints

- Backend MUST be implemented with .NET 9 and ABP Framework as described in
  `docs/BACKEND_STRUCTURE.md`.
- Frontend MUST be implemented with Next.js and Ant Design.
- Primary persistence MUST use PostgreSQL; vector search MAY use in-memory cosine similarity
  for MVP and can evolve to pgvector for scale.
- AI stack MUST use OpenAI-compatible services for embeddings, multilingual generation,
  transcription, and text-to-speech, with environment-based key management.
- Knowledge ingestion MUST support structured chunking of legislation and contract text extraction.
- Contract analysis MUST include type detection, health scoring, summary generation, and
  legislation-backed flagging.
- Deployment MUST support GitHub Actions CI/CD with environment separation and secret handling.

## Development Workflow & Quality Gates

- Work items MUST map to approved backlog issues and milestone outcomes.
- Before writing any backend code, the author MUST consult `docs/BACKEND_STRUCTURE.md` to
  confirm the correct layer, naming convention, and entity pattern.
- Before writing or reviewing any C# code, the author MUST apply the rules in `docs/RULES.md`
  and the practices in `docs/BP.md`. Non-compliant code MUST NOT be merged.
- Any AI-facing endpoint MUST define prompt contract, citation behavior, and fallback behavior.
- Any new document source MUST be validated for licensing/public availability before ingestion.
- Pull requests MUST include test evidence for changed behavior (unit/integration/UI where
  applicable) and explicit accessibility impact notes for frontend changes.
- Release readiness MUST verify: disclaimer rendering, multilingual behavior, citation presence,
  and role-based access for admin functions.
- Major architectural changes MUST include migration notes for data model and pipeline impacts.
- All implementation tasks MUST use the relevant skill listed in the Skill Usage Policy above
  before falling back to manual scaffolding.

## Constitution Check Gates (for `speckit.plan`)

Every implementation plan MUST pass these gates before Phase 0 research:

1. **Layer Gate**: Does every new entity belong to the correct DDD layer per
   `docs/BACKEND_STRUCTURE.md`? (Core = entity, Application = service+DTO, EFCore = DbSet)
2. **Naming Gate**: Do all new services, DTOs, and controllers follow naming conventions in
   `docs/BACKEND_STRUCTURE.md`?
3. **Coding Standards Gate**: Does the planned implementation approach comply with
   `docs/RULES.md` (length, nesting, guard clauses, naming)?
4. **Skill Gate**: Has the author checked the Skill Usage Policy and identified which skills
   apply to this feature's tasks?
5. **Multilingual Gate**: Are all user-facing outputs planned for all four supported languages?
6. **Citation Gate**: Do all AI-facing endpoints define their RAG contract and citation format?
7. **Accessibility Gate**: Are keyboard navigation and screen reader semantics planned for all
   new frontend components?

## Governance

This constitution overrides conflicting implementation preferences for the project.

Amendment policy:
- Amendments require documented rationale linked to specification changes.
- Every amendment MUST update the version and amendment date.
- MAJOR version increments apply to breaking governance changes.
- MINOR version increments apply to new principles or materially expanded rules.
- PATCH version increments apply to clarifications without behavioral requirement changes.

Compliance policy:
- Reviews MUST verify adherence to all core principles.
- Any exception MUST be explicit, time-bound, and approved with a remediation plan.
- Non-compliant changes MUST NOT be merged without recorded exception approval.

**Version**: 1.1.0 | **Ratified**: 2026-03-26 | **Last Amended**: 2026-03-27
