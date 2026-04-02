# Feature Specification: Intent-Aware Legal Retrieval for RAG Answers

**Feature Branch**: `feat/021-intent-aware-rag`  
**Created**: 2026-04-01  
**Status**: Draft  
**Input**: User description: "Refine the South Africa-grounded legal assistant using the deep research report so ordinary users can ask legal questions in plain language, get primary-source-grounded answers, understand when official guidance is not binding, and receive safer clarification or escalation when certainty or urgency requires it."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ask in everyday language and still reach the right law (Priority: P1)

An ordinary South African user asks a legal question in plain English, based on their real-world problem rather than the title of an Act. The system identifies the supported legal issue, finds the governing source, and answers in plain language with visible citations.

**Why this priority**: This is the main value of the feature. Users usually know their problem before they know the name of the law that applies to it.

**Independent Test**: Submit supported South African legal questions about common issues such as housing, labour, family safety, or small claims without naming any Act and verify the system returns grounded answers that cite the correct primary source on the first pass.

**Acceptance Scenarios**:

1. **Given** a user describes a supported legal problem in everyday language, **When** the system answers, **Then** it returns a plain-language response grounded in the most relevant primary legal source.
2. **Given** a user does not know the title of the governing Act or section, **When** the question is processed, **Then** the system still identifies the correct source family and shows supporting citations.
3. **Given** the answer includes practical next steps, **When** those steps rely on official forms or procedure guidance, **Then** the controlling legal source remains visible alongside any supporting guidance.

---

### User Story 2 - Different phrasings and wrong source hints still converge on the right meaning (Priority: P2)

A user describes the same issue in different ways, uses colloquial language, or even mentions the wrong Act name. The system stays anchored to the meaning of the problem and surfaces the most relevant source rather than overreacting to noisy wording.

**Why this priority**: Real users search with everyday speech, partial facts, and memory errors. The system needs to feel reliable even when the wording is messy.

**Independent Test**: Ask semantically equivalent variants of the same supported legal question, including versions with colloquial terms or an incorrect Act hint, and verify that the same primary source or source set is selected or that the wrong hint is clearly corrected.

**Acceptance Scenarios**:

1. **Given** two questions describe the same legal issue using different wording, **When** both are processed, **Then** the same primary source or source set is selected for both.
2. **Given** a user uses colloquial terms such as "boss," "landlord," or "maintenance money," **When** the system interprets the question, **Then** it maps those expressions to the correct legal issue area.
3. **Given** a user names the wrong Act or section but the facts point elsewhere, **When** the question is answered, **Then** the response surfaces the more relevant source and makes the mismatch clear instead of forcing the wrong hint.

---

### User Story 3 - Users can see what is law, what is official guidance, and why multiple sources were used (Priority: P3)

A user asks a question that requires more than one source to answer safely. The system combines the controlling legal source with any needed official procedure guidance, cites each source clearly, and distinguishes binding law from supporting guidance.

**Why this priority**: Many legal problems require both the governing rule and practical next-step information. Users need the system to assemble that source chain without hiding which part is actually binding law.

**Independent Test**: Ask a supported legal question that requires both a primary legal source and an official guide, form, or related supporting source, then verify the answer cites the relevant source set and labels their roles clearly.

**Acceptance Scenarios**:

1. **Given** a question is governed by more than one relevant source, **When** the system answers, **Then** it includes the combination of sources needed to support the answer.
2. **Given** the answer uses both binding law and official guidance, **When** the response is shown, **Then** it clearly distinguishes the controlling legal source from the supporting procedural source.
3. **Given** a user reviews the response, **When** they inspect a material claim, **Then** they can trace that claim back to the cited source that supports it.

---

### User Story 4 - The system becomes more cautious or escalates when certainty or stakes are high (Priority: P4)

A user asks a broad, ambiguous, unsupported, or urgent legal question. The system adjusts its response posture by asking for clarification, giving a limited answer, or recommending timely human or official help when the matter is too risky to handle as a routine direct answer.

**Why this priority**: A legal assistant should not sound confident when material facts are missing, the support is weak, or the user's situation is time-sensitive or high-stakes.

**Independent Test**: Submit ambiguous, unsupported, and urgent benchmark questions and verify the system asks for clarification, returns a clearly limited response, or includes escalation language instead of presenting a definitive legal conclusion.

**Acceptance Scenarios**:

1. **Given** the system has strong supporting legal material, **When** it answers, **Then** it provides a direct grounded response with citations.
2. **Given** a missing fact would materially affect the safe or correct answer, **When** the question is processed, **Then** the system asks a focused clarification question before making a confident conclusion.
3. **Given** the available support is weak, conflicting, or outside the supported source set, **When** the system responds, **Then** it gives a clearly limited or insufficient response rather than a definitive legal answer.
4. **Given** the question indicates urgency or high stakes such as imminent eviction, arrest, immediate safety risk, or a near deadline, **When** the system responds, **Then** it includes clear language encouraging timely human legal or official assistance instead of treating the matter as routine.

---

### Edge Cases

- What happens when a one-word or very short prompt such as "eviction" or "dismissal" could refer to several different legal situations?
- How does the system behave when a user names the wrong Act or section but the surrounding facts clearly point to a different legal source?
- What happens when official guidance is available but the underlying binding law is missing, narrower, or points to a different conclusion?
- How does the system respond when a question spans two related legal domains, such as housing and constitutional rights, or labour and administrative fairness?
- What happens when the question falls outside the product's supported South African legal source set?
- How does the system respond when a user asks for an urgent tactical answer in a high-stakes situation such as arrest, domestic violence, or a deadline-driven eviction matter?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST accept supported South African legal questions in plain language without requiring users to provide Act names, section numbers, or formal legal terminology.
- **FR-002**: The system MUST determine the likely legal issue and relevant source area from the meaning of the user's question and its material facts, not only from exact keyword matches.
- **FR-003**: The system MUST prioritize primary South African legal sources when a supported question can be answered from binding law.
- **FR-004**: The system MAY use official public guidance, forms, or regulator material to help users understand procedure or next steps, but it MUST not present those sources as binding law.
- **FR-005**: The system MUST clearly distinguish between binding legal sources and supporting official guidance whenever both appear in the same response.
- **FR-006**: The system MUST prefer sources that directly govern the user's issue over sources that merely repeat similar words or broad topic labels.
- **FR-007**: The system MUST recognize common everyday or colloquial phrasing and map it to the appropriate legal issue area.
- **FR-008**: The system MUST produce materially consistent source selection for semantically equivalent questions phrased in different ways.
- **FR-009**: When the user explicitly names a legal source, the system MUST treat that as a strong signal while still considering other clearly relevant sources.
- **FR-010**: If the user names an incorrect or incomplete legal source, the system MUST still surface the more relevant source when the facts point elsewhere.
- **FR-011**: When more than one source is needed, the system MUST combine the relevant source set in the answer rather than forcing the user to ask source-specific follow-up questions.
- **FR-012**: The system MUST present grounded answers in plain language suitable for non-lawyers while preserving the legal source trail.
- **FR-013**: The system MUST include citations for each material legal claim in the answer and clearly indicate which source or sources were used.
- **FR-014**: The system MUST adjust answer behavior based on the strength of support, the presence of ambiguity, the importance of missing facts, and the urgency of the user's situation.
- **FR-015**: The system MUST provide a direct, citation-grounded answer only when support is strong enough to justify a confident response.
- **FR-016**: The system MUST ask a focused clarification question when a missing fact materially affects the safe or correct answer.
- **FR-017**: The system MUST provide a clearly limited or insufficient response when support is weak, conflicting, or outside the supported source set.
- **FR-018**: The system MUST not present low-confidence interpretations, unsupported claims, or uncited general legal guidance as settled legal conclusions.
- **FR-019**: When the question indicates urgency or high stakes, the system MUST include clear guidance to seek timely human legal help or official assistance.
- **FR-020**: The system MUST explain when it cannot safely complete the answer without more detail or without escalation beyond the product.

### Key Entities *(include if feature involves data)*

- **User Question**: A legal question submitted by an ordinary user in natural language, often without formal legal terminology or source names.
- **Supported Legal Issue**: The legal problem the system infers from the user's wording and facts within the product's current South African scope.
- **Primary Legal Source**: The controlling legal source used to support the answer, such as a constitution provision, statute, or other binding authority already supported by the product.
- **Supporting Official Guidance**: A non-binding but authoritative public source such as a form, regulator guide, or official procedure page that helps the user act on the answer.
- **Source Authority Type**: The role assigned to a cited source, such as binding law or official guidance, so the user can understand how much legal weight it carries.
- **Clarification Gap**: A missing fact or ambiguity that prevents the system from answering safely or accurately.
- **Grounding Confidence**: The degree of certainty that the selected legal material directly supports the answer being prepared.
- **Answer Mode**: The response posture chosen for the question, such as direct answer, cautious answer, clarification request, or insufficient response.
- **Risk Trigger**: A fact pattern or wording cue that indicates urgency, high stakes, or the need for human escalation.
- **Citation Set**: The collection of source references shown with the answer so users can trace each material claim back to supporting authority.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On a benchmark set of supported South African legal questions phrased without Act names, at least 85% return a grounded answer that cites the correct primary source on the first attempt.
- **SC-002**: On a benchmark set of semantically equivalent variants and wrong-source-hint prompts, at least 90% converge on the same primary source or source set, or explicitly correct the misleading hint.
- **SC-003**: At least 80% of first-time test users can successfully ask a supported legal question in plain language without being told to name an Act or section.
- **SC-004**: 100% of responses that rely on both binding law and official guidance clearly distinguish the legal source from the supporting procedural source.
- **SC-005**: 100% of ambiguous, unsupported, or high-risk benchmark prompts result in clarification, limited-answer, or escalation behavior rather than a definitive uncited legal conclusion.
- **SC-006**: Users receive either a grounded answer, a clarification request, or a limited response within 10 seconds under normal operating conditions.
- **SC-007**: 100% of material claims in grounded answers remain traceable to the citations returned with the response.

## Assumptions

- The feature remains focused on supported South African legal topics already covered by the product's curated source corpus, with legislation-first coverage as the current baseline.
- Supported user interactions may be submitted in English, isiZulu, Sesotho, or Afrikaans, while retrieval continues to normalize non-English input to English internally.
- The feature improves how legal sources are found, explained, and safety-routed, but it does not replace professional legal representation.
- Human escalation in this milestone means clear referral or recommendation messaging to appropriate legal or official help channels, not full lawyer booking or case management.
- Official forms, regulator guidance, and public procedure pages may be used as supporting sources where they help users act, but they do not replace controlling legal authority.
