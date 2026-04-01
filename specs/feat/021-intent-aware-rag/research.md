# Research: Intent-Aware Legal Retrieval for RAG Answers

**Feature**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01

## Decision Log

---

### R-001: Retrieval Architecture - Hybrid Semantic Retrieval with Document Reranking

**Decision**: Keep the existing embedding-based semantic search as the first pass, but replace the current "top-5 chunks above one threshold" rule with a two-stage retrieval flow:

1. Generate a wider chunk candidate set from semantic similarity.
2. Aggregate those candidates to the document level.
3. Rerank documents using semantic strength plus metadata alignment.
4. Select final context chunks from the top-ranked documents.

**Rationale**: The current implementation already searches by meaning at the chunk level, but it answers too early from whichever chunks clear a fixed threshold. That works best when the user uses vocabulary that is already close to the indexed text. Document-level reranking makes the system better at finding the governing Act even when the user only describes the problem in plain language.

**Alternatives considered**:
- **Pure chunk top-K with threshold only**: Too brittle for plain-language legal questions and often overfits isolated passages.
- **pgvector / ANN index**: Useful for scale, but unnecessary for the current corpus size and not required to improve source selection quality.
- **Lexical-only search**: Rejected because it would regress the current semantic behavior that already helps with paraphrases.

---

### R-002: Use Existing Enrichment Metadata Instead of Adding New Search Infrastructure

**Decision**: Reuse the metadata already present on `DocumentChunk` and related models:

- `DocumentChunk.Keywords`
- `DocumentChunk.TopicClassification`
- `LegalDocument.Title`
- `LegalDocument.ShortName`
- `LegalDocument.ActNumber`
- `Category.Name`

No new database tables or search service will be added in this milestone.

**Rationale**: The ETL pipeline already enriches chunks with topic and keyword data. That metadata is exactly what the RAG service needs to improve meaning-based source discovery without introducing another store or migration. Using it in memory keeps the request path fast and implementation risk low.

**Alternatives considered**:
- **Add a source-alias table**: Better long-term for curation, but not necessary for this refinement.
- **Add BM25/full-text infrastructure**: Valuable later for hybrid IR, but too much operational scope for this feature.
- **Store per-document embeddings**: Possible, but the current chunk embeddings plus metadata are sufficient for the current corpus size.

---

### R-003: Explicit Act Names Are Strong Hints, Not Hard Filters

**Decision**: If the user names an Act, short name, or common source label, treat it as a ranking boost only. Do not filter all other sources out.

**Rationale**: Users may misremember Act names, abbreviations, or cite a secondary law while the facts actually point to a different primary source. Hard-filtering to named Acts would make the assistant brittle and would fail the spec requirement that the system still consider other clearly relevant sources.

**Alternatives considered**:
- **Hard filter to named sources**: Too risky when the user is mistaken.
- **Ignore explicit names completely**: Wastes a valuable relevance signal when the user does know the correct source.

---

### R-004: Confidence Must Be Computed from Retrieval Signals, Not Model Self-Reporting

**Decision**: Add an explicit confidence evaluation step that uses deterministic retrieval signals:

- best document score
- gap between the top document and close alternatives
- number and spread of strong supporting chunks
- agreement across topic/category metadata
- ambiguity detected from the question wording or overly broad scope

These signals determine `RagConfidenceBand` and `RagAnswerMode`.

**Rationale**: Confidence needs to be testable and stable. Letting the language model tell us how confident it feels would be harder to verify and easier to regress. Retrieval signals provide a more transparent safety boundary.

**Alternatives considered**:
- **LLM self-reported confidence**: Too subjective and difficult to unit test.
- **Single numeric threshold only**: Too simplistic for cases where scores are decent but split across conflicting source groups.

---

### R-005: Response Modes Should Be Explicit and Backward-Compatible

**Decision**: Extend `RagAnswerResult` with structured response-state metadata while keeping all current fields:

- `answerMode`
- `confidenceBand`
- `clarificationQuestion`

Current fields like `answerText`, `isInsufficientInformation`, `citations`, `chunkIds`, `answerId`, and `detectedLanguageCode` remain.

**Rationale**: The frontend needs structured state to distinguish a confident answer from a cautious answer or clarification request. Keeping the existing shape preserves compatibility with current consumers and avoids breaking the Ask page during rollout.

**Alternatives considered**:
- **Backend-only textual changes**: Cheaper but makes caution/clarification behavior harder to surface consistently in the UI.
- **Brand-new endpoint version**: Cleaner contract boundary, but unnecessary for this small contract expansion.

---

### R-006: Temperature Should Vary by Response Mode but Stay Low Overall

**Decision**: Use a bounded dynamic temperature policy:

- `Direct`: 0.2
- `Cautious`: 0.1
- `Clarification`: 0.0
- `Insufficient`: no open-ended legal answer generation; use a deterministic insufficiency response

**Rationale**: This feature specifically calls for more controlled generation behavior. In a legal domain, temperature should remain low across all modes, but the assistant should become even less creative when evidence is weak or when it needs to ask for clarification.

**Alternatives considered**:
- **Fixed 0.2 everywhere**: Simpler, but does not reflect the requested adaptive behavior.
- **0.0 everywhere**: Safest, but tends to read too stiffly for grounded explanatory answers.
- **Higher temperature for clarification**: Rejected because even clarification prompts should remain precise and focused.

---

### R-007: Remove the General-Knowledge Legal Fallback from `/api/app/qa/ask`

**Decision**: The ask endpoint should no longer produce a general-knowledge legal answer when no grounded legislation is found. Instead it should return either:

- a clarification request, or
- a grounded insufficiency response

**Rationale**: The current fallback contradicts the core RAG safety promise and the new feature spec. For a legal assistant, a disclaimer plus uncited legal guidance is still too risky. This feature exists specifically to improve grounded document discovery and conservative behavior when grounding is weak.

**Alternatives considered**:
- **Keep the fallback with a stronger disclaimer**: Still too risky and does not satisfy the spec.
- **Silent failure / empty response**: Poor user experience and not actionable.

---

### R-008: No New Migration for This Feature

**Decision**: No EF Core migration is needed.

**Rationale**: The retrieval improvement uses existing source metadata, existing answer persistence, and response DTO changes only. There is no requirement to persist confidence, response mode, or document ranking results in the database for this milestone.

**Alternatives considered**:
- **Persist answer mode and confidence to `Answer`**: Useful for analytics, but not required to meet the current spec and would add migration scope.
- **Add a retrieval diagnostics table**: Valuable later for tuning and telemetry, but premature here.

---

### R-009: Frontend Change Scope - Small Ask-Page Enhancement Only

**Decision**: Limit frontend work to the existing Ask-page chat consumer:

- update response types
- propagate new fields through chat state
- render localized caution/clarification notices

No new route, no new design system, and no new page flow.

**Rationale**: The spec does not require a new UI journey. The existing Ask page already renders answer text and citations; it only needs a structured way to reflect the safer response modes introduced by this feature.

**Alternatives considered**:
- **Backend only**: Works technically, but users would not reliably see why a response is cautious or why a clarification is being requested.
- **Large Ask-page redesign**: Unnecessary for this retrieval refinement milestone.

---

### R-010: Calibrate Retrieval and Safety with a Small Benchmark Set Before Declaring the Feature Done

**Decision**: Maintain a lightweight benchmark prompt pack that covers:

- plain-language questions without Act names
- semantically equivalent phrasing variants
- wrong explicit Act hints
- multi-source legal questions
- short ambiguous prompts
- unsupported questions

These benchmark cases will be used to tune retrieval weights and answer-mode thresholds before implementation is considered complete.

**Rationale**: The deep research report emphasizes retrieval quality, groundedness, and human-reviewable evaluation. For this milestone, a compact benchmark set gives the team a repeatable way to calibrate the existing system without adding new telemetry tables or production analytics infrastructure.

**Alternatives considered**:
- **Ad-hoc manual testing only**: Too easy to forget edge cases and too hard to repeat after code changes.
- **Production-log-driven tuning only**: Too slow and too risky for a legal assistant safety feature.

---

### R-011: Keep This Milestone Legislation-First and Defer Court-Hierarchy Weighting Until Judgments Are Ingested

**Decision**: This milestone will keep the retriever focused on the current legislation corpus and existing document metadata. Court-hierarchy and precedential-weight ranking from the deep research report are recorded as a future refinement once judgments and court-level metadata are part of the indexed corpus.

**Rationale**: The current feature scope and existing data model are centered on Acts, categories, chunks, and chunk enrichment metadata. Adding authority-weight logic without judgment coverage would create false precision and distract from the more immediate need: better Act discovery, safer confidence handling, and clearer response modes.

**Alternatives considered**:
- **Introduce court-weighting now**: Misaligned with the current corpus and likely to increase complexity without improving this milestone's real behavior.
- **Expand the corpus inside this feature**: Too much scope for a retrieval-hardening milestone with no planned migrations or new infrastructure.

---

### R-012: Clarification and Insufficient Responses Are Correct Safety Outcomes, Not UX Failures

**Decision**: Treat `Clarification` and `Insufficient` as first-class, testable success states when the system lacks decisive grounding. Evaluation and UI behavior must reflect that a safe follow-up question or grounded refusal is the correct response in some legal scenarios.

**Rationale**: The deep research report frames legal-assistant quality as a safety problem as much as a retrieval problem. If the system is broad, ambiguous, or under-grounded, asking one focused question or declining is more correct than forcing a direct answer.

**Alternatives considered**:
- **Optimize mostly for direct answers**: Encourages overconfident legal responses.
- **Treat all non-answer states as failure**: Hides the safety value of clarification and grounded refusal behavior.

---

## Summary

All planning unknowns were resolved without introducing new infrastructure or new database schema. The feature will improve source discovery by combining semantic chunk retrieval with metadata-aware document reranking, replace overconfident fallback behavior with explicit response modes, and expose enough structured metadata for the existing Ask page to represent those safer behaviors clearly.
