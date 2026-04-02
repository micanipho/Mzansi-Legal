# Feature Specification: Authority-Aware RAG Refinement

**Feature Branch**: `feat/025-refine-rag-system`  
**Created**: 2026-04-02  
**Status**: Draft  
**Input**: User description: "Refine the MzansiLegal RAG system to improve grounded legal answer quality, authority handling, multilingual reliability, corpus freshness, and fallback behavior based on the current architecture and constraints."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Receive a grounded answer from the right legal authority (Priority: P1)

A person asks a legal or financial question and receives an answer that is grounded in the most relevant South African source material. The answer makes it clear which sources are binding law, which are only supporting guidance, and which source is controlling when more than one source is cited.

**Why this priority**: The product promise depends on users being able to trust that answers come from the right legal authority, not just from text that happens to look similar.

**Independent Test**: Ask benchmark questions across employment, housing, consumer, privacy, debt, tax, and insurance topics and confirm the answer cites the correct primary source or source set with the controlling authority clearly identified.

**Acceptance Scenarios**:

1. **Given** a question is directly answered by a binding law source, **When** the system returns a response, **Then** the answer cites that binding source as the primary authority.
2. **Given** a question requires both binding law and procedural guidance, **When** the system answers, **Then** the response clearly distinguishes which source controls the legal rule and which source only supports next steps or process.
3. **Given** retrieved material includes a source with similar wording but lower legal relevance, **When** the answer is generated, **Then** that source does not displace the more directly relevant governing source.

---

### User Story 2 - Get a safe answer when support is weak, conflicting, or incomplete (Priority: P2)

A user asks a broad, ambiguous, urgent, or weakly supported question. Instead of sounding falsely certain, the system becomes more cautious, asks for clarification when needed, highlights when authority is missing or mixed, and escalates appropriately for high-risk situations.

**Why this priority**: A legal assistant must fail safely. Trust is damaged faster by overconfident wrong answers than by careful, limited answers.

**Independent Test**: Submit ambiguous, unsupported, conflicting, and urgent test questions and verify the response changes posture appropriately instead of always returning a direct answer.

**Acceptance Scenarios**:

1. **Given** the retrieved support is too weak to answer responsibly, **When** the system responds, **Then** it states that support is insufficient and avoids a definitive conclusion.
2. **Given** a question could be answered accurately if one missing fact were provided, **When** the system responds, **Then** it asks a clarifying follow-up instead of guessing.
3. **Given** a question describes an urgent or high-risk situation, **When** the available support is limited or context-dependent, **Then** the system includes a clear escalation cue toward official or legal help.

---

### User Story 3 - Ask in a supported language without losing legal meaning (Priority: P3)

A person asks a question in English, isiZulu, Sesotho, or Afrikaans. The system still identifies the correct legal meaning, finds the same governing authority as the English equivalent, and responds in the user’s language while keeping legal source labels verifiable.

**Why this priority**: Multilingual access is core to the product. Retrieval quality must not materially drop for users who do not ask in English.

**Independent Test**: Ask equivalent benchmark questions in each supported language and compare the returned primary source set, answer posture, and citation clarity against the English baseline.

**Acceptance Scenarios**:

1. **Given** two questions in different supported languages express the same legal issue, **When** both are answered, **Then** they cite the same primary governing source or materially equivalent source set.
2. **Given** a non-English answer is returned, **When** the user reads the response, **Then** the legal explanation is in the requested language while Act names and source locators remain clear and consistent.
3. **Given** a translation loses important legal nuance, **When** the system detects that confidence has dropped, **Then** it shifts to a more cautious answer posture rather than presenting a confident conclusion.

---

### User Story 4 - Keep the legal corpus trustworthy as sources change (Priority: P4)

A product operator updates or refreshes public legal and regulatory source material. The question-answering system uses the refreshed source set, avoids citing superseded material once the update is complete, and preserves traceability about where each answer came from.

**Why this priority**: Even a strong retrieval system becomes unsafe if it answers from stale or untraceable source material.

**Independent Test**: Refresh a source that changes legal wording or guidance, then verify that subsequent answers cite the updated source while outdated material is no longer presented as current.

**Acceptance Scenarios**:

1. **Given** a public legal or regulatory source has been refreshed, **When** the refresh is completed, **Then** subsequent answers use the refreshed source rather than the superseded version.
2. **Given** a source is not approved for use because its provenance or usage rights are unclear, **When** the system selects supporting material, **Then** that source is excluded from user-facing answers.
3. **Given** a user reviews an answer after a source refresh, **When** they inspect the citations, **Then** each citation can still be traced back to a specific source passage and locator.

---

### Edge Cases

- What happens when binding law and official guidance appear to point in different directions for the same user question?
- How does the system behave when no retrieved source is authoritative enough to support a direct legal conclusion?
- What happens when a translated question and its English equivalent retrieve different sources with similar confidence?
- How does the system respond when an urgent scenario is described but the available authority only covers part of the issue?
- What happens when a source refresh changes the controlling section after earlier answers were already stored?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST prioritize sources that directly govern the user’s issue over sources that merely share overlapping terms.
- **FR-002**: The system MUST distinguish between binding law and supporting official guidance in every answer that uses both.
- **FR-003**: The system MUST identify which cited source is controlling when more than one source is presented.
- **FR-004**: The system MUST NOT present guidance-only support as though it were binding law.
- **FR-005**: When authoritative support is weak, incomplete, or absent, the system MUST shift to a limited response posture rather than giving a definitive legal conclusion.
- **FR-006**: When one additional fact would materially change the answer, the system MUST ask a targeted clarification question instead of guessing.
- **FR-007**: When a query indicates urgency or high risk, the system MUST include an appropriate escalation cue if a direct grounded conclusion cannot be given safely.
- **FR-008**: Semantically equivalent questions in the supported languages MUST lead to materially consistent governing-source selection and answer posture.
- **FR-009**: The system MUST answer in the user’s requested or detected language while preserving clear source names and locators for verification.
- **FR-010**: The system MUST expose the role of each citation in the response so users can tell whether a source is primary or supplementary.
- **FR-011**: The system MUST preserve traceability from each answer back to the specific supporting source passage used to justify it.
- **FR-012**: Refreshed or replaced source material MUST become the active basis for future answers once the refresh is approved and completed.
- **FR-013**: Sources without acceptable provenance, freshness status, or usage rights MUST be excluded from user-facing retrieval and answers.
- **FR-014**: Previously stored answers and citations MUST remain auditable after corpus refinement, including when source content has since been refreshed or superseded.
- **FR-015**: The refinement MUST preserve POPIA-compatible handling for stored user interactions and MUST NOT broaden exposure of personal information in answer review or citation surfaces.

### Key Entities *(include if feature involves data)*

- **User Question**: A legal or financial question asked in one of the supported languages, including wording, context, and urgency signals.
- **Retrieved Source**: A candidate law, regulation, guide, or official material selected to support an answer, including its authority role and freshness status.
- **Authority Role**: The classification that explains whether a source is binding, supplementary, procedural, or unsuitable for a controlling legal conclusion.
- **Answer Posture**: The response mode chosen for a question, such as direct answer, cautious answer, clarification request, or insufficient-support response.
- **Source Refresh Record**: The metadata that indicates a source’s provenance, approval state, freshness, and whether it supersedes an earlier version.
- **Citation Trace**: The user-visible and auditable record linking an answer claim to the exact supporting source passage and locator.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On a benchmark set of supported legal and financial questions, at least 90% of answers cite the correct primary governing source or source set.
- **SC-002**: 100% of answers that rely only on supplementary guidance or weak support clearly disclose that limitation and avoid presenting a definitive legal conclusion.
- **SC-003**: On benchmark questions translated across the supported languages, at least 90% of non-English variants return the same primary governing source or materially equivalent source set as the English baseline.
- **SC-004**: 100% of urgent or high-risk test scenarios either provide a safely grounded answer or include a clear clarification or escalation cue when support is not strong enough.
- **SC-005**: After an approved source refresh, new answers stop citing the superseded source in the affected topic area within one completed refresh cycle.
- **SC-006**: 100% of user-facing answer claims remain traceable to a stored citation record with a source role and locator that a reviewer can audit.

## Assumptions

- The existing ask flow, history flow, persistence model, and current supported languages remain in place; this feature refines answer quality rather than replacing those journeys.
- The current public South African legal and regulatory corpus remains the baseline source set for this refinement.
- Only sources with acceptable public availability and usage rights are eligible for inclusion in grounded answers.
- The product continues to provide legal information, not legal representation or formal legal advice.
- Corpus refreshes may continue to be triggered through existing operational workflows; this feature focuses on trust, source selection, and answer behavior rather than introducing a new operator interface from scratch.
- Existing personal-information handling for stored questions and answers remains the compliance baseline and is not reduced by this feature.
