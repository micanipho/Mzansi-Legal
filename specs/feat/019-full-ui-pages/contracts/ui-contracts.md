# Contract: Shared UI Components

**Feature**: feat/019-full-ui-pages
**Date**: 2026-03-31

## Purpose

Defines the public-facing prop contracts for shared components used across multiple pages. These contracts must not break between page implementations.

---

## AppNavbar

**Route**: All pages (rendered in `[locale]/layout.tsx`)

### Behaviour Contract
| Condition | CTA Display | Active Link |
|---|---|---|
| Unauthenticated | "Get started" button (→ `/auth`) | Highlighted by current pathname |
| Authenticated (user) | User avatar/initials + dropdown menu | Same |
| Authenticated (admin) | User avatar/initials + dropdown menu | Dashboard link visible |

### Dropdown Menu Items (authenticated)
- User name + email (non-interactive)
- Sign Out action (clears `ml_token` + `ml_user` cookies → redirect to home)

### Language Selector
- Displays current locale label (e.g., "English")
- Dropdown: English, isiZulu, Sesotho, Afrikaans
- On select: rebuild current URL with new locale segment using `buildLocaleSwitchHref`

---

## SummaryCard (Dashboard)

```
┌─────────────────────────────┐
│  [Icon]                     │
│  2,847                      │
│  Questions answered         │
└─────────────────────────────┘
```

- Value: large bold number
- Label: small muted text below
- Background: `C.card` with `borderRadius: 16px`
- Border: `1px solid C.border`

---

## RightsCard (My Rights)

### Collapsed state
```
┌───────────────────────────────────────┬───┐
│  [bold title]                         │ + │
│  [legislation citation, muted]        │   │
│  [one-line summary]                   │   │
└───────────────────────────────────────┴───┘
```

### Expanded state
```
┌─────────────────────────────────────────────┬───┐
│  [bold title]                               │ – │
│  [legislation citation, muted]              │   │
│                                             │   │
│  [Full explanation paragraph]               │   │
│                                             │   │
│  ┌─────────────────────────────────────┐   │   │
│  │  "[Pull-quote from legislation]"    │   │   │
│  └─────────────────────────────────────┘   │   │
│                                             │   │
│  [Ask a follow-up]  [Listen in isiZulu]     │   │
│                     [Share]                 │   │
└─────────────────────────────────────────────┴───┘
```

- Toggle button uses `aria-expanded` attribute
- Smooth expand with CSS `max-height` transition or Ant Design `Collapse`
- Pull-quote block: left border `4px solid C.accent`, italic, `C.mutedFg` text
- "Ask a follow-up" → navigates to `/ask` with `?q=<title>` search param pre-filled

---

## ContractScoreBadge (Contract Detail)

```
    ┌──────────────────┐
    │       62         │
    │      /100        │
    └──────────────────┘
```

- Circular badge, 120×120px
- Stroke: scored dynamically
  - 0–39: `C.destructive` (red)
  - 40–69: `C.secondary` (amber)
  - 70–100: `C.primary` (green)
- Font: Fraunces serif, large weight

---

## ChatInput (Ask Page)

```
┌─────────────────────────────────────────────┬───┬───┐
│  [placeholder text]                         │ 🎤 │ ➤ │
└─────────────────────────────────────────────┴───┴───┘
```

- Grows to max 4 lines then scrolls
- Submit on Enter (not Shift+Enter)
- Microphone icon activates `VoiceInput` component
- Send button (`➤`) disabled when input is empty
- Both mic and send buttons are keyboard-accessible (tab + Enter/Space)

---

## OrganicBackground

A decorative SVG or CSS gradient overlay applied behind the hero section of the landing page. Creates the subtle circular blob shapes visible in the design references. Implemented as a fixed-position `div` with low `z-index` and `pointer-events: none`.
