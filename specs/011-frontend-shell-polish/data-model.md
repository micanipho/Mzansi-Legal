# Data Model: Frontend Shell Polish

## Overview

This feature does not add backend persistence. Its primary "entities" are frontend presentation models and shell state objects that define route ownership, localized navigation behavior, and shared visual tokens.

## Entities

### LocalizedShellRoute

**Purpose**: Represents one canonical destination in the localized frontend shell.

**Fields**:
- `locale`: supported locale identifier (`en`, `zu`, `st`, `af`)
- `path`: canonical localized path segment
- `routeType`: page family identifier (`home`, `ask`, `contracts-list`, `contract-detail`, `rights`, `admin-dashboard`, `legacy-ask`)
- `labelKey`: translation key used in shell navigation or page headers
- `isCanonical`: whether the route is the preferred user-facing destination
- `redirectTarget`: canonical destination for compatibility-only paths

**Validation Rules**:
- `locale` must be one of the four supported MVP languages
- canonical routes must have a unique `routeType` per locale
- compatibility routes must declare a `redirectTarget`

**Relationships**:
- maps to one `LanguageSelectionState`
- may reference one `ContractDetailViewState` when `routeType` is `contract-detail`

### LanguageSelectionState

**Purpose**: Tracks the user’s currently active shell language and how the router should resolve the equivalent destination.

**Fields**:
- `currentLocale`: active locale
- `availableLocales`: ordered list of switchable locales
- `currentPath`: current localized shell path
- `preservedQuery`: query state to retain across locale changes when safe
- `switchSource`: where the locale change originated (`navbar`, `direct-link`, `fallback`)

**Validation Rules**:
- `currentLocale` must always be present
- `currentPath` must resolve to a known or derivable localized shell route
- `preservedQuery` may be empty but must never create an invalid route

**State Transitions**:
- `idle -> switching` when a user chooses a new locale
- `switching -> resolved` when the equivalent route is found and navigation succeeds
- `switching -> fallback-home` when the current journey cannot be mapped safely

### ContractDetailViewState

**Purpose**: Represents the content shown for one contract analysis detail page.

**Fields**:
- `contractId`: route identifier
- `title`: contract name shown in the header
- `status`: availability state (`ready`, `loading`, `empty`, `not-found`)
- `summary`: plain-language overview
- `score`: overall contract score
- `riskSections`: grouped issue sections
- `metadata`: supporting facts such as upload date, type, and language
- `followUpActions`: available next actions from the detail page

**Validation Rules**:
- `contractId` must be present for the detail route
- `status=ready` requires `title` and `summary`
- `score` must be bounded to the contract scoring scale used by the product

**State Transitions**:
- `loading -> ready`
- `loading -> empty`
- `loading -> not-found`

### AdminDashboardViewState

**Purpose**: Represents the admin dashboard shell content and its readiness for presentation.

**Fields**:
- `status`: dashboard availability (`ready`, `empty`, `loading`)
- `summaryCards`: high-level metric cards
- `visualInsightPanel`: chart-ready insight region
- `activityPanels`: optional supporting sections for queue, trends, or highlights
- `emptyStateMessage`: explanatory fallback text

**Validation Rules**:
- at least one `summaryCard` or `visualInsightPanel` must be present
- `status=empty` requires `emptyStateMessage`
- `visualInsightPanel` must support a title and bounded dataset

**State Transitions**:
- `loading -> ready`
- `loading -> empty`

### VisualTokenSet

**Purpose**: Defines the shared shell styling values used across pages.

**Fields**:
- `colorTokens`: background, text, primary, secondary, border, accent, status, and surface values
- `typographyTokens`: serif and sans roles
- `surfaceTokens`: radii, shadows, translucency, card surface values
- `textureTokens`: paper-grain opacity, blend behavior, and layering rules

**Validation Rules**:
- all required shell pages must consume the same token names
- color tokens must provide readable contrast for primary text and action states
- texture tokens must not reduce legibility of body copy or controls

**Relationships**:
- applied by `LocalizedShellRoute` layouts
- used by `AdminDashboardViewState`, `ContractDetailViewState`, and shared shell components
