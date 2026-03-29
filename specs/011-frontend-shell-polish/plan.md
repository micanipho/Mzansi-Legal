# Implementation Plan: Frontend Shell Polish

**Branch**: `[011-frontend-shell-polish]` | **Date**: 2026-03-28 | **Spec**: [spec.md](C:/Users/Nhlakanipho/Documents/Projects/Mzansi-legal/specs/011-frontend-shell-polish/spec.md)
**Input**: Feature specification from `/specs/011-frontend-shell-polish/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Polish the existing Next.js frontend shell so the demo story matches the product promise: expose the ask flow on `/ask`, add a contract detail route and an admin dashboard route, make locale switching work from any supported page, and unify the interface through shared CSS-variable design tokens and a paper-style background system. The plan keeps the existing localized app structure, adds a canonical route contract with legacy `/chat` compatibility, and introduces one dashboard visualization area to strengthen platform storytelling.

## Technical Context

**Language/Version**: TypeScript 5, React 19, Next.js 16 App Router  
**Primary Dependencies**: Next.js, next-intl, Ant Design, @ant-design/icons, @ant-design/nextjs-registry, lucide-react, @ant-design/charts  
**Storage**: N/A for this feature; UI-only state with existing message JSON files  
**Testing**: ESLint, Next.js production build, manual localized route smoke checks, manual responsive verification  
**Target Platform**: Modern desktop and mobile browsers  
**Project Type**: Web application  
**Performance Goals**: All required localized routes render without broken navigation; locale switching preserves journey context; dashboard visual area loads without visibly blocking page interaction  
**Constraints**: Preserve multilingual-first behavior for EN/ZU/ST/AF, keep accessibility semantics intact, avoid introducing route ambiguity between canonical and legacy ask paths, keep dashboard shell useful even without live backend metrics  
**Scale/Scope**: Six primary localized page families (home, ask, contracts list, contract detail, rights, admin dashboard), four locales, one legacy route compatibility path, one shared shell and visual-token refresh

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify all gates from the constitution (`/.specify/memory/constitution.md`):

- [x] **Layer Gate**: Frontend-only feature; no new backend/domain entities introduced, so DDD layer boundaries remain intact
- [x] **Naming Gate**: Planned routes, pages, and UI contracts follow existing Next.js file-based naming conventions and localized shell terminology
- [x] **Coding Standards Gate**: Plan keeps logic in small composable frontend modules, avoids new backend rule violations, and retains TypeScript-first structure
- [x] **Skill Gate**: `speckit.plan` used for planning; implementation is expected to use relevant frontend-oriented skills where available during execution
- [x] **Multilingual Gate**: All shell labels, navigation states, and locale switching behavior are planned for English, isiZulu, Sesotho, and Afrikaans
- [x] **Citation Gate**: No new AI-facing endpoint or citation-generating behavior is introduced in this feature; existing cited experiences remain untouched
- [x] **Accessibility Gate**: Keyboard-accessible navigation, language switching, responsive layouts, and readable textured backgrounds are planned explicitly
- [x] **ETL/Ingestion Gate**: Not applicable; this feature does not add or modify document ingestion

**Post-Design Re-check**: PASS. Research, data model, contracts, and quickstart artifacts keep the feature within multilingual, accessibility, and frontend delivery constraints without introducing new governance violations.

## Project Structure

### Documentation (this feature)

```text
specs/011-frontend-shell-polish/
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   |-- frontend-shell-contract.md
|   `-- localized-route-contract.md
`-- tasks.md
```

### Source Code (repository root)

```text
backend/
|-- src/
`-- tests/

frontend/
|-- public/
|-- src/
|   |-- app/
|   |   |-- [locale]/
|   |   |   |-- page.tsx
|   |   |   |-- ask/
|   |   |   |-- contracts/
|   |   |   |   |-- page.tsx
|   |   |   |   `-- [id]/
|   |   |   |-- rights/
|   |   |   `-- admin/
|   |   |       `-- dashboard/
|   |   |-- layout.tsx
|   |   `-- page.tsx
|   |-- components/
|   |   |-- layout/
|   |   |-- dashboard/
|   |   `-- providers/
|   |-- i18n/
|   |-- messages/
|   |-- services/
|   `-- styles/
|-- package.json
`-- next.config.ts
```

**Structure Decision**: Use the existing web-application split and keep all work inside the current `frontend/` app-router project. Route work will happen under `frontend/src/app/[locale]`, shell components under `frontend/src/components/layout` and `frontend/src/components/dashboard`, message updates under `frontend/src/messages`, and shared visual tokens under `frontend/src/styles`.

## Complexity Tracking

No constitution violations or justified complexity exceptions are required for this feature.
