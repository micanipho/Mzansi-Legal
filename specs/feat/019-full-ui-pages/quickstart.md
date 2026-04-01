# Quickstart: Full UI Pages — MzansiLegal

**Branch**: `feat/019-full-ui-pages`
**Date**: 2026-03-31

---

## Prerequisites

- Node.js 20+ and npm 10+
- Backend running on `http://localhost:21021` (or the configured `NEXT_PUBLIC_API_BASE`)
- PostgreSQL running (docker: `docker exec mzansi-pg psql ...`)

---

## Step 1: Switch to the Feature Branch

```bash
git checkout feat/019-full-ui-pages
```

---

## Step 2: Restore Frontend Files

The frontend directory was removed from the working tree when the branch was created. Restore it from the last committed state:

```bash
git checkout HEAD -- frontend/
```

This restores all `frontend/` files including `package.json`, `tsconfig.json`, and all source files.

---

## Step 3: Install Dependencies

```bash
cd frontend
npm install
```

No new packages are introduced in this feature. If `node_modules` already exists, this is a no-op.

---

## Step 4: Configure Environment

Create or verify `frontend/.env.local`:

```env
NEXT_PUBLIC_API_BASE=http://localhost:21021
```

---

## Step 5: Start the Dev Server

```bash
npm run dev
```

The app will be available at `http://localhost:3000`. Next.js redirects the root to `http://localhost:3000/en` by default.

---

## Page-by-Page Dev Guide

### Home (`/en/`)
- Verify hero tagline renders in Fraunces serif
- Verify organic background blobs appear behind the hero
- Verify four stats cards render with correct numbers
- Verify nine category cards render with correct icons and type badges
- Verify five trending questions render with correct tags
- Click "Get started" → should navigate to `/en/auth`
- Verify language selector switches locale and re-renders all strings

### Auth (`/en/auth`)
- Default tab: Sign In
- Click "Register" tab → form switches to register fields
- Try submitting with empty fields → inline validation errors appear
- Register with valid data → should redirect to `/en/`
- Sign in with admin credentials → should redirect to `/en/admin/dashboard`
- Sign in with user credentials → should redirect to `/en/`

### Ask (`/en/ask`)
- Type a question and press Enter → question appears in thread, AI answer streams in
- Verify citations accordion is hidden when collapsed
- Click "Sources (N sections cited)" → accordion expands, legislation list renders
- Click "Listen in isiZulu" → browser TTS reads the answer aloud
- Click a related question chip → same question submitted as a new message
- Click microphone → browser asks for mic permission (if first time), then listens

### Contracts (`/en/contracts`)
- Unauthenticated → redirected to `/en/auth`
- Authenticated → empty state shown if no contracts uploaded
- Upload button → triggers file picker (PDF only)
- After upload → contract appears in list with "Analysing" status

### Contract Detail (`/en/contracts/[id]`)
- Score circle renders with correct colour (red/amber/green)
- "Plain-language summary" card shows paragraph text
- Red flags section shows each flag with red left border and legislation chip
- Caution section shows each item with amber warning icon
- Standard clauses section shows green check summary
- Inline chat input sends follow-up question in context of this contract
- "← Back to contracts" navigates back to list

### My Rights (`/en/rights`)
- Progress bar shows "X of 20 rights topics" with percentage
- Category tab "All" → all cards shown
- Click "Employment" tab → only Employment cards shown
- Click "+" on a card → card expands showing full explanation, pull-quote, action buttons
- Click "–" again → card collapses
- Click "Ask a follow-up" on an expanded card → navigates to Ask page

### History (`/en/history`)
- Unauthenticated → redirected to `/en/auth`
- Authenticated with no history → empty state with CTA
- With history → conversation list with timestamps and question previews
- Click a conversation → navigates to Ask page with conversation pre-loaded

### Admin Dashboard (`/en/admin/dashboard`)
- Unauthenticated → redirected to `/en/auth`
- Authenticated as regular user → redirected to `/en/`
- Authenticated as admin → summary cards, chart, and recent activity render
- All strings render correctly in all four languages

---

## Language Testing

Switch between all four locales and verify no missing translation key warnings appear in the browser console:

| URL Prefix | Language |
|---|---|
| `/en/` | English |
| `/zu/` | isiZulu |
| `/st/` | Sesotho |
| `/af/` | Afrikaans |

Any `[MISSING]` key output in the UI indicates a gap in one of the language JSON files in `frontend/src/messages/`.

---

## Build Check

Before raising a PR, run the production build to catch type errors and missing imports:

```bash
cd frontend
npm run build
```

Zero TypeScript errors and zero missing translation warnings are required to pass.
