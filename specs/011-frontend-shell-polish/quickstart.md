# Quickstart: Frontend Shell Polish

## Prerequisites

1. Use branch `011-frontend-shell-polish`
2. Node modules are installed in `frontend/`
3. Existing locale messages remain available for `en`, `zu`, `st`, and `af`

## Setup

1. Open the frontend workspace:
   ```bash
   cd frontend
   ```
2. Install the dashboard chart dependency planned for this feature:
   ```bash
   npm install @ant-design/charts
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```

## Manual Verification Flow

### Route verification

1. Open `http://localhost:3000`
2. Confirm the app resolves to the default localized route
3. Visit each canonical route directly:
   - `/en`
   - `/en/ask`
   - `/en/contracts`
   - `/en/contracts/maple-street-lease`
   - `/en/rights`
   - `/en/admin/dashboard`
4. Visit `/en/chat` and confirm it resolves into the ask journey rather than behaving as a separate primary route

### Locale-switch verification

1. Open `/en/ask`
2. Use the language selector to switch to isiZulu, Sesotho, and Afrikaans
3. Confirm the route stays within the ask journey while the locale prefix changes
4. Repeat the same-page language switch check on:
   - `/en/contracts/maple-street-lease`
   - `/en/rights`
   - `/en/admin/dashboard`

### Visual system verification

1. Confirm the shared shell uses the same palette, typography, and paper-like texture across:
   - home
   - ask
   - contracts list
   - contract detail
   - rights
   - admin dashboard
2. Resize to a mobile-width viewport and confirm:
   - no horizontal scrolling in the main content area
   - the navigation remains reachable
   - the language selector remains usable

### Dashboard verification

1. Open `/en/admin/dashboard`
2. Confirm the page includes:
   - summary content
   - one visual insight area
   - an intentional empty/fallback state if live metrics are unavailable

## Validation Commands

Run these before implementation sign-off:

```bash
npm run lint
npm run build
```
