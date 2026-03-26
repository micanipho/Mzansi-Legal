# Data Model: MzansiLegal

## Aggregates and Entities

### 1. Category Aggregate
- **Category** (Root)
  - `Id`: Guid (PK)
  - `Name`: string
  - `LocalizedLabels`: jsonb/string (localized for en, zu, st, af)
  - `Icon`: string
  - `Domain`: RefList(Legal | Financial)
  - `SortOrder`: int

### 2. Knowledge Base Aggregate
- **LegalDocument** (Root)
  - `Id`: Guid (PK)
  - `Title`: string
  - `ShortName`: string
  - `ActNumber`: string
  - `Year`: int
  - `FullText`: string
  - `OriginalPdf`: StoredFile (attachment)
  - `CategoryId`: Guid (FK -> Category)
  - `IsProcessed`: bool
  - `TotalChunks`: int
- **DocumentChunk** (PartOf LegalDocument)
  - `Id`: Guid (PK)
  - `LegalDocumentId`: Guid (FK)
  - `ChapterTitle`: string
  - `SectionNumber`: string
  - `SectionTitle`: string
  - `Content`: string
  - `TokenCount`: int
  - `SortOrder`: int
- **ChunkEmbedding** (PartOf DocumentChunk)
  - `Id`: Guid (PK)
  - `DocumentChunkId`: Guid (FK)
  - `Vector`: float[1536]

### 3. Conversation Aggregate
- **Conversation** (Root, PartOf AppUser)
  - `Id`: Guid (PK)
  - `AppUserId`: Guid (FK -> IdentityUser)
  - `Language`: RefList(en | zu | st | af)
  - `InputMethod`: RefList(Text | Voice)
  - `StartedAt`: DateTime
  - `IsPublicFaq`: bool
  - `FaqCategory`: Guid? (nullable FK -> Category)
- **Question** (PartOf Conversation)
  - `Id`: Guid (PK)
  - `ConversationId`: Guid (FK)
  - `OriginalText`: string
  - `TranslatedText`: string
  - `Language`: RefList
  - `InputMethod`: RefList
  - `AudioFile`: StoredFile? (nullable)
- **Answer** (PartOf Question)
  - `Id`: Guid (PK)
  - `QuestionId`: Guid (FK)
  - `Text`: string
  - `Language`: RefList
  - `AudioFile`: StoredFile? (nullable)
  - `IsAccurate`: bool? (nullable)
  - `AdminNotes`: string?
- **AnswerCitation** (PartOf Answer)
  - `Id`: Guid (PK)
  - `AnswerId`: Guid (FK)
  - `ChunkId`: Guid (FK -> DocumentChunk)
  - `SectionNumber`: string
  - `Excerpt`: string
  - `RelevanceScore`: decimal

### 4. Contract Aggregate
- **ContractAnalysis** (Root, PartOf AppUser)
  - `Id`: Guid (PK)
  - `AppUserId`: Guid (FK -> IdentityUser)
  - `OriginalFile`: StoredFile
  - `ExtractedText`: string
  - `ContractType`: RefList(Employment | Lease | Credit | Service)
  - `HealthScore`: int (0-100)
  - `Summary`: string
  - `Language`: RefList
  - `AnalysedAt`: DateTime
- **ContractFlag** (PartOf ContractAnalysis)
  - `Id`: Guid (PK)
  - `ContractAnalysisId`: Guid (FK)
  - `Severity`: RefList(Red | Amber | Green)
  - `Title`: string
  - `Description`: string
  - `ClauseText`: string
  - `LegislationCitation`: string
  - `SortOrder`: int

### 5. AppUser Extension
- **AppUser** (Extends ABP IdentityUser)
  - `PreferredLanguage`: RefList(en | zu | st | af)
  - `DyslexiaMode`: bool
  - `AutoPlayAudio`: bool
  - `Role`: RefList(Citizen | Admin)

## RefLists (Enums)
- **Domain**: `Legal` (1), `Financial` (2)
- **Language**: `en` (1), `zu` (2), `st` (3), `af` (4)
- **InputMethod**: `Text` (1), `Voice` (2)
- **UserRole**: `Citizen` (1), `Admin` (2)
- **ContractType**: `Employment` (1), `Lease` (2), `Credit` (3), `Service` (4)
- **FlagSeverity**: `Red` (1), `Amber` (2), `Green` (3)
