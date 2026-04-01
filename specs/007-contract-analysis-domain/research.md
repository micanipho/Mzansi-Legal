# Research: Contract Analysis Domain Model

**Branch**: `007-contract-analysis-domain` | **Phase**: 0 | **Date**: 2026-03-28

---

## Decision 1: Language Enum — Reuse vs. New

**Decision**: Reuse the existing `backend.Domains.QA.Language` enum from `backend/src/backend.Core/Domains/QA/Language.cs`.

**Rationale**: The enum already defines all four constitutionally required SA languages (English=0, Zulu=1, Sesotho=2, Afrikaans=3). Creating a duplicate enum would violate DRY and would require two maintenance points if a new language were added in future.

**Alternatives considered**:
- Create a new `ContractLanguage` enum — rejected because it is structurally identical and the constitution prohibits unnecessary duplication.

---

## Decision 2: StoredFile Pattern for OriginalFile

**Decision**: Represent `OriginalFile` as a nullable `Guid?` property named `OriginalFileId`, referencing ABP's `BinaryObject` table.

**Rationale**: `LegalDocument.OriginalPdfId` (in `backend.Core/Domains/LegalDocuments/LegalDocument.cs`) uses the exact same pattern (`Guid? OriginalPdfId`). ABP's `IBinaryObjectManager` manages the actual binary; the domain entity stores only the foreign key. This avoids storing binary data in the domain table.

**Alternatives considered**:
- Store a file path string — rejected because ABP BinaryObject is already in use for the same purpose in the adjacent `LegalDocument` entity; consistency matters.
- A dedicated `StoredFile` domain entity — not present in the codebase; over-engineering for MVP.

---

## Decision 3: UserId Type

**Decision**: `UserId` on `ContractAnalysis` is typed as `long`.

**Rationale**: ABP Zero's `User` entity uses `long` as its primary key (`AbpUser<User>`). Every existing entity that references a user (e.g., `Conversation.UserId`) also uses `long`. Using `Guid` would be incorrect.

**Alternatives considered**: `Guid` — rejected; `int` — rejected. ABP Zero is authoritative here.

---

## Decision 4: HealthScore Constraint

**Decision**: Use `[Range(0, 100)]` data annotation on `HealthScore`, typed as `int`.

**Rationale**: The codebase uses data annotations for property-level validation as the standard pattern (BACKEND_STRUCTURE.md). No Fluent API is needed because EF Core will produce a check constraint from `[Range]` on PostgreSQL with Npgsql. This keeps the constraint co-located with the property definition.

**Alternatives considered**:
- Fluent API `HasCheckConstraint` — valid but unnecessary duplication when `[Range]` achieves the same result at the annotation level.
- Domain constructor validation only — rejected; constraint must be enforced at the DB layer per FR-003.

---

## Decision 5: ContractType and FlagSeverity Enums — New Enums

**Decision**: Create two new enums in the `ContractAnalysis` domain folder:
- `ContractType { Employment = 0, Lease = 1, Credit = 2, Service = 3 }`
- `FlagSeverity { Red = 0, Amber = 1, Green = 2 }`

**Rationale**: These are domain-specific enumerations with no equivalents elsewhere in the codebase. They follow the same enum pattern established by `Language` and `InputMethod`. Integer backing values are preferred for database efficiency (same reasoning as the `Language` enum's XML comment).

**Alternatives considered**:
- Store as strings — rejected; enums provide compile-time safety and are consistent with all existing RefList patterns.

---

## Decision 6: LegislationCitation Storage

**Decision**: Store `LegislationCitation` on `ContractFlag` as a plain `string` (free-text), with `[MaxLength(1000)]`.

**Rationale**: There is no `Legislation` table in the system. The spec explicitly states this is a free-text citation (e.g., "Labour Relations Act 66 of 1995, Section 37"). A 1000-char limit is generous enough for compound citations while preventing unbounded storage. The same pattern is used for `Answer.AdminNotes`.

**Alternatives considered**:
- FK to a legislation entity — rejected; no such entity exists and introducing one is out of scope.

---

## Decision 7: Domain Folder Name

**Decision**: Create a new domain folder `backend/src/backend.Core/Domains/ContractAnalysis/`.

**Rationale**: Follows the existing pattern of domain-named folders (`LegalDocuments/`, `QA/`). "ContractAnalysis" is the aggregate root and the most natural name for the business area.

**Alternatives considered**:
- `Contracts/` — ambiguous (could mean API contracts or legal contracts); avoided.
- `ContractReview/` — equally valid but less precise than the entity name.

---

## Decision 8: Cascade Delete Strategy

**Decision**: Configure `ContractFlag → ContractAnalysis` cascade delete via Fluent API in `backendDbContext.OnModelCreating`, using a private `ConfigureContractAnalysisRelationships` method.

**Rationale**: This matches the established pattern in `backendDbContext` (see `ConfigureQuestionRelationships`, `ConfigureAnswerRelationships`). Data annotations cannot express cascade behavior, so Fluent API is the correct choice. The same method will also add a composite index on `ContractFlag.ContractAnalysisId` and a `Severity` index for the cross-contract query use case.

---

## Decision 9: Indexes

**Decision**: Add the following indexes via Fluent API:
1. `ContractAnalysis.UserId` — for efficient retrieval of all analyses for a given user.
2. `ContractFlag.ContractAnalysisId` — for efficient retrieval of all flags for a given analysis (cascade query, lazy load).
3. `ContractFlag.Severity` — for the cross-analysis severity filter use case (FR-010, SC-005).

**Rationale**: RULES.md mandates indexes for frequently queried fields. All three fields are expected to be filter/join targets in primary application queries.

---

## Decision 10: Application Service Scope

**Decision**: This feature covers **domain model and migration only**. Application services (`ContractAnalysisAppService`, `ContractFlagAppService`) and DTOs are **out of scope** for this feature branch; they will be scaffolded using the `add-endpoint` skill in a subsequent feature.

**Rationale**: The spec's acceptance criteria are: "Tables created, ContractAnalysis correctly linked to AppUser." No API endpoints are required to meet those criteria. Keeping the PR focused on the data layer reduces review surface.
