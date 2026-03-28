# Feature Specification: Contract Analysis Domain Model

**Feature Branch**: `007-contract-analysis-domain`
**Created**: 2026-03-28
**Status**: Draft
**Input**: User description: "We need entities to store uploaded contracts, their analysis results (health score, summary), and the individual red flag/caution/standard findings."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Store Contract Analysis Result (Priority: P1)

A user uploads a contract for analysis. The system stores the uploaded file, the extracted text, the overall health score, a plain-language summary, and the contract type and language — all linked to the authenticated user's account.

**Why this priority**: This is the foundational record. Nothing else can be persisted without a `ContractAnalysis` entry. All other data hangs off this entity.

**Independent Test**: Can be fully tested by creating a `ContractAnalysis` record with all required fields and verifying it is retrievable and correctly linked to the owning user.

**Acceptance Scenarios**:

1. **Given** an authenticated user has submitted a contract, **When** the analysis completes, **Then** a `ContractAnalysis` record is created containing the user's ID, the original file reference, extracted text, contract type, health score (0–100), summary, detected language, and the timestamp of analysis.
2. **Given** a `ContractAnalysis` record exists, **When** the owning user's account is looked up, **Then** the analysis appears in the user's list of analyses (PartOf relationship).
3. **Given** no `UserId` is provided, **When** a `ContractAnalysis` creation is attempted, **Then** the operation fails with a validation error (UserId is mandatory).

---

### User Story 2 - Store Individual Contract Flags (Priority: P2)

After a contract analysis is complete, the system stores each individual finding (red flag, caution, or standard clause note) as a `ContractFlag` linked to the parent `ContractAnalysis`. Each flag includes its severity, title, description, the relevant clause text, and any applicable legislation citation.

**Why this priority**: Flags are the primary value delivered to users — without them, the health score and summary have no supporting detail.

**Independent Test**: Can be fully tested by creating `ContractFlag` records linked to a `ContractAnalysis` and verifying they are retrievable ordered by `SortOrder`.

**Acceptance Scenarios**:

1. **Given** a `ContractAnalysis` record exists, **When** one or more `ContractFlag` records are created for it, **Then** each flag is persisted with the correct `ContractAnalysisId`, severity (Red/Amber/Green), title, description, clause text, legislation citation (if applicable), and sort order.
2. **Given** multiple flags exist across different analyses, **When** a query is made for all red flags across all contracts, **Then** only flags with `Severity = Red` are returned, regardless of which analysis they belong to.
3. **Given** a `ContractAnalysis` is deleted, **When** the database is queried, **Then** all associated `ContractFlag` records are also removed (cascade delete).

---

### User Story 3 - Query Flags by Severity Across All Contracts (Priority: P3)

An administrator or power user needs to retrieve all flags of a specific severity (e.g., all "Red" flags) across every contract analysis in the system, to identify systemic risks or common problematic clauses.

**Why this priority**: This is a cross-cutting query capability enabled by the separate entity design (as opposed to storing flags as JSON). It is not needed for basic operation but is the key justification for the chosen architecture.

**Independent Test**: Can be fully tested by creating flags across multiple `ContractAnalysis` records and verifying that a severity-filtered query returns the correct subset.

**Acceptance Scenarios**:

1. **Given** flags of mixed severities exist across multiple contracts, **When** a query filters by `Severity = Red`, **Then** only Red flags are returned with their parent analysis context.
2. **Given** no flags of a requested severity exist, **When** the query runs, **Then** an empty result set is returned without error.

---

### Edge Cases

- What happens when `HealthScore` is set outside the 0–100 range? The system must reject values below 0 or above 100 with a validation error.
- What happens when `ContractType` or `Language` is set to a value not in the defined reference list? The system must reject the value with a validation error.
- What happens when `LegislationCitation` is omitted on a `ContractFlag`? It is optional — the record must still be accepted without it.
- What happens when `SortOrder` is not provided for a `ContractFlag`? A default of 0 is applied.
- What happens when `ExtractedText` is empty (e.g., a scanned image contract where OCR failed)? The record may be created with a null/empty `ExtractedText`; upstream processing handles this case.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST persist a `ContractAnalysis` record containing: owning user reference (mandatory), original file reference, extracted text, contract type, health score (integer 0–100 inclusive), plain-language summary, language, and the date/time of analysis.
- **FR-002**: System MUST enforce that `UserId` on `ContractAnalysis` is non-null and references a valid `AppUser`.
- **FR-003**: System MUST enforce that `HealthScore` is an integer constrained to the range 0–100 inclusive.
- **FR-004**: System MUST persist a `ContractFlag` record containing: parent analysis reference (mandatory), severity, title, description, clause text, legislation citation (optional), and sort order.
- **FR-005**: System MUST enforce that `ContractAnalysisId` on `ContractFlag` is non-null and references a valid `ContractAnalysis`.
- **FR-006**: System MUST provide a `ContractType` reference list with values: Employment, Lease, Credit, Service.
- **FR-007**: System MUST provide a `FlagSeverity` reference list with values: Red, Amber, Green.
- **FR-008**: System MUST provide a `Language` reference list covering at least English, Zulu, Xhosa, and Afrikaans.
- **FR-009**: System MUST cascade-delete all `ContractFlag` records when their parent `ContractAnalysis` is deleted.
- **FR-010**: System MUST allow querying `ContractFlag` records filtered by `Severity` across all `ContractAnalysis` records.
- **FR-011**: System MUST apply a database migration that creates the `ContractAnalysis` and `ContractFlag` tables with all constraints defined above.

### Key Entities *(include if feature involves data)*

- **ContractAnalysis**: Represents a single contract review event for a user. Holds the uploaded file reference, extracted text, classified contract type (from reference list), an integer health score (0–100), a plain-language summary, the detected/selected language (from reference list), and the timestamp when analysis completed. Belongs to an `AppUser`. One analysis has many `ContractFlag` records.
- **ContractFlag**: Represents a single finding within a `ContractAnalysis`. Classified by severity (Red/Amber/Green). Contains a short title, a user-readable description, the verbatim clause text from the contract, an optional citation to applicable legislation, and a sort order for display. Belongs to a `ContractAnalysis`.
- **ContractType** *(reference list)*: Enumeration — Employment, Lease, Credit, Service.
- **FlagSeverity** *(reference list)*: Enumeration — Red, Amber, Green.
- **Language** *(reference list)*: Enumeration covering supported application languages — English, Zulu, Xhosa, Afrikaans.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The `ContractAnalysis` and `ContractFlag` tables exist in the database with all columns, constraints, and foreign keys defined in this specification, as verified by an applied EF migration.
- **SC-002**: A `ContractAnalysis` record cannot be created without a valid user reference — 100% of attempts without a `UserId` are rejected.
- **SC-003**: A `ContractAnalysis` record cannot be created with a `HealthScore` outside 0–100 — 100% of out-of-range values are rejected.
- **SC-004**: Deleting a `ContractAnalysis` removes all of its associated `ContractFlag` records — zero orphaned flag records remain after cascade delete.
- **SC-005**: A query filtering `ContractFlag` records by `Severity` returns only matching records across all analyses — 100% accuracy with no false positives or false negatives.
- **SC-006**: All reference list values (ContractType × 4, FlagSeverity × 3, Language × 4) are present and enforced at the data layer.

## Assumptions

- The `AppUser` entity (extending ABP Identity User) already exists in the system as established in feature branch `006-appuser-extension`.
- `OriginalFile` on `ContractAnalysis` is a reference to a stored file record (e.g., a file ID or path stored as a string/GUID). The file storage mechanism is handled outside this feature.
- `Language` will reuse or extend an existing reference list if one already exists in the codebase; otherwise a new enum is created specifically for this feature.
- `LegislationCitation` on `ContractFlag` is a free-text field (not a FK to a legislation table) — legislation references are stored as descriptive strings.
- `SortOrder` defaults to 0 if not explicitly provided by the caller.
- `ExtractedText` may be null/empty for contracts where text extraction has not yet occurred or failed.
- The EF migration will target the existing PostgreSQL database configured in the ABP backend project.
- Admin review of flags across all contracts is an anticipated use case (justifying separate entity over JSON array), but no admin UI is in scope for this feature.
- Frontend display of these entities is out of scope — this feature covers only the domain model and database layer.
