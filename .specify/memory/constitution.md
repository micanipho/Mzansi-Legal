<!--
Sync Impact Report
- Version change: template -> 1.0.0
- Modified principles: placeholders replaced with 5 enforceable project principles
- Added sections: Technology & Delivery Constraints; Development Workflow & Quality Gates
- Removed sections: none
- Templates requiring updates:
	- .specify/templates/plan-template.md (⚠ pending manual alignment check)
	- .specify/templates/spec-template.md (⚠ pending manual alignment check)
	- .specify/templates/tasks-template.md (⚠ pending manual alignment check)
- Follow-up TODOs:
	- Confirm whether accessibility targets in CI will be enforced as blocking gates or release gates
-->

# MzansiLegal Constitution

## Core Principles

### I. Multilingual-First User Experience
Every user-facing capability MUST support English, isiZulu, Sesotho, and Afrikaans for MVP,
including text input, voice input, output responses, and UI labels where applicable. Retrieval and
citations MUST remain grounded in English source legislation, while answers MUST be returned in
the detected or selected user language. New language additions MUST not require re-indexing of
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
The domain model MUST follow DDD composition rules: child entities declare PartOf ownership,
enumerations use controlled RefList values, and binary assets use StoredFile patterns. User-specific
artifacts (conversations, answers, contract analyses, uploads) MUST be authenticated and scoped to
their owning AppUser except explicitly published FAQs. Cross-aggregate references are allowed only
for traceability requirements such as citation linkage to document chunks.

### V. Responsible Delivery and Operational Measurability
Each feature MUST ship with measurable outcomes aligned to success metrics: citation quality,
latency, language detection accuracy, accessibility conformance, and ingestion reliability. CI/CD
automation MUST build, test, and deploy both backend and frontend through a repeatable pipeline.
Legal and financial disclaimers MUST be present in relevant user flows and localized for supported
languages.

## Technology & Delivery Constraints

- Backend MUST be implemented with .NET 8 and ABP Framework.
- Frontend MUST be implemented with Next.js and Ant Design.
- Primary persistence MUST use PostgreSQL; vector search MAY use in-memory cosine similarity
	for MVP and can evolve to pgvector for scale.
- AI stack MUST use OpenAI-compatible services for embeddings, multilingual generation,
	transcription, and text-to-speech, with environment-based key management.
- Knowledge ingestion MUST support structured chunking of legislation and contract text extraction.
- Contract analysis MUST include type detection, health scoring, summary generation, and
	legislation-backed flagging.
- Deployment MUST support Azure DevOps CI/CD with environment separation and secret handling.

## Development Workflow & Quality Gates

- Work items MUST map to approved backlog issues and milestone outcomes.
- Any AI-facing endpoint MUST define prompt contract, citation behavior, and fallback behavior.
- Any new document source MUST be validated for licensing/public availability before ingestion.
- Pull requests MUST include test evidence for changed behavior (unit/integration/UI where
	applicable) and explicit accessibility impact notes for frontend changes.
- Release readiness MUST verify: disclaimer rendering, multilingual behavior, citation presence,
	and role-based access for admin functions.
- Major architectural changes MUST include migration notes for data model and pipeline impacts.

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
- Non-compliant changes MUST not be merged without recorded exception approval.

**Version**: 1.0.0 | **Ratified**: 2026-03-26 | **Last Amended**: 2026-03-26
