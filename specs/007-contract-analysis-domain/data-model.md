# Data Model: Contract Analysis Domain

**Branch**: `007-contract-analysis-domain` | **Phase**: 1 | **Date**: 2026-03-28

---

## Entities

### ContractAnalysis

**File**: `backend/src/backend.Core/Domains/ContractAnalysis/ContractAnalysis.cs`
**Namespace**: `backend.Domains.ContractAnalysis`
**Base**: `FullAuditedEntity<Guid>`
**Aggregate root**: Yes

| Property | Type | Nullable | Annotation | Notes |
|---|---|---|---|---|
| `UserId` | `long` | No | `[Required]` | FK → ABP `User.Id` (long). Mandatory owner. |
| `OriginalFileId` | `Guid?` | Yes | — | FK → ABP `BinaryObject.Id`. Null until file uploaded. |
| `ExtractedText` | `string` | Yes | — | Plain text extracted from the contract. Null if OCR failed. |
| `ContractType` | `ContractType` | No | `[Required]` | Enum — Employment, Lease, Credit, Service. |
| `HealthScore` | `int` | No | `[Required]`, `[Range(0, 100)]` | Integer health score 0–100 inclusive. |
| `Summary` | `string` | Yes | — | Plain-language AI-generated summary. |
| `Language` | `Language` | No | `[Required]` | Reuses `backend.Domains.QA.Language` enum. |
| `AnalysedAt` | `DateTime` | No | `[Required]` | UTC timestamp of when analysis completed. |
| `Flags` | `ICollection<ContractFlag>` | — | virtual | Navigation — owned flags. Not populated on lightweight queries. |

**Relationships**:
- One `ContractAnalysis` has many `ContractFlag` records (cascade delete).
- `UserId` references ABP Zero `User` (no EF navigation property — the ABP User entity is managed separately).

---

### ContractFlag

**File**: `backend/src/backend.Core/Domains/ContractAnalysis/ContractFlag.cs`
**Namespace**: `backend.Domains.ContractAnalysis`
**Base**: `FullAuditedEntity<Guid>`
**Aggregate root**: No (owned by `ContractAnalysis`)

| Property | Type | Nullable | Annotation | Notes |
|---|---|---|---|---|
| `ContractAnalysisId` | `Guid` | No | `[Required]` | FK → `ContractAnalysis.Id`. Mandatory. |
| `ContractAnalysis` | `ContractAnalysis` | — | `[ForeignKey(nameof(ContractAnalysisId))]`, virtual | Navigation property. |
| `Severity` | `FlagSeverity` | No | `[Required]` | Enum — Red, Amber, Green. |
| `Title` | `string` | No | `[Required]`, `[MaxLength(200)]` | Short display title for the flag. |
| `Description` | `string` | No | `[Required]` | User-readable explanation of the finding. |
| `ClauseText` | `string` | No | `[Required]` | Verbatim clause text extracted from the contract. |
| `LegislationCitation` | `string` | Yes | `[MaxLength(1000)]` | Free-text citation (e.g., "LRA 66 of 1995, s37"). Optional. |
| `SortOrder` | `int` | No | — | Display order. Defaults to 0. |

**Relationships**:
- Many `ContractFlag` records belong to one `ContractAnalysis` (cascade delete on parent deletion).

---

### ContractType (enum)

**File**: `backend/src/backend.Core/Domains/ContractAnalysis/ContractType.cs`
**Namespace**: `backend.Domains.ContractAnalysis`

```
Employment = 0
Lease      = 1
Credit     = 2
Service    = 3
```

---

### FlagSeverity (enum)

**File**: `backend/src/backend.Core/Domains/ContractAnalysis/FlagSeverity.cs`
**Namespace**: `backend.Domains.ContractAnalysis`

```
Red   = 0
Amber = 1
Green = 2
```

---

## Language Enum (reused)

**File**: `backend/src/backend.Core/Domains/QA/Language.cs` *(existing — no changes)*

The `ContractAnalysis.Language` property references `backend.Domains.QA.Language` directly. No new enum is created.

---

## DbContext Changes

**File**: `backend/src/backend.EntityFrameworkCore/EntityFrameworkCore/backendDbContext.cs`

Two new `DbSet<T>` properties added under a `// ── Contract Analysis domain ──` comment block:

```csharp
public DbSet<ContractAnalysis> ContractAnalyses { get; set; }
public DbSet<ContractFlag> ContractFlags { get; set; }
```

New private method `ConfigureContractAnalysisRelationships(ModelBuilder)` registered in `OnModelCreating`:

| Configuration | Detail |
|---|---|
| `ContractFlag → ContractAnalysis` FK | Cascade delete on parent |
| Index on `ContractAnalysis.UserId` | Efficient user-scoped queries |
| Index on `ContractFlag.ContractAnalysisId` | Efficient flag retrieval per analysis |
| Index on `ContractFlag.Severity` | Efficient cross-analysis severity filter |

---

## Migration

**Name**: `AddContractAnalysisDomain`
**Command** (run from repo root):
```bash
cd backend
dotnet ef migrations add AddContractAnalysisDomain --project src/backend.EntityFrameworkCore --startup-project src/backend.Web.Host
```

**Expected tables created**:
- `ContractAnalyses` — 10 columns (+ 6 audit columns from `FullAuditedEntity`)
- `ContractFlags` — 9 columns (+ 6 audit columns from `FullAuditedEntity`)

**Expected indexes created**:
- `IX_ContractAnalyses_UserId`
- `IX_ContractFlags_ContractAnalysisId`
- `IX_ContractFlags_Severity`

---

## Validation Rules Summary

| Entity | Property | Rule | Enforced By |
|---|---|---|---|
| `ContractAnalysis` | `UserId` | Not null | `[Required]` + DB NOT NULL |
| `ContractAnalysis` | `ContractType` | In enum range | `[Required]` + enum type |
| `ContractAnalysis` | `HealthScore` | 0–100 inclusive | `[Range(0,100)]` + DB check constraint |
| `ContractAnalysis` | `Language` | In enum range | `[Required]` + enum type |
| `ContractAnalysis` | `AnalysedAt` | Not null | `[Required]` + DB NOT NULL |
| `ContractFlag` | `ContractAnalysisId` | Not null, valid FK | `[Required]` + DB FK constraint |
| `ContractFlag` | `Severity` | In enum range | `[Required]` + enum type |
| `ContractFlag` | `Title` | Not null, max 200 chars | `[Required]` + `[MaxLength(200)]` |
| `ContractFlag` | `Description` | Not null | `[Required]` |
| `ContractFlag` | `ClauseText` | Not null | `[Required]` |
| `ContractFlag` | `LegislationCitation` | Max 1000 chars | `[MaxLength(1000)]` |
| `ContractFlag` | Delete cascade | Cascade from parent analysis | Fluent API |

---

## State Transitions

No explicit state machine. `ContractAnalysis.HealthScore` and `Flags` are written once by the analysis pipeline and are read-only thereafter (soft-delete via `FullAuditedEntity.IsDeleted` is the only lifecycle transition).
