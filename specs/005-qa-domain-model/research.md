# Research: Q&A Domain Model (005-qa-domain-model)

**Date**: 2026-03-28
**Branch**: `005-qa-domain-model`

---

## Decision 1: Enum Representation Strategy

**Decision**: Use C# `enum` types (not string columns) for `Language` and `InputMethod`, stored as integers in PostgreSQL.

**Rationale**: The project already uses `DocumentDomain` as a C# enum (see `Category.cs`). Enums are validated at compile time and stored efficiently. ABP/EF Core serializes enums as integers by default, which is consistent with existing patterns in this codebase.

**Alternatives considered**:
- String columns with a check constraint: rejected — more complex to maintain and not the existing project convention.
- Lookup table / RefList entity: rejected for MVP — overkill for a fixed closed list; may evolve to a lookup table if dynamic language additions are needed later.

---

## Decision 2: Folder Location for Q&A Entities

**Decision**: Create `backend/src/backend.Core/Domains/QA/` for all four new entities (Conversation, Question, Answer, AnswerCitation).

**Rationale**: Mirrors the existing `Domains/LegalDocuments/` folder pattern exactly. Keeps domain modules isolated and navigable. Q&A is a distinct bounded context from the RAG ingestion pipeline.

**Alternatives considered**:
- Place under `LegalDocuments/`: rejected — Q&A is functionally separate from document ingestion.
- Flat `Domains/` folder: rejected — not the established convention.

---

## Decision 3: Cross-Aggregate Reference for AnswerCitation → DocumentChunk

**Decision**: Store only `ChunkId` (Guid FK) on `AnswerCitation` with a restricted-delete FK, and include a navigation property to `DocumentChunk`. No cascade — deletion of a chunk must be blocked while citations reference it.

**Rationale**: ABP constitution explicitly permits cross-aggregate references for "traceability requirements such as citation linkage to document chunks." A navigation property without cascade ownership is the correct DDD cross-aggregate pattern. `DeleteBehavior.Restrict` protects data integrity.

**Alternatives considered**:
- No navigation property, ChunkId only: rejected — makes queries verbose without meaningful benefit.
- Cascade delete: rejected — would silently remove legal citations when legislation is de-indexed, violating citation integrity.
- Snapshot/copy of chunk content into AnswerCitation: rejected — duplicates data maintained by the ingestion pipeline; use `Excerpt` field for the relevant text portion instead.

---

## Decision 4: AudioFile Storage Pattern

**Decision**: Represent `AudioFile` as a `string` property (`[MaxLength(500)]`) holding an opaque file reference (path, URL, or object storage key). No binary data in the database.

**Rationale**: Constitution requires `StoredFile` pattern for binary assets. The existing codebase does not yet have a `StoredFile` domain abstraction. For this iteration, a string reference is sufficient and consistent with how the spec documents it. A dedicated `StoredFile` entity can be introduced in a future feature.

**Alternatives considered**:
- `byte[]` column: rejected — binary data in PostgreSQL is inefficient at scale.
- Dedicated `StoredFile` entity with FK: preferred long-term; deferred to avoid scope creep in this data-model feature.

---

## Decision 5: StoredFile Property as Nullable String

**Decision**: `AudioFile` on both `Question` and `Answer` is nullable (`string?`) — text-only interactions must be fully supported.

**Rationale**: Spec acceptance criteria explicitly state voice input is optional. Nullable aligns with the spec's "AudioFile (StoredFile)" being a reference that may not exist for text interactions.

**Alternatives considered**:
- Non-nullable with empty string default: rejected — misleading; a missing file reference should be null.

---

## Decision 6: UserId Type on Conversation

**Decision**: `UserId` is `long` (matching ABP's `User.Id` which is `AbpUserBase` with `long` primary key in ABP Zero).

**Rationale**: ABP Zero's `User` entity extends `AbpUser<Tenant>` which inherits from `FullAuditedEntity<long>` — the PK is `long`, not `Guid`. This is confirmed by looking at the existing project's `User.cs` pattern in ABP Zero scaffolding. The FK on `Conversation` must match.

**Alternatives considered**:
- `Guid` for UserId: rejected — would not match ABP Zero's `long`-keyed User entity, breaking the FK.

---

## Decision 7: Migration Naming Convention

**Decision**: Migration name: `AddQADomainModel`

**Rationale**: Follows the existing project's migration naming convention (PascalCase verb + feature name). Consistent with `AddLegalDocumentDomain` style from feature 004.

---

## Decision 8: Multilingual Gate — Scope for Domain Model

**Decision**: Language and InputMethod enum values support all four required languages at the data layer. No UI-facing output exists in this feature (pure domain/infrastructure work). Multilingual gate is satisfied by the Language enum inclusion.

**Rationale**: This feature is a domain model feature — there are no user-facing string outputs. The Language enum (en, zu, st, af) ensures all four supported languages are first-class values in the data model, enabling multilingual behaviour in higher layers.

---

## Decision 9: Accessibility Gate — Not Applicable

**Decision**: No frontend components are introduced in this feature. Accessibility gate is not applicable.

**Rationale**: This feature is backend-only (domain entities, EF configuration, migration). Accessibility planning applies to future frontend features that consume this model.

---

## Decision 10: Citation Gate — Not Applicable for This Feature

**Decision**: No AI-facing endpoints are introduced in this feature. Citation gate is not applicable.

**Rationale**: This feature establishes the data model. The RAG pipeline endpoint that uses `AnswerCitation` will be a separate feature and must define its citation contract at that point.
