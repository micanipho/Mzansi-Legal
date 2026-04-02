# Research: Contract Analysis

**Feature**: `feat/022-contract-analysis` | **Date**: 2026-04-02

## Decision Log

---

### R-001: Reuse the existing `ContractAnalysis` aggregate instead of inventing a second persistence model

**Decision**: Build the feature on top of the already-existing `ContractAnalysis` and `ContractFlag` domain entities and their current EF mapping.

**Rationale**: The repo already contains the right aggregate root for MVP contract analysis: ownership, extracted text, contract type, health score, summary, language, timestamp, and child flags are already modeled and migrated. Reusing that aggregate keeps the implementation aligned with the codebase and avoids a second "contract result" model.

**Alternatives considered**:
- **Create a brand-new contract review aggregate**: Rejected because it duplicates an existing persisted model.
- **Store only transient JSON results**: Rejected because the feature requires saved history and user-owned retrieval.

---

### R-002: Keep upload plumbing in a thin controller, but keep analysis logic in application services

**Decision**: Expose the upload route through a lightweight `ContractController` in `backend.Web.Host`, with actual orchestration delegated to `IContractAppService` and focused helper services in `backend.Application`.

**Rationale**: Multipart PDF upload is a web concern, while classification, OCR fallback, legislation retrieval, JSON parsing, persistence, and follow-up logic are application concerns. This matches the repo's architecture guidance and avoids embedding file-processing logic in the controller.

**Alternatives considered**:
- **Put everything in the controller**: Rejected because it violates the repo's layering rules.
- **Force upload through a generic ABP CRUD surface**: Rejected because file upload and analysis are not normal CRUD.

---

### R-003: Reuse the repo's PdfPig extraction approach, but separate text extraction from legislation chunking

**Decision**: Reuse the proven PdfPig word-level extraction strategy already present in `PdfIngestionAppService`, but refactor or wrap it as a shared text-extraction helper for contracts instead of reusing the legislation chunking pipeline directly.

**Rationale**: The existing ingestion service solves an important South African PDF problem: many PDFs need word-level spatial grouping rather than naive `page.Text`. Contracts need that extraction quality, but they do not need `IngestionJob` stage transitions or section chunking.

**Alternatives considered**:
- **Duplicate the PdfPig logic inside a new contract service**: Rejected because it would drift from the existing extraction behavior.
- **Route contract uploads through the full legislation ETL pipeline**: Rejected because contracts are user documents, not catalogued legislation.

---

### R-004: Trigger OCR fallback only when direct text extraction is materially insufficient

**Decision**: If PdfPig extraction yields fewer than 100 meaningful characters, invoke OCR fallback using the OpenAI-compatible vision path before concluding the contract is unreadable.

**Rationale**: This keeps text-first PDFs cheap and fast while still supporting scanned or image-heavy uploads. The 100-character threshold is also concrete enough to test.

**Alternatives considered**:
- **Always run OCR**: Rejected because it is slower, costlier, and unnecessary for most PDFs.
- **Never run OCR**: Rejected because the specification explicitly requires best-effort handling for scanned contracts.

---

### R-005: Constrain contract type detection to the four supported families and fail safely otherwise

**Decision**: Use a lightweight GPT-4o classification call over the first 500 characters, constrained to `Employment`, `Lease`, `Credit`, or `Service`. If the result is weak or unsupported, stop normal analysis and return a clear unsupported response.

**Rationale**: The feature scope is intentionally limited, and the legal coverage in the current corpus is not broad enough to safely infer arbitrary contract families. Constrained classification keeps the analyzer honest.

**Alternatives considered**:
- **Heuristic keyword-only classification**: Simpler, but less reliable for mixed or plain-language contracts.
- **Allow free-form model classifications**: Rejected because they would create unsupported pseudo-types that the corpus cannot back.

---

### R-006: Build analysis prompts as structured JSON contracts, not prose-only completions

**Decision**: Require the analysis model to return JSON containing:

- `healthScore`
- `summary`
- `flags[]` with `severity`, `title`, `description`, `clauseText`, and `legislationCitation`

The backend will parse and validate that structure before persistence.

**Rationale**: Contract analysis is a workflow output, not just free text. A structured response is easier to test, easier to normalize, and safer to persist.

**Alternatives considered**:
- **Free-form prose with regex extraction**: Too brittle for a legal workflow.
- **Multiple separate model calls for score, summary, and flags**: More expensive and harder to keep internally consistent.

---

### R-007: Definitive legal flags require grounded legislation citations; unsupported issues must downgrade

**Decision**: A finding may only stay as a definitive legal red flag if the cited legislation chunks support the claim. If support is weak, missing, or outside the current corpus, the system must downgrade the finding into cautionary or review-needed wording.

**Rationale**: The repo research is explicit that primary-law grounding and citation contracts are safety boundaries, not optional polish. This matters especially for contract issues where the current corpus is incomplete.

**Alternatives considered**:
- **Let the LLM cite from training knowledge**: Rejected because it breaks the citation contract.
- **Suppress all weakly grounded findings**: Too conservative; users still benefit from plain-language caution if it is labeled honestly.

---

### R-008: Reuse the existing legislation corpus and RAG planner for contract analysis instead of adding a second retriever

**Decision**: Generate contract-analysis legislation context with the existing in-memory legal corpus and helper stack (`RagIndexStore`, `RagSourceHintExtractor`, `RagRetrievalPlanner`, `RagConfidenceEvaluator`), using contract type and extracted terms as query signals.

**Rationale**: The project already has a retrieval system for South African law. Contract analysis needs a different query shape, not a separate search engine.

**Alternatives considered**:
- **Dedicated contract-only keyword search**: Too weak for paraphrased or clause-heavy language.
- **New vector database or new indexing layer**: Unnecessary for the current corpus size and scope.

---

### R-009: Contract coverage must reflect the actual seeded corpus, not the full legal domain

**Decision**: Plan and verification must distinguish what the current corpus can genuinely support:

- **Employment**: strong baseline via `BCEA` and `LRA`
- **Credit**: strong baseline via `NCA` and `CPA`
- **Service**: moderate baseline via `CPA`
- **Lease**: moderate baseline via `RHA` and `Constitution`, but weaker for eviction/procedure because `PIE` is not yet ingested

**Rationale**: `docs/research_legislation.md` explicitly identifies missing source bundles that matter to lease and procedural rights. The analyzer must not treat those gaps as though they do not exist.

**Alternatives considered**:
- **Pretend all four contract families are equally covered**: Rejected because it would create false confidence.
- **Block lease analysis entirely until `PIE` is added**: Too restrictive for MVP because many lease terms are still analyzable through `RHA` and the Constitution.

---

### R-010: Follow-up Q&A should be contract-aware but not require saved chat persistence for MVP

**Decision**: Add a contract-specific follow-up endpoint that uses the stored contract analysis and legislation corpus as context, but do not add new persistence for follow-up chat history in MVP.

**Rationale**: The acceptance criteria require correct contract-aware answers, not saved threaded history. Avoiding new follow-up persistence keeps MVP aligned with the current domain model.

**Alternatives considered**:
- **Persist follow-up Q&A in the existing QA domain immediately**: Valuable later, but it introduces cross-aggregate design decisions that are not required for MVP acceptance.
- **Send users back to the generic ask endpoint**: Rejected because it loses contract context.

---

### R-011: Analysis and follow-up output must stay multilingual even though citations remain English

**Decision**: Generate summaries, flag descriptions, failure messages, and follow-up answers in the user's current app language (`en`, `zu`, `st`, `af`), while keeping Act names, section numbers, and clause excerpts in their original legal wording.

**Rationale**: The constitution requires multilingual user-facing behavior, and the contract domain already has a `Language` field on `ContractAnalysis`. Localized prose with English citations mirrors the current multilingual RAG stance.

**Alternatives considered**:
- **English-only contract output**: Rejected by the multilingual product constraints.
- **Translate Act names and section references**: Rejected because it weakens citation fidelity.

---

### R-012: Uploaded contracts are high-sensitivity content and must stay private-by-default

**Decision**: Contract uploads and results remain scoped to the authenticated user, are never public FAQ content, and must not be logged in plaintext. External AI calls are treated as cross-border/high-sensitivity processing that must align with the repo's POPIA-aware posture.

**Rationale**: `docs/deep-research-report.md` frames legal AI as a governance problem as much as a model problem. Contracts often contain identity, salary, banking, or home-address data, so privacy must shape the design.

**Alternatives considered**:
- **Treat contracts like generic ask prompts**: Rejected because the privacy sensitivity is materially higher.
- **Log raw contract text for debugging**: Rejected because it conflicts with POPIA-oriented minimization and security principles.

---

### R-013: Frontend work should replace the existing demo contracts flow instead of introducing a second contracts UI

**Decision**: Keep the existing `/contracts` and `/contracts/[id]` routes, replace provider demo data with real APIs, and attach follow-up interaction to the contract detail experience.

**Rationale**: The repo already has a contracts shell, detail route, and auth guard. Replacing mock wiring is lower risk and keeps the user journey coherent.

**Alternatives considered**:
- **Create a separate upload wizard and separate follow-up page**: Rejected because the repo already has the right route structure.
- **Keep the current "ask about contract" generic Ask-page bridge**: Rejected because it loses contract-specific context.

---

### R-014: Missing high-value legal bundles stay explicit follow-on work

**Decision**: Treat source gaps surfaced by contract analysis as explicit corpus-expansion backlog, especially:

1. `PIE` for lease/eviction procedure
2. later case-law bundles for contract interpretation depth
3. any sector-specific service-contract bundles that prove necessary in production

These expansions must continue to use `LegislationManifest`, the ETL pipeline, and `IngestionJob`.

**Rationale**: The repo research already establishes a public-source-first, staged corpus strategy. Contract analysis should plug into that roadmap rather than bypass it.

**Alternatives considered**:
- **Silently stretch the current corpus assumptions**: Rejected because it obscures real coverage limits.
- **Create an ad-hoc contract-law document loader**: Rejected because it violates the repo's ingestion governance direction.

---

## Summary

The design for contract analysis stays deliberately close to the existing repo architecture: use the current contract aggregate, reuse the proven PdfPig extraction approach, reuse the current legal RAG corpus for legislation grounding, and keep all user-facing outputs multilingual and private. The research documents matter directly here: they force the feature to stay primary-source-first, POPIA-aware, and explicit about contract-law coverage gaps such as `PIE`, rather than overpromising legal certainty.
