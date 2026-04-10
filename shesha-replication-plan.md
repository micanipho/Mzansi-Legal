# MzansiLegal Shesha Replication Plan

## Purpose

This document is the implementation plan for recreating the current MzansiLegal solution in Shesha.

It is written as a practical delivery guide, not just an architecture note. The goal is to help you:

- understand what is already implemented in the current solution
- decide what should be built as Shesha-configured functionality versus custom code
- replicate the public-facing product in a Shesha-friendly way
- write FluentMigrator migrations in the Shesha style
- sequence the work so you get value early without fighting the framework

## Executive Summary

MzansiLegal should not be ported to Shesha as a pure low-code app.

The best replication strategy is:

- use Shesha for platform concerns, admin/configuration, dynamic CRUD, forms, auth shell, stored files, menus, and configurable pages
- use custom Shesha pages for the citizen-facing AI-heavy experiences
- keep the RAG, contract analysis, ingestion, and curated content projection as custom backend application services

This is important because the current app is not just CRUD. It includes:

- multilingual legal Q&A with RAG and citations
- contract upload and AI analysis
- follow-up Q&A over analysed contracts
- curated public FAQ and rights academy experiences
- user conversation history
- admin and content-management needs

That combination fits Shesha well only if you split the work into:

1. configurable/admin surfaces
2. custom public pages
3. custom backend orchestration services

## What Exists Today

### Frontend features already implemented

The current frontend already has these user-facing areas:

- Home dashboard
- Auth page with sign-in and registration
- Ask page for legal Q&A chat
- Contracts list page
- Contract detail page with follow-up chat
- Rights explorer
- Rights track detail page
- Conversation history page
- Admin dashboard shell

### Features that are backed by real backend services

These are already implemented with working backend services and should be treated as real product scope:

- Q&A ask flow
- conversation history
- contract upload and analysis
- contract follow-up question flow
- public FAQ feed
- rights academy catalog
- rights academy progress

### Features that are incomplete or placeholder

These should not be treated as complete during the Shesha port:

- admin dashboard metrics are currently mock data
- voice support exists in an older chat implementation, but the active Ask page does not currently use it
- auth return URL handling is not fully wired through the sign-in flow
- some frontend service logic is duplicated
- some user-facing text is still hardcoded in English

## Key Shesha Decision

### Do not do a literal frontend migration

The current frontend stack and the Shesha reference stack are different:

- current app: Next 16, React 19, Ant Design 6
- `shesha-reactjs` reference: Next 14, React 18, Ant Design 5

Because of that, this should be treated as a feature replication into a Shesha app, not a direct code transplant.

### Recommended target shape

Use a two-surface Shesha architecture:

- `adminportal`
  - internal/admin users
  - forms designer
  - CRUD/configuration
  - FAQ curation
  - document and category management
  - ingestion monitoring
  - contract review and analytics
- `publicportal` or `mzansi-public`
  - citizen-facing portal
  - public shell and branding
  - custom pages for Ask, Rights, Contracts, and History
  - configurable login/header/footer where useful

## What Shesha Should Own vs What Should Stay Custom

### Best fit for Shesha-configured forms and generic CRUD

These areas are good candidates for Shesha entities, views, and configured forms:

- Categories
- Legal documents registry
- document metadata management
- FAQ curation workflow
- rights academy content management if later moved out of settings
- ingestion job monitoring
- contract analysis review screens
- admin dashboards that mostly display entity-backed aggregates
- login page, header, footer, menus, static content pages

### Should remain custom backend services

These areas contain orchestration logic and should stay custom:

- RAG ask pipeline
- conversation reconstruction logic
- retrieval planning and confidence logic
- language detection and translation support
- contract analysis orchestration
- contract follow-up answering
- PDF ingestion/chunking/embedding pipelines
- analytics aggregation endpoints

### Should be built as custom Shesha pages

These pages are too specialized to force into pure configured forms:

- Home page
- Ask page
- Contract upload page
- Contract detail page with follow-up chat
- Rights explorer
- Rights track page
- History page
- richer admin analytics page

## Recommended Replication Strategy

### Strategy in one sentence

Replicate the domain and backend services first, then expose simple management surfaces through Shesha configuration, and finally rebuild the citizen-facing experiences as custom pages inside the Shesha public portal.

## Current Feature to Shesha Mapping

| Current feature | Shesha approach | Backend approach | Priority |
| --- | --- | --- | --- |
| Home dashboard | Custom public page | custom read models + existing content endpoints | High |
| Sign in / register | Configurable Shesha login or lightly customized login page | standard auth + small extensions | High |
| Ask chat | Custom public page | keep custom RAG service | High |
| Conversation history | Custom public page | keep custom history endpoints | High |
| Rights explorer | Custom public page | keep FAQ and rights academy projection services | High |
| Rights progress | custom page with simple persisted state | keep progress app service | High |
| Contracts upload | Custom public page using Shesha stored files where possible | keep custom contract service | High |
| Contract detail / follow-up | Custom public page | keep custom contract follow-up service | High |
| Categories admin | Configured CRUD | Shesha entity + generated CRUD | Medium |
| Legal document admin | Configured CRUD + upload workflow | entity + custom ingestion hooks | Medium |
| FAQ curation admin | Configured forms over Q&A entities | generated CRUD + small custom actions | Medium |
| Analytics dashboard | Start custom page | custom aggregate endpoint | Medium |

## Target Solution Layout

## Recommended repository layout

```text
backend/
  src/
    MzansiLegal.Domain/
      Domains/
      Migrations/
    MzansiLegal.Application/
      Services/
      Dto/
    MzansiLegal.Web.Host/
      Controllers/
adminportal/
publicportal/
docs/
```

If your Shesha starter already uses a different module naming pattern, keep the starter conventions and only apply the structure logically.

## Delivery Phases

## Phase 0: Foundation and Portal Registration

### Goal

Create the Shesha application shell and register the new public portal correctly.

### Outputs

- Shesha starter solution running
- separate public portal created
- application key registered in `frwk.front_end_apps`
- `ShaApplicationProvider` configured with the same `applicationKey`

### Tasks

1. Create a new public portal from the Shesha starter pattern.
2. Pick an application key, for example:
   - `mzansi-public`
3. Add a Shesha migration that inserts the portal into `frwk.front_end_apps` if it does not already exist.
4. Configure the public portal app provider with the same `applicationKey`.
5. Decide the base layout mode:
   - `horizontalLayout` is the closest fit for the public citizen portal

### Notes

This step is mandatory in Shesha. Without the application key, the portal will not be identified correctly by the framework.

## Phase 1: Domain Modeling and Migrations

### Goal

Recreate the business domain in the Shesha backend using Shesha-compatible entities and FluentMigrator migrations.

### Core business entities to port

- Category
- LegalDocument
- DocumentChunk
- ChunkEmbedding
- Conversation
- Question
- Answer
- AnswerCitation
- ContractAnalysis
- ContractFlag
- IngestionJob

### Recommended entity file structure

Mirror the current backend domain grouping in the Shesha domain project so the port stays mechanical and the supporting enums remain close to the entities that use them.

```text
backend/
  src/
    MzansiLegal.Domain/
      Domains/
        LegalDocuments/
          Category.cs
          LegalDocument.cs
          DocumentChunk.cs
          ChunkEmbedding.cs
          DocumentDomain.cs
          ChunkStrategy.cs
        QA/
          Conversation.cs
          Question.cs
          Answer.cs
          AnswerCitation.cs
          Language.cs
          InputMethod.cs
        ContractAnalysis/
          ContractAnalysis.cs
          ContractFlag.cs
          ContractType.cs
          FlagSeverity.cs
        ETL/
          IngestionJob.cs
          IngestionStatus.cs
      Migrations/
        M20260410120001_RegisterPublicPortal.cs
        M20260410121500_CreateCategories.cs
        M20260410123000_CreateLegalDocuments.cs
        M20260410124500_CreateQaTables.cs
        M20260410130000_CreateContractAnalysisTables.cs
        M20260410131500_CreateIngestionTables.cs
```

Suggested file ownership rules:

- keep one entity or enum per file
- keep domain-specific enums beside their related entities instead of creating a generic shared dumping ground
- keep FluentMigrator classes in `MzansiLegal.Domain/Migrations`
- only move shared cross-module primitives elsewhere when they are genuinely reused beyond one domain area

### Suggested early stance on ownership

- expose CRUD for `Category`, `LegalDocument`, `ContractAnalysis`, and `IngestionJob`
- keep `DocumentChunk`, `ChunkEmbedding`, and `AnswerCitation` internal
- expose `Conversation`, `Question`, and `Answer` only for admin/review if needed

### Important modeling rule

Keep the AI pipeline tables as business tables in your app schema, not in `frwk`.

Use `frwk` only for framework-managed records such as:

- `front_end_apps`
- stored files and other Shesha framework tables

### Suggested schema

Pick one app schema and keep it consistent, for example:

- `mzl`

If your Shesha starter prefers a different application schema, use that instead.

## Phase 2: Authentication and Public Shell

### Goal

Recreate the public shell with Shesha-friendly auth and navigation.

### Build decisions

- use Shesha login form or a custom login page backed by Shesha auth
- keep the public portal header/footer configurable
- use Shesha menus where it helps
- keep locale switching and public navigation in the shell

### Pages to stand up early

- home
- ask
- rights
- contracts
- history
- auth

### Roles

Use at least:

- `Citizen`
- `Admin`

If Shesha starter already has framework roles, map these into your security model rather than fighting it.

## Phase 3: Knowledge Base and Content Management

### Goal

Recreate the admin-side content and ingestion tooling before finishing the public experiences.

### Admin capabilities to build

- category CRUD
- legal document CRUD
- upload and register legislation
- trigger ingestion or monitor ingestion jobs
- review conversations
- mark approved answers as public FAQs
- assign FAQ category

### Recommended Shesha fit

- Category: pure configured CRUD
- LegalDocument: configured CRUD plus custom action buttons for ingestion
- Conversation/Answer review: configured list/detail pages with custom action buttons

### Rights academy content strategy

Start with the current behavior:

- rights academy catalog loaded from app setting JSON

Later, if content editing becomes heavy, migrate the academy catalog into first-class entities.

This keeps phase 1 and phase 2 smaller.

## Phase 4: Ask Page

### Goal

Rebuild the Ask experience as a custom Shesha public page.

### Why custom

The Ask page includes:

- chat thread rendering
- answer mode banners
- confidence display
- citation grouping
- urgent-attention UI
- conversation resumption

That is far beyond what is reasonable to model as a pure configurable form.

### What to reuse conceptually

- Shesha shell
- Shesha auth/session
- Shesha navigation
- Shesha HTTP/client helpers if useful

### What to rebuild custom

- chat provider/state
- ask input
- chat thread UI
- citations UI
- load conversation flow
- send message flow

### Backend dependency

Keep these endpoints custom:

- ask question
- get conversations
- get conversation by id

## Phase 5: History Page

### Goal

Provide a signed-in history page showing previous conversations.

### Build approach

- custom public page
- reuse existing history endpoint shape
- allow click-through back into Ask with `conversationId`

### Nice fit with Shesha

This page can still live inside the Shesha public portal and use framework auth/session, but the actual rendering should remain custom.

## Phase 6: Rights Explorer

### Goal

Recreate the rights explorer and track detail views.

### Keep this architecture

- public FAQs remain projected content
- rights academy remains projected content
- rights progress remains user-specific persisted state

### Why custom page

This feature includes:

- two different content modes
- expandable cards
- local progress tracking
- server sync
- speech synthesis
- sharing
- deep links to lessons

That makes it a custom UI.

### Recommended admin side

The content pipeline behind it should still be Shesha-admin-friendly:

- FAQ approval forms
- category management
- future academy content management

## Phase 7: Contract Analysis

### Goal

Rebuild contract upload, result listing, result detail, and follow-up Q&A.

### Recommended design

- public list page: custom
- upload interaction: custom
- detail page: custom
- admin review pages: configurable where useful

### Important Shesha integration decision

Use Shesha stored file patterns for upload ownership where possible.

That lets you align with the framework instead of building a separate file model just for the portal.

### Good staged approach

#### Stage 1

- keep the current multipart analysis endpoint working
- rebuild only the public page in Shesha

#### Stage 2

- upload file into Shesha stored files first
- send stored file id into contract analysis service
- persist original file references in framework-friendly form

This reduces friction and lets you ship sooner.

## Phase 8: Admin Analytics

### Goal

Replace the current mock admin dashboard with real analytics.

### Current state

The current dashboard is only a shell and uses mock stats.

### Recommended backend approach

Add a custom aggregate endpoint that returns:

- total indexed documents
- total analyses
- active alerts
- questions by category
- language distribution
- review queue counts

### Recommended frontend approach

Build the dashboard as a custom admin page inside Shesha.

Charts and richer cards are easier to own in code than in configured forms.

## Phase 9: Hardening

### Goal

Stabilize the Shesha version before production rollout.

### Tasks

- unify duplicated client/service logic
- fix auth return URL behavior
- restore or consciously defer voice support
- remove hardcoded tenant assumptions if needed
- complete i18n coverage
- add permissions to admin actions
- add auditing and monitoring

## Detailed Build Order

Use this order if you want the least painful path:

1. Create public portal and register application key
2. Port business entities and FluentMigrator migrations
3. Port current custom backend services
4. Stand up auth and public shell
5. Build Ask page
6. Build History page
7. Build Rights explorer
8. Build Contracts pages
9. Build admin CRUD/config pages
10. Build analytics
11. Harden and polish

This order gives you a working citizen portal early while the admin/configuration side catches up.

## Backend Port Plan

## Services to keep custom

- `RagAppService`
- contract analysis orchestration service
- contract follow-up service
- public FAQ projection service
- rights academy projection service
- rights progress service
- ingestion pipeline service
- analytics service

## Services that can become partly generic

- category management
- legal document CRUD
- ingestion job listing
- contract analysis review listing

## Endpoints that should remain custom

- `POST /api/app/qa/ask`
- `GET /api/app/qa/conversations`
- `GET /api/app/qa/conversations/{id}`
- `POST /api/app/contract/analyse`
- `GET /api/app/contract/my`
- `GET /api/app/contract/{id}`
- `POST /api/app/contract/{id}/ask`
- `GET /api/app/question/faqs`
- `GET /api/app/question/academy`
- `GET /api/app/question/academy-progress`
- `PUT /api/app/question/academy-progress`

## Endpoints that can move toward generated CRUD

- category CRUD
- legal document CRUD
- ingestion job CRUD or list/detail
- admin review endpoints where behavior is simple

## FluentMigrator Plan

## Migration categories you will need

### 1. Framework registration migrations

Use `Execute.Sql(...)` for records such as:

- new `front_end_apps` registration
- seeded app-level configuration records if needed

### 2. Business schema migrations

Use `Create.Table(...)`, `Alter.Table(...)`, indexes, and foreign keys for:

- all app domain tables

### 3. Seed or bootstrap data migrations

Use careful insert scripts for:

- initial categories
- rights academy catalog if you decide to seed it through DB instead of settings

## Naming convention

Use timestamp-based migration numbers in the Shesha style:

- `M20260410120001`
- `M20260410121500`

## Example migration sequence

Suggested order:

1. register public portal in `frwk.front_end_apps`
2. create categories and legal documents
3. create chunk and embedding tables
4. create conversation, question, answer, citation tables
5. create contract analysis and contract flag tables
6. create ingestion job tables if needed
7. seed categories
8. seed portal-level config

## Example: public portal registration migration

```csharp
using FluentMigrator;
using Shesha.FluentMigrator;

namespace MzansiLegal.Domain.Migrations
{
    [Migration(20260410120001)]
    public class M20260410120001 : OneWayMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM frwk.front_end_apps
                    WHERE app_key = 'mzansi-public'
                )
                BEGIN
                    INSERT INTO frwk.front_end_apps
                    (
                        id,
                        creation_time,
                        creator_user_id,
                        is_deleted,
                        tenant_id,
                        name,
                        description,
                        app_key
                    )
                    VALUES
                    (
                        NEWID(),
                        GETDATE(),
                        NULL,
                        0,
                        NULL,
                        'Mzansi Public Portal',
                        'Citizen-facing multilingual legal assistant portal',
                        'mzansi-public'
                    )
                END
            ");
        }
    }
}
```

## Example: business table migration style

```csharp
using FluentMigrator;
using Shesha.FluentMigrator;

namespace MzansiLegal.Domain.Migrations
{
    [Migration(20260410121500)]
    public class M20260410121500 : OneWayMigration
    {
        public override void Up()
        {
            Create.Table("categories").InSchema("mzl")
                .WithIdAsGuid("id")
                .WithFullAuditColumns(SnakeCaseDbObjectNames.Instance, true)
                .WithColumn("name").AsString(200).NotNullable()
                .WithColumn("icon").AsString(100).Nullable()
                .WithColumn("domain_lkp").AsInt64().NotNullable()
                .WithColumn("sort_order").AsInt32().NotNullable();

            Create.Index("ix_mzl_categories_name")
                .OnTable("categories").InSchema("mzl")
                .OnColumn("name").Ascending()
                .WithOptions().NonClustered();
        }
    }
}
```

## Porting notes from current EF Core migrations

When converting the current solution, use the EF migrations only as schema references.

Do not copy them literally.

Instead:

- convert table names to your chosen schema and naming convention
- convert audit columns to Shesha helper methods
- convert foreign keys carefully
- decide which enums become lookup ids versus ints
- align file references with Shesha stored file patterns where appropriate

## Table-by-table migration guidance

### Categories

Create as first-class managed entity.

Fields:

- name
- icon
- domain
- sort order
- audit fields

### LegalDocuments

Fields:

- title
- short name
- act number
- year
- file name
- original pdf id
- category id
- is processed
- total chunks
- audit fields

### DocumentChunks

Keep internal.

Fields:

- document id
- chapter title
- section number
- section title
- content
- token count
- sort order
- audit fields

### ChunkEmbeddings

Keep internal.

Store in the way most compatible with your chosen DB provider in the Shesha backend.

Important decision:

- if the Shesha target is SQL Server, revisit how vectors are stored
- do not assume the current PostgreSQL array approach can be copied unchanged

### Conversations, Questions, Answers, AnswerCitations

Keep these as business tables because they are core product data, even if not all are exposed through generic CRUD.

### ContractAnalyses and ContractFlags

Port these directly and decide whether `OriginalFileId` should point to a Shesha stored file record.

That is the preferred long-term design.

## Frontend Replication Plan in Shesha

## Home page

### Build as

Custom page

### Needs

- hero search
- trending questions
- CTAs into Ask and Contracts
- multilingual shell

### Data dependencies

- trending content can start static
- FAQ snippets can later become dynamic

## Auth page

### Build as

Start with Shesha login configuration if possible.

If registration needs special fields or branding, wrap or customize the login page.

### Recommendation

Do not rebuild cookie and token handling manually unless Shesha makes it necessary.
Prefer the framework auth/session flow.

## Ask page

### Build as

Custom page

### Core interactions

- send user question
- render grounded answer
- show citations
- show answer mode
- show confidence
- support resuming a conversation

### Optional after MVP

- restore voice input
- restore voice output

## History page

### Build as

Custom page

### Core interactions

- fetch conversation summaries
- fetch thread preview
- navigate back into Ask

## Rights explorer

### Build as

Custom page

### Core interactions

- academy/FAQ switch
- per-topic filter
- expandable content cards
- progress indicator
- share
- ask follow-up
- speech synthesis

## Contracts list page

### Build as

Custom page

### Core interactions

- upload/drag-drop
- run analysis
- display recent analyses
- navigate to detail

## Contract detail page

### Build as

Custom page

### Core interactions

- health score summary
- red/amber/green flags
- plain language summary
- follow-up questions
- citation display

## Admin pages

### Build mix

- configurable CRUD for data management
- custom pages for advanced analytics and review flows

## Risks and Constraints

## Major risk 1: version mismatch

The Shesha reference frontend is on older framework versions.

Mitigation:

- treat `shesha-reactjs` as pattern reference only
- use the actual Shesha starter for the working portal
- port behavior, not components

## Major risk 2: file storage assumptions

The current solution stores file references in a way that may not line up with Shesha stored files.

Mitigation:

- normalize uploads around Shesha stored files
- only keep raw multipart endpoints temporarily

## Major risk 3: vector storage differences

The current app uses PostgreSQL-specific storage patterns for embeddings.

Mitigation:

- make vector persistence a design checkpoint before porting ingestion
- if needed, keep embedding persistence behind a repository abstraction

## Major risk 4: trying to over-configure custom UX

If you try to force Ask, Rights, or Contracts into pure configured forms, delivery speed will drop.

Mitigation:

- keep those pages custom from day one

## Recommended Definition of Done Per Milestone

## Milestone 1

- public portal registered
- app provider configured
- shell loads

## Milestone 2

- domain entities and migrations applied
- admin CRUD for categories and legal documents works

## Milestone 3

- Ask page works end to end
- history works end to end

## Milestone 4

- rights explorer works end to end
- academy progress persists

## Milestone 5

- contract upload and detail flows work end to end

## Milestone 6

- admin curation and analytics work

## Recommended Immediate Next Steps

1. Stand up the Shesha starter with a dedicated public portal.
2. Register `mzansi-public` in `frwk.front_end_apps`.
3. Convert the current EF Core schema into Shesha FluentMigrator migrations.
4. Port the custom backend services before touching the citizen-facing pages.
5. Build the Ask page first as the proving ground for the Shesha public portal.

## Suggested First Deliverable Backlog

### Sprint 1

- create public portal
- register application key migration
- configure app provider
- create categories and legal documents migrations
- stand up category CRUD

### Sprint 2

- port conversation and contract entities
- port RAG and contract services
- build Ask page

### Sprint 3

- build History page
- build Rights explorer
- implement academy progress sync

### Sprint 4

- build Contracts list/detail pages
- add stored file alignment
- add admin review pages

### Sprint 5

- replace analytics mocks
- harden auth and i18n
- complete polish and test coverage
