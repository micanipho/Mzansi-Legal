# Research: Intent-Aware Legal Retrieval for RAG Answers

**Feature**: `feat/021-intent-aware-rag` | **Date**: 2026-04-01

## Decision Log

---

### R-001: Retrieval Architecture - Hybrid Semantic Retrieval with Document Reranking

**Decision**: Keep the existing embedding-based semantic search as the first pass, but replace the current "top chunks above one threshold" behavior with a two-stage retrieval flow:

1. Generate a wider chunk candidate set from semantic similarity.
2. Aggregate those candidates to the document level.
3. Rerank documents using semantic strength plus metadata alignment.
4. Select final context chunks from the top-ranked documents.

**Rationale**: The current implementation already searches by meaning at the chunk level, but it answers too early from whichever chunks clear a fixed threshold. Document-level reranking makes the system better at finding the governing source when the user only describes the problem in plain language.

**Alternatives considered**:
- **Pure chunk top-K with threshold only**: Too brittle for plain-language legal questions and often overfits isolated passages.
- **pgvector or ANN index**: Useful for scale, but unnecessary for the current corpus size and not required to improve source selection quality in this milestone.
- **Lexical-only search**: Rejected because it would regress the semantic behavior that already helps with paraphrases.

---

### R-002: Use Existing Enrichment Metadata Instead of Adding New Search Infrastructure

**Decision**: Reuse the metadata already present on `DocumentChunk` and related models:

- `DocumentChunk.Keywords`
- `DocumentChunk.TopicClassification`
- `LegalDocument.Title`
- `LegalDocument.ShortName`
- `LegalDocument.ActNumber`
- `Category.Name`

No new database tables or search service will be added for the retrieval-hardening slice.

**Rationale**: The ETL pipeline already enriches chunks with topic and keyword data. That metadata is enough to improve meaning-based source discovery without introducing another store or migration. Using it in memory keeps the request path fast and implementation risk low.

**Alternatives considered**:
- **Add a source-alias table**: Better long-term for curation, but not necessary for this refinement.
- **Add BM25 or full-text infrastructure**: Valuable later for hybrid IR at scale, but too much operational scope for this feature.
- **Store per-document embeddings**: Possible, but the current chunk embeddings plus metadata are sufficient for the current corpus size.

---

### R-003: Explicit Act Names Are Strong Hints, Not Hard Filters

**Decision**: If the user names an Act, short name, or common source label, treat it as a ranking boost only. Do not filter all other sources out.

**Rationale**: Users may misremember Act names, abbreviations, or cite a secondary law while the facts actually point to a different primary source. Hard-filtering to named Acts would make the assistant brittle and would fail the requirement that the system still consider other clearly relevant sources.

**Alternatives considered**:
- **Hard filter to named sources**: Too risky when the user is mistaken.
- **Ignore explicit names completely**: Wastes a valuable relevance signal when the user does know the correct source.

---

### R-004: Confidence Must Be Computed from Retrieval Signals, Not Model Self-Reporting

**Decision**: Keep an explicit confidence evaluation step that uses deterministic retrieval signals:

- best document score
- gap between the top document and close alternatives
- number and spread of strong supporting chunks
- agreement across topic and category metadata
- ambiguity detected from the question wording, missing facts, or overly broad scope
- risk triggers such as urgency, imminent deadlines, arrest, or safety concerns

These signals determine `RagConfidenceBand` and `RagAnswerMode`.

**Rationale**: Confidence needs to be testable and stable. Letting the language model tell us how confident it feels would be harder to verify and easier to regress. Retrieval and risk signals provide a more transparent safety boundary.

**Alternatives considered**:
- **LLM self-reported confidence**: Too subjective and difficult to unit test.
- **Single numeric threshold only**: Too simplistic for cases where scores are decent but split across conflicting source groups or where the facts are urgent but incomplete.

---

### R-005: Response Modes Should Stay Explicit and Backward-Compatible

**Decision**: Keep `RagAnswerResult` append-only and structured:

- `answerMode`
- `confidenceBand`
- `clarificationQuestion`

Current fields like `answerText`, `isInsufficientInformation`, `citations`, `chunkIds`, `answerId`, and `detectedLanguageCode` remain. Any new citation metadata for source labeling must also be additive.

**Rationale**: The frontend needs structured state to distinguish a confident answer from a cautious answer or clarification request. Keeping the existing shape preserves compatibility with current consumers and avoids breaking the Ask page during rollout.

**Alternatives considered**:
- **Backend-only textual changes**: Cheaper but makes caution, clarification, and source-role behavior harder to surface consistently in the UI.
- **Brand-new endpoint version**: Cleaner contract boundary, but unnecessary for this milestone.

---

### R-006: Temperature Should Vary by Response Mode but Stay Low Overall

**Decision**: Use a bounded dynamic temperature policy:

- `Direct`: `0.2`
- `Cautious`: `0.1`
- `Clarification`: `0.0`
- `Insufficient`: no open-ended legal answer generation; use a deterministic limitation or escalation response

**Rationale**: This feature calls for more controlled generation behavior. In a legal domain, temperature should remain low across all modes, but the assistant should become even less creative when evidence is weak or when it needs to ask for clarification.

**Alternatives considered**:
- **Fixed `0.2` everywhere**: Simpler, but does not reflect the requested adaptive behavior.
- **`0.0` everywhere**: Safest, but tends to read too stiffly for grounded explanatory answers.
- **Higher temperature for clarification**: Rejected because even clarification prompts should remain precise and focused.

---

### R-007: Keep the General-Knowledge Legal Fallback Removed

**Decision**: The ask endpoint must not produce a general-knowledge legal answer when no grounded source is found. Instead it should return either:

- a clarification request,
- a grounded limited response, or
- an escalation-oriented insufficient response

**Rationale**: A disclaimer plus uncited legal guidance is still too risky. This feature exists specifically to improve grounded source discovery and conservative behavior when grounding is weak or the stakes are high.

**Alternatives considered**:
- **Keep the fallback with a stronger disclaimer**: Still too risky and does not satisfy the refined spec.
- **Silent failure or empty response**: Poor user experience and not actionable.

---

### R-008: No New Migration for Retrieval Hardening and Source Labeling

**Decision**: No EF Core migration is needed for the current feature slice.

**Rationale**: The retrieval improvement uses existing source metadata, existing answer persistence, and append-only response DTO changes only. The new law-vs-guidance distinction can be derived from curated document metadata rather than from new persisted columns in this milestone.

**Alternatives considered**:
- **Persist answer mode and confidence to `Answer`**: Useful for analytics, but not required to meet the current scope.
- **Add document-type or authority columns now**: Cleaner schema, but larger scope than needed for the immediate refinement.

---

### R-009: Frontend Change Scope Remains the Existing Ask Flow

**Decision**: Limit frontend work to the current Ask-page chat consumer:

- update response types
- propagate answer-mode, confidence, and source-label fields through chat state
- render localized caution, clarification, escalation, and source-role notices

No new route, no new page flow, and no design-system rewrite are needed.

**Rationale**: The spec does not require a new UI journey. The existing Ask page already renders answer text and citations; it only needs a structured way to reflect safer response modes and the distinction between binding law and official guidance.

**Alternatives considered**:
- **Backend only**: Works technically, but users would not reliably see why a response is cautious or why a source is guidance rather than law.
- **Large Ask-page redesign**: Unnecessary for this refinement milestone.

---

### R-010: Calibrate Retrieval and Safety with a Benchmark Pack Before Declaring the Feature Done

**Decision**: Maintain a lightweight benchmark prompt pack that covers:

- plain-language questions without Act names
- semantically equivalent phrasing variants
- wrong explicit Act hints
- multi-source legal questions
- short ambiguous prompts
- unsupported questions
- guidance-vs-law scenarios
- urgent escalation scenarios

These benchmark cases will be used to tune retrieval weights and answer-mode thresholds before implementation is considered complete.

**Rationale**: The research reports emphasize retrieval quality, groundedness, labeling, and human-reviewable evaluation. A compact benchmark pack gives the team a repeatable way to calibrate the existing system without adding new telemetry tables or production analytics infrastructure.

**Alternatives considered**:
- **Ad-hoc manual testing only**: Too easy to forget edge cases and too hard to repeat after code changes.
- **Production-log-driven tuning only**: Too slow and too risky for a legal assistant safety feature.

---

### R-011: Keep This Milestone Legislation-First and Defer Judgment Weighting Until Case Law Is Ingested

**Decision**: This milestone will keep the retriever focused on the current legislation-centric corpus and existing document metadata. Court-hierarchy and precedential-weight ranking from the research reports are recorded as a follow-on once judgments and court-level metadata are part of the indexed corpus.

**Rationale**: The current feature scope and existing data model are centered on Acts, categories, chunks, and enrichment metadata. Adding authority-weight logic without judgment coverage would create false precision and distract from the more immediate need: better source discovery, safer answer modes, clearer source labeling, and explicit escalation behavior.

**Alternatives considered**:
- **Introduce court weighting now**: Misaligned with the current corpus and likely to increase complexity without improving this milestone's real behavior.
- **Expand the corpus inside this feature without boundaries**: Too much scope for a retrieval-hardening milestone.

---

### R-012: Clarification and Insufficient Responses Are Correct Safety Outcomes, Not UX Failures

**Decision**: Treat `Clarification` and `Insufficient` as first-class, testable success states when the system lacks decisive grounding or the matter is too urgent to answer routinely. Evaluation and UI behavior must reflect that a safe follow-up question or grounded refusal is the correct response in some legal scenarios.

**Rationale**: Legal-assistant quality is a safety problem as much as a retrieval problem. If the system is broad, ambiguous, under-grounded, or high-risk, asking one focused question or declining with escalation language is more correct than forcing a direct answer.

**Alternatives considered**:
- **Optimize mostly for direct answers**: Encourages overconfident legal responses.
- **Treat all non-answer states as failure**: Hides the safety value of clarification and grounded refusal behavior.

---

### R-013: Plan Against the Real Corpus First, Then Stage Expansion Bundles Separately

**Decision**: Split planning into two corpus horizons:

- **Current corpus horizon**: Calibrate retrieval against the legislation and guidance already seeded in `LegislationManifest.cs`.
- **Expansion horizon**: Stage additional public-source bundles from `docs/research_legislation.md` as explicit follow-on work, not hidden assumptions inside retrieval tuning.

Current seeded coverage includes the Constitution, BCEA, LRA, POPIA, RHA, PHA, CPA, NCA, TAA, and selected financial guidance material. High-value gaps identified by the legislation research include PIE, Domestic Violence Act bundles, Small Claims materials, PAJA, PAIA, Criminal Procedure Act, CCMA rules/forms, and maintenance materials.

**Rationale**: Retrieval failures and corpus gaps are different problems. The planning set should make that visible so the team does not misdiagnose missing-source scenarios as ranking regressions.

**Alternatives considered**:
- **Treat all benchmark misses as retrieval defects**: Hides corpus coverage gaps.
- **Expand the corpus immediately inside the same implementation slice**: Adds too much scope and licensing risk.

---

### R-014: Use a Public-Source-First, Licensing-Aware Corpus Policy

**Decision**: For any follow-on corpus expansion, prefer public authoritative sources first:

- `gov.za` for Acts and Gazette PDFs
- `justice.gov.za` for forms and public legal guidance
- Constitutional Court and Supreme Court of Appeal repositories for future judgment bundles
- Information Regulator, SARS, and Gazette-hosted procedural forms for official guidance

Justice-hosted PDFs that include third-party publisher notices are treated as higher licensing risk and should be replaced with Gazette originals or cleared explicitly before bulk ingestion.

**Rationale**: The legislation research shows that "officially hosted" does not always mean low licensing risk. A public-source-first rule reduces licensing uncertainty while preserving citation reliability.

**Alternatives considered**:
- **Ingest all DOJ-hosted PDFs as-is**: Too risky when publisher notices are present.
- **Rely on commercial compilations by default**: Increases cost and licensing complexity for a public-oriented assistant.

---

### R-015: Distinguish Binding Law from Official Guidance Through Curated Source Metadata

**Decision**: Add an append-only source-labeling layer to the ask contract and planning model using curated source metadata:

- `authorityType`: `bindingLaw` or `officialGuidance`
- `sourceRole`: `primary` or `supporting`
- generic display fields for source title and locator

For this milestone, these labels are derived from the current corpus manifest, document title patterns, category knowledge, and curated source-family rules rather than from schema changes.

**Rationale**: The refined spec requires the product to tell users which part of an answer is binding law and which part is merely official guidance. This is a product requirement, not just a prompt instruction.

**Alternatives considered**:
- **Leave the distinction implicit in answer text only**: Too easy for users to miss and too hard to test.
- **Introduce new persisted document metadata first**: Cleaner long-term, but larger scope than needed right now.

---

### R-016: Track Coverage State in Benchmarks and Quickstart

**Decision**: Classify benchmark scenarios using explicit coverage states:

- `InCorpusNow`
- `NeedsCorpusExpansion`
- `GuidanceOnly`
- `Escalate`

The quickstart and verification loop must record which scenarios should pass with the current seeded corpus and which ones are intentionally deferred until a future ingestion bundle is added.

**Rationale**: This prevents the team from overpromising current behavior and gives a clean handoff from retrieval hardening to corpus expansion.

**Alternatives considered**:
- **Single undifferentiated benchmark list**: Makes debugging and scoping less clear.
- **Ignore out-of-corpus scenarios entirely**: Misses the planning value of the legislation research.

---

### R-017: High-Risk Escalation Must Work Even When the Corpus Is Incomplete

**Decision**: High-risk triggers such as arrest, domestic violence, imminent eviction, or near-deadline administrative harm must still route users toward safer clarification or escalation messaging even when the authoritative source bundle is not yet ingested.

**Rationale**: Safety cannot depend entirely on perfect corpus coverage. If a question is urgent, the system should not act as though it is a normal low-stakes search failure.

**Alternatives considered**:
- **Require authoritative in-corpus support before any escalation messaging**: Leaves high-risk users without helpful safety guidance.
- **Use broad legal advice fallback**: Reintroduces the risk the feature is designed to avoid.

---

### R-018: Follow-On Corpus Expansion Should Reuse Existing ETL and Manifest Patterns

**Decision**: If the team expands the corpus after this planning cycle, it should do so through the existing document registration and ingestion path:

- seed and classify new source definitions through `LegislationManifest.cs` and related registrars
- process them through the current ETL pipeline
- track ingestion through `IngestionJob`

Priority follow-on bundles from the legislation research are:

1. Housing and eviction bundle: PIE Act plus related leading public guidance
2. Labour procedure bundle: CCMA rules and Gazette forms
3. Family safety bundle: Domestic Violence Act and DOJ forms
4. Small claims bundle: Small Claims Courts Act and DOJ guidance
5. Administrative access bundle: PAJA citizen materials and PAIA forms
6. Criminal rights bundle: Criminal Procedure Act

**Rationale**: The project already has a manifest-driven document catalog and an ingestion job model. Reusing those patterns respects the constitution and avoids inventing a separate corpus-loading path.

**Alternatives considered**:
- **Manual one-off document loading per bundle**: Harder to audit and maintain.
- **Delay bundle planning until after retrieval tuning**: Loses the value of the legislation research in current planning.

---

## Summary

The planning set now reflects both research inputs. The current milestone remains a retrieval-hardening and response-safety feature on top of the existing multilingual RAG pipeline, but it now explicitly accounts for source authority labeling, guidance-vs-law distinction, urgent escalation behavior, and the difference between current corpus coverage and future public-source bundle expansion. No new schema or infrastructure is required for the current slice, and any later corpus growth should reuse the existing manifest and ETL pipeline rather than bypassing it.
