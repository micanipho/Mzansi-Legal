<!--
Sync Impact Report
- Version change: 1.2.0 -> 1.3.0
- Modified principles:
    - II. Citation-Grounded AI Responses -> II. Authority-Weighted Citation-Grounded AI Responses
    - III. Accessibility Is Non-Negotiable -> III. Accessible, Plain-Language, Mobile-First Delivery
    - IV. Domain Integrity and Secure User Ownership -> IV. Domain Integrity, Provenance, and Secure User Ownership
    - V. Responsible Delivery and Operational Measurability -> V. Legal Product Safety, POPIA Stewardship, and Measurable Trust
- Added sections:
    - South Africa Legal Research Inputs
- Removed sections: none
- Templates requiring updates:
    - .specify/templates/plan-template.md: Constitution Check expanded with source authority, safety, data governance, and corpus governance gates (updated)
    - .specify/templates/spec-template.md: feature requirements guidance expanded for legal-AI source authority, escalation, and POPIA constraints (updated)
    - .specify/templates/tasks-template.md: verification and corpus-governance tasks aligned to the new constitutional requirements (updated)
- Follow-up TODOs: none - both research reports are now encoded as mandatory project guardrails
-->

# MzansiLegal Constitution

## Core Principles

### I. Multilingual-First User Experience

Every user-facing capability MUST support English, isiZulu, Sesotho, and
Afrikaans for MVP, including text input, voice input, output responses, and UI
labels where applicable. Retrieval and citations MUST remain grounded in English
source legislation unless and until multilingual primary-source ingestion is
explicitly added. Answers MUST be returned in the detected or selected user
language. New language additions MUST NOT require re-indexing of the existing
knowledge base.

### II. Authority-Weighted Citation-Grounded AI Responses

All legal and financial answers MUST be generated through retrieval-augmented
generation (RAG) against approved South African knowledge sources. Binding law -
the Constitution, statutes, regulations, and authoritative judgments - MUST
outrank official guidance, forms, regulator material, and any secondary
commentary in retrieval, reranking, and response composition. Every material
legal claim MUST include a verifiable pinpoint citation to the supporting
section, paragraph, rule, or form identifier. Official guidance MAY be used to
explain procedure or next steps, but it MUST be labeled as guidance and MUST NOT
be presented as binding law. If sufficient authority cannot be retrieved, the
system MUST ask clarifying questions, limit the answer, or state that it lacks
enough information rather than infer the missing law.

When judgments are used, court hierarchy MUST shape authority weighting:
Constitutional Court first, Supreme Court of Appeal second, then the most
relevant High Court authority. Cross-domain questions MUST be answered
holistically only when relevant authority exists in indexed sources. Rights
violation letters, contract analysis outputs, and comparison recommendations
MUST cite the specific legislation or authority relied upon.

### III. Accessible, Plain-Language, Mobile-First Delivery

The platform MUST be usable by blind, visually impaired, dyslexic, low-literacy,
and mobile-first users. Every major response flow MUST lead with a short
plain-language explanation and then allow the user to expand into the supporting
law, citations, and nuance. All major flows MUST support keyboard navigation,
screen reader semantics, and voice interaction. Dyslexia mode, audio output, and
progressive disclosure are first-class features, not optional polish.
Accessibility regressions in critical user journeys MUST block release.

### IV. Domain Integrity, Provenance, and Secure User Ownership

The domain model MUST follow DDD composition rules as specified in
`docs/BACKEND_STRUCTURE.md`: child entities declare PartOf ownership,
enumerations use controlled RefList values, and binary assets use StoredFile
patterns. Every domain entity MUST extend `FullAuditedEntity<Guid>`. The strict
one-way layer dependency (Web.Host -> Web.Core -> Application -> Core/EFCore)
MUST never be violated - no layer may reference a layer above it.

The domain MUST implement 6 aggregates: Knowledge Base (LegalDocument),
Conversation (Q&A + FAQs), Contract (ContractAnalysis), Comparison
(DocumentComparison), ETL (IngestionJob), and User (AppUser extends
IdentityUser). The standalone Category entity groups documents and FAQs.

Every ingested legal source MUST preserve provenance metadata sufficient for
authority checks, freshness review, and citations, including source authority,
document type, source title, act or case identifier, and effective or last-seen
dates where available. Cross-aggregate references are allowed only for
traceability requirements such as citation linkage to document chunks or
ingestion tracking to LegalDocument.

User-specific artifacts (conversations, answers, contract analyses, uploads,
violation letters, document comparisons) MUST be authenticated and scoped to
their owning AppUser. Exception: the Community Insights Dashboard is
public-facing and MUST NOT expose any personally identifiable data - only
anonymized, aggregated statistics are permitted. FAQs (Conversation with
IsPublicFaq = true) are publicly visible but MUST only be created by admin-role
users. Regular user conversations MUST remain private.

### V. Legal Product Safety, POPIA Stewardship, and Measurable Trust

The product MUST present itself as an information, guidance, and explanation
system - not as a law firm or legal practitioner. The system MUST NOT promise
outcomes, hold itself out as professional representation, or generate uncited
litigation-ready content without licensed-practitioner review. User-drafted
templates MAY be offered for letters, requests, or checklists, but
court-directed or high-stakes drafting MUST be gated behind explicit human
review or kept out of scope.

High-risk or fact-sensitive matters - including arrest, imminent eviction,
domestic violence, urgent deadlines, or similarly consequential legal exposure -
MUST trigger clarifying questions and/or visible escalation guidance to human
legal help, Legal Aid, or official emergency resources where applicable.
Low-confidence, unsupported, or uncited legal interpretations MUST NOT be
presented as settled conclusions.

Any persistent handling of user questions, uploads, logs, analytics, or vendor
processing that contains personal information MUST define and implement
POPIA-aligned purpose limitation, minimality, retention, deletion or
de-identification, security safeguards, breach response, and cross-border
transfer posture before release. The system MUST NOT be used as a solely
automated decision-maker for outcomes with legal or similarly substantial effect.

Each shipped feature MUST include measurable quality evidence appropriate to its
scope. At minimum this includes citation quality, latency, language detection
accuracy, and accessibility conformance. Legal-AI features MUST also show
evidence for primary-source retrieval accuracy, guidance-versus-law labeling,
groundedness or faithfulness, and high-risk escalation behavior.

## South Africa Legal Research Inputs

The following project research documents are normative for legal-AI product
behavior and corpus governance:

| Document | Scope | Authority |
|---|---|---|
| `docs/deep-research-report.md` | South African source hierarchy, product safety boundaries, POPIA controls, evaluation strategy, escalation design | Authoritative legal-AI safety and retrieval policy |
| `docs/research_legislation.md` | Corpus bundle priorities, official forms and guides, leading cases, source licensing posture, freshness expectations | Authoritative corpus-ingestion and source-selection policy |

Rules derived from these research inputs that MUST be enforced in every legal-AI
feature or corpus change:

- Primary sources MUST be preferred in this order unless a narrower binding rule
  applies: Constitution, statutes/regulations, authoritative judgments, official
  forms and regulator guidance, then any approved secondary commentary.
- Official guidance and forms MUST be labeled as guidance or procedure support
  and MUST NOT replace controlling legal authority when the response makes a
  legal conclusion.
- Corpus expansion MUST follow a domain-bundle approach: primary law, official
  procedure guidance/forms, and leading cases where interpretation materially
  affects user outcomes.
- Sources with uncertain versioning or licensing - including unofficial mirrors
  or publisher-watermarked compiled PDFs - MUST be link-only or explicitly
  approved before ingestion; prefer Government Gazette or other official
  repository originals when available.
- Legal chunks MUST preserve natural legal citation units: section/subsection,
  paragraph, rule number, or form number.
- Freshness is part of correctness: mutable primary sources, rules, and forms
  MUST have an explicit update cadence and owner recorded in the implementation
  plan or corpus task.
- High-risk legal flows MUST define clarifying-question behavior, escalation
  copy, and the point at which the system stops short of individualized advice.
- POPIA-relevant changes MUST document the data purpose, storage duration,
  deletion or de-identification path, breach workflow, and vendor transfer
  posture.

## Mandatory Reference Documents

These documents are authoritative and MUST be consulted before writing or
reviewing any code:

| Document | Scope | Authority |
|---|---|---|
| `docs/BACKEND_STRUCTURE.md` | .NET/ABP backend layer rules, entity patterns, DbContext, migrations | Architecture reference for all backend work |
| `docs/BP.md` | C# and Next.js coding best practices | Coding standards guide for all C# and frontend work |
| `docs/RULES.md` | Enforceable C# coding rules (length, naming, guard clauses, formatting) | Non-negotiable quality gate for all C# code reviews |
| `docs/deep-research-report.md` | South African legal AI safety, authority hierarchy, escalation, and evaluation requirements | Non-negotiable quality gate for legal-AI features |
| `docs/research_legislation.md` | South African corpus scope, licensing posture, and official-source rules | Non-negotiable quality gate for corpus and ingestion work |

Rules derived from these documents that MUST be enforced in every PR:

**C# (from `docs/RULES.md` and `docs/BP.md`)**:
- All classes MUST have a purpose comment unless self-evident to a junior
  developer.
- All public methods MUST have a purpose comment.
- Non-obvious logic MUST be preceded by an explanatory comment.
- Classes MUST NOT exceed 350 lines; methods MUST NOT require vertical
  scrolling.
- Nesting MUST NOT exceed two levels deep - refactor with early returns or
  extracted methods.
- Guard clauses using `Ardalis.GuardClauses` MUST validate all preconditions at
  the top of complex methods.
- Variable names MUST be clear and correctly spelled; `var` MUST only be used
  when the type is obvious from the right-hand side.
- Magic numbers MUST be replaced with named `const` values or `enum` types.
- Duplicated logic MUST be extracted into reusable methods (DRY).
- Dead code (unused methods, variables, commented-out blocks) MUST be deleted
  before merging.
- Code MUST be formatted before committing.
- Performance: loops MUST NOT cause multiple database calls; bulk operations are
  preferred.
- Database indexes MUST be added for frequently queried fields.

**Backend Architecture (from `docs/BACKEND_STRUCTURE.md`)**:
- Every domain entity MUST extend `FullAuditedEntity<Guid>`.
- Every domain entity MUST have a corresponding `DbSet<T>` in `backendDbContext`.
- Application services MUST extend `AsyncCrudAppService` (CRUD) or
  `ApplicationService` (custom).
- Every service class MUST have a corresponding interface
  (`I{Entity}AppService`).
- DTOs MUST be decorated with `[AutoMap(typeof(TEntity))]`.
- DTOs MUST NOT expose EF navigation properties directly.
- Domain logic MUST NOT be placed in the Application layer.
- DTO classes MUST NOT be placed in the domain layer (except domain-service
  parameters).
- All controllers MUST inherit from `backendControllerBase`.

**Frontend (from `docs/BP.md`)**:
- File-based routing conventions MUST be followed in Next.js.
- SSR (`getServerSideProps`) MUST only be used for pages that genuinely require
  real-time data.
- Next.js `Image` component MUST be used for all images.
- TypeScript MUST be used throughout the frontend.
- Global state MUST be kept minimal; prefer props over global state.
- Third-party dependencies MUST be justified by necessity.

**Legal AI and Corpus Governance (from `docs/deep-research-report.md` and
`docs/research_legislation.md`)**:
- Binding law MUST be retrieved and ranked above official guidance when both
  exist.
- Court authority weighting MUST prefer Constitutional Court, then Supreme Court
  of Appeal, then the most relevant High Court before any lower-authority
  material.
- Every legal claim MUST satisfy the "no citation, no claim" rule.
- If authoritative support is missing, the system MUST ask clarifying questions,
  limit the answer, or escalate - never guess.
- User-facing legal responses MUST distinguish binding law from official
  guidance whenever both appear.
- Official forms, guides, and regulator material MUST be treated as procedure
  support rather than authoritative substitutes for law.
- Corpus ingestion MUST prefer official/public sources such as gov.za,
  Government Gazette materials, justice.gov.za forms/guidance, apex-court
  repositories, and official regulator portals.
- Commercial, unofficial, or licensing-sensitive compiled materials MUST remain
  link-only unless explicit ingestion rights are recorded.
- Corpus or ingestion changes MUST define provenance metadata, freshness/update
  cadence, and licensing posture.
- High-risk legal flows MUST define escalation paths and human-help messaging
  before release.
- POPIA-sensitive changes MUST define retention, deletion or de-identification,
  security, breach handling, and cross-border vendor posture.

## Skill Usage Policy

When implementing tasks, agents MUST use the available project skills where
applicable rather than writing boilerplate from scratch:

| Task Type | Skill to Use |
|---|---|
| Scaffold a new ABP CRUD endpoint | `add-endpoint` |
| Create or update auth provider | `add-auth-provider` |
| Apply provider/state pattern | `apply-provider-pattern` |
| Add or update styles | `add-styling` |
| Create or amend the constitution | `speckit-constitution` |
| Create feature specification | `speckit-specify` |
| Plan implementation | `speckit-plan` |
| Generate tasks | `speckit-tasks` |
| Execute implementation | `speckit-implement` |
| Analyze generated spec artifacts | `speckit-analyze` |
| Generate a feature checklist | `speckit-checklist` |
| Follow git workflow | `follow-git-workflow` |

Using a skill when one exists is MANDATORY. Deviating requires documented
justification in the PR.

## Technology & Delivery Constraints

- Backend MUST be implemented with .NET 9 and ABP Framework as described in
  `docs/BACKEND_STRUCTURE.md`.
- Frontend MUST be implemented with Next.js and Ant Design.
- Primary persistence MUST use PostgreSQL; vector search MAY use in-memory
  cosine similarity for MVP and can evolve to pgvector for scale.
- AI stack MUST use OpenAI-compatible services for embeddings, multilingual
  generation, transcription (Whisper), and text-to-speech, with
  environment-based key management.
- Knowledge ingestion MUST support structured ETL chunking of legislation:
  Extract (PdfPig + OCR fallback) -> Transform (chapter/section parsing, topic
  enrichment, authority labeling) -> Load (embed + store). Each stage MUST be
  tracked in IngestionJob with duration, chunk counts, source provenance, and
  error details.
- Corpus ingestion MUST prefer official/public sources and MUST record source
  authority, document type, authority type, provenance URL, and effective or
  last-seen metadata needed for auditing and freshness review.
- Contract analysis MUST include type detection, health scoring (0-100),
  plain-language summary, and legislation-backed red flag flagging for MVP
  contract types (employment, lease, credit, service).
- Smart document comparison MUST accept two contracts of the same type, generate
  per-aspect comparison points (ContractA value, ContractB value, winner,
  legislation citation), and produce an overall recommendation stored as
  ComparisonPoint entities.
- Rights violation letter generation MUST produce multilingual formal demand
  letters citing the specific legislation violated, support PDF export via
  StoredFile, and display a disclaimer.
- Community insights MUST aggregate anonymized platform statistics (contract
  health scores, top violations, category distribution, language trends) and
  MUST NOT expose any PII.
- Deployment MUST support CI/CD with environment separation and secret handling
  (Azure DevOps or GitHub Actions).

## Development Workflow & Quality Gates

- Work items MUST map to approved backlog issues and milestone outcomes.
- Before writing any backend code, the author MUST consult
  `docs/BACKEND_STRUCTURE.md` to confirm the correct layer, naming convention,
  and entity pattern.
- Before writing or reviewing any C# code, the author MUST apply the rules in
  `docs/RULES.md` and the practices in `docs/BP.md`. Non-compliant code MUST
  NOT be merged.
- Before changing any legal-AI behavior, the author MUST consult
  `docs/deep-research-report.md` and confirm source authority order, escalation
  behavior, and evaluation expectations.
- Before changing corpus scope or ingestion, the author MUST consult
  `docs/research_legislation.md` and confirm official-source preference,
  licensing posture, freshness expectations, and bundle composition.
- Any AI-facing endpoint MUST define prompt contract, citation behavior,
  fallback behavior, and how it distinguishes law from official guidance.
- Any high-risk legal flow MUST define clarifying-question behavior and
  escalation copy to human or official help before release.
- Any new document source MUST be validated for licensing/public availability,
  provenance metadata, and freshness ownership before ingestion.
- Any change that stores or exports personal information MUST document POPIA
  purpose, retention, deletion or de-identification path, breach workflow, and
  cross-border vendor posture.
- Pull requests MUST include evidence for changed behavior (unit/integration/UI
  tests, benchmark cases, or validation checklists as applicable) and explicit
  accessibility impact notes for frontend changes.
- Pull requests for legal-AI features MUST include evaluation evidence for
  grounded citations, guidance-versus-law labeling, and at least one adversarial
  or wrong-source-hint scenario.
- Release readiness MUST verify: disclaimer rendering, multilingual behavior,
  citation presence, guidance labeling, role-based access for admin functions,
  high-risk escalation messaging, community insights data anonymization, and
  POPIA safeguards for any new data flow.
- Major architectural changes MUST include migration notes for data model and
  pipeline impacts.
- All implementation tasks MUST use the relevant skill listed in the Skill Usage
  Policy above before falling back to manual scaffolding.

## Constitution Check Gates (for `speckit.plan`)

Every implementation plan MUST pass these gates before Phase 0 research:

1. **Layer Gate**: Does every new entity belong to the correct DDD layer per
   `docs/BACKEND_STRUCTURE.md`? (Core = entity, Application = service+DTO,
   EFCore = DbSet)
2. **Naming Gate**: Do all new services, DTOs, and controllers follow naming
   conventions in `docs/BACKEND_STRUCTURE.md`?
3. **Coding Standards Gate**: Does the planned implementation approach comply
   with `docs/RULES.md` (length, nesting, guard clauses, naming)?
4. **Skill Gate**: Has the author checked the Skill Usage Policy and identified
   which skills apply to this feature's tasks?
5. **Multilingual Gate**: Are all user-facing outputs planned for all four
   supported languages?
6. **Authority Gate**: Do AI-facing outputs preserve primary-source-first
   behavior, authority weighting, and explicit law-versus-guidance labeling?
7. **Citation Gate**: Do all AI-facing endpoints define their RAG contract,
   citation format, and fallback behavior?
8. **Safety Gate**: Are clarifying questions, high-risk escalation triggers, and
   human-help messaging planned for ambiguous or urgent legal flows?
9. **Accessibility Gate**: Are keyboard navigation, screen reader semantics,
   plain-language output, and mobile-first delivery planned for new frontend
   components?
10. **Data Governance Gate**: If personal data, uploads, logs, or vendors
    change, are POPIA retention, security, breach, and cross-border
    implications documented?
11. **Corpus Governance Gate**: If the feature adds or modifies documents or
    ingestion, are official-source preference, licensing posture, provenance
    metadata, and freshness ownership specified?
12. **ETL/Ingestion Gate**: If the feature adds or modifies document ingestion,
    is an IngestionJob entity used to track all pipeline stages with status,
    timing, provenance, and error details?

## Governance

This constitution overrides conflicting implementation preferences for the
project.

Amendment policy:
- Amendments require documented rationale linked to specification, research, or
  governance changes.
- Every amendment MUST update the version and amendment date.
- MAJOR version increments apply to breaking governance changes.
- MINOR version increments apply to new principles or materially expanded rules.
- PATCH version increments apply to clarifications without behavioral
  requirement changes.

Compliance policy:
- Reviews MUST verify adherence to all core principles and the South Africa
  legal research inputs when applicable.
- Any exception MUST be explicit, time-bound, and approved with a remediation
  plan.
- Non-compliant changes MUST NOT be merged without recorded exception approval.

**Version**: 1.3.0 | **Ratified**: 2026-03-26 | **Last Amended**: 2026-04-01
