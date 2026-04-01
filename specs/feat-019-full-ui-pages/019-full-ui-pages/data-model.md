# Data Model: Full UI Pages — MzansiLegal

**Phase**: 1 — Frontend design
**Branch**: `feat/019-full-ui-pages`
**Date**: 2026-03-31

> This document covers frontend data shapes, component prop types, and API response shapes consumed by each page. No new backend entities or database migrations are introduced.

---

## 1. Authentication

### `AuthUser` (from `AuthProvider`)
```typescript
interface AuthUser {
  id: string;
  name: string;
  email: string;
  isAdmin: boolean;
}
```
**Source**: Decoded from `ml_user` cookie (JSON-encoded).
**Used by**: All pages via `useAuth()` hook.

---

## 2. Ask / Chat Page

### `ChatMessage`
```typescript
interface ChatMessage {
  id: string;
  role: "user" | "assistant";
  content: string;
  citations?: Citation[];
  relatedQuestions?: string[];
  timestamp: Date;
}
```

### `Citation`
```typescript
interface Citation {
  id: string;
  actName: string;       // e.g., "Constitution of the Republic of South Africa"
  section: string;       // e.g., "Section 26(3)"
  excerpt?: string;      // Optional short excerpt from the source
}
```

### `ConversationSession`
```typescript
interface ConversationSession {
  id: string;
  messages: ChatMessage[];
  locale: string;
  startedAt: Date;
}
```

**API Response Shape** (from backend `/api/qa/ask`):
```typescript
interface QaResponse {
  answer: string;
  citations: Array<{
    documentChunkId: string;
    actName: string;
    section: string;
    excerpt?: string;
  }>;
  relatedQuestions?: string[];
  conversationId: string;
  questionId: string;
}
```

---

## 3. Contracts Pages

### `ContractListItem`
```typescript
interface ContractListItem {
  id: string;
  name: string;
  type: "lease" | "employment" | "credit" | "service" | "other";
  uploadedAt: string;       // ISO date string
  status: "analysing" | "complete" | "failed";
  score?: number;           // 0–100, present when status === "complete"
  pageCount?: number;
  language?: string;
}
```

### `ContractAnalysis` (detail page)
```typescript
interface ContractAnalysis {
  id: string;
  name: string;
  type: string;
  uploadedAt: string;
  analysedInSeconds: number;
  pageCount: number;
  clauseCount: number;
  language: string;
  score: number;            // 0–100 risk score (higher = healthier)
  summary: string;          // Plain-language summary paragraph
  redFlags: ContractIssue[];
  cautions: ContractIssue[];
  standardClausesOk: boolean;
  standardClausesNote?: string;
}

interface ContractIssue {
  id: string;
  title: string;
  description: string;
  legislation?: string;     // e.g., "Rental Housing Act, Section 5(3)(g)"
}
```

### Static Mock (contractData.ts)
For this iteration, contract data is served from a static mock file at `frontend/src/components/contracts/contractData.ts`. Real backend integration is a follow-on feature.

---

## 4. My Rights Page

### `RightCard`
```typescript
interface RightCard {
  id: string;
  category: "Employment" | "Housing" | "Consumer" | "Debt & Credit" | "Tax" | "Privacy";
  titleKey: string;         // i18n translation key for title
  legislationKey: string;   // i18n translation key for legislation citation
  summaryKey: string;       // i18n translation key for one-line summary
  bodyKey: string;          // i18n translation key for full explanation
  quoteKey?: string;        // i18n translation key for pull-quote (optional)
  r: string;                // Organic border radius token (R.o1–R.o4)
}
```

### `RightsProgress`
```typescript
interface RightsProgress {
  explored: number;         // Number of cards the user has expanded
  total: number;            // Always 20 for MVP
}
```
**Source**: Stored in `localStorage` keyed by `ml_rights_progress` (JSON array of explored card IDs). Falls back to 0 for unauthenticated users.

---

## 5. History Page

### `HistoryItem`
```typescript
interface HistoryItem {
  conversationId: string;
  firstQuestion: string;    // First question text (truncated to 120 chars)
  questionCount: number;
  startedAt: string;        // ISO date string
  locale: string;
}
```

**API Response Shape** (from backend `/api/conversations`):
```typescript
interface ConversationsListResponse {
  items: Array<{
    id: string;
    firstQuestion: string;
    questionCount: number;
    createdAt: string;
    locale: string;
  }>;
  totalCount: number;
}
```

---

## 6. Admin Dashboard

### `DashboardSummary`
```typescript
interface DashboardSummary {
  totalQuestions: number;
  activeUsers: number;
  contractsAnalysed: number;
  documentsIndexed: number;
}
```

### `InsightDataPoint`
```typescript
interface InsightDataPoint {
  label: string;    // e.g., "Housing", "Employment", "Credit"
  value: number;    // Percentage or count
  tone: "primary" | "secondary" | "danger";
}
```

**Source**: Backend `/api/admin/stats` (or static mock for MVP if endpoint unavailable).

---

## 7. Navigation

### `NavLink`
```typescript
interface NavLink {
  labelKey: string;         // i18n translation key
  path: string;             // Route constant from appRoutes
  adminOnly?: boolean;
}
```

### Supported Locales
```typescript
type SupportedLocale = "en" | "zu" | "st" | "af";
```

---

## 8. Component Prop Contracts

### `SummaryCard` props
```typescript
interface SummaryCardProps {
  icon: React.ReactNode;
  label: string;
  value: string | number;
  tone?: "primary" | "secondary" | "danger" | "default";
}
```

### `RightsCard` rendered props (from RightCard data shape)
```typescript
interface RightsCardProps {
  card: RightCard;
  isExpanded: boolean;
  onToggle: (id: string) => void;
  onAskFollowUp: (title: string) => void;
  locale: string;
}
```

### `CitationList` props
```typescript
interface CitationListProps {
  citations: Citation[];
  defaultExpanded?: boolean;
}
```

---

## 9. i18n Message Namespaces

| Namespace | Page / Component |
|---|---|
| `nav` | AppNavbar |
| `home` | Landing page |
| `ask` | Ask/Chat page |
| `contracts` | Contracts list and detail pages |
| `rights` | My Rights page |
| `history` | History page |
| `auth` | Sign In / Register page |
| `admin` | Admin Dashboard |
| `common` | Shared strings (loading, error, empty states, disclaimer) |
