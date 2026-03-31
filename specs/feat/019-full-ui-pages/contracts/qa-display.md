# Contract: Q&A Citation Display (Ask Page)

**Feature**: feat/019-full-ui-pages
**Date**: 2026-03-31

## Purpose

Defines how the Ask page renders AI-generated answers with RAG citations. This is the display-side contract — the backend RAG contract is defined in feat/014.

---

## RAG Response → UI Mapping

| Backend Field | UI Element | Display Rules |
|---|---|---|
| `answer` | Chat bubble body | Rendered as plain text with line-break support |
| `citations[].actName` | Citation label (bold) | Displayed as-is |
| `citations[].section` | Citation label (secondary) | Displayed as "Section X" |
| `citations[].excerpt` | Citation excerpt (italic) | Shown only when non-empty |
| `relatedQuestions[]` | Related question chips | Max 3 shown; clicking submits as new message |
| `conversationId` | Session state | Stored in component state for follow-up questions |

---

## Citation Display Format

```
Sources (N sections cited)     [collapse/expand toggle]
┌────────────────────────────────────────────────────┐
│ 📄  Constitution of the Republic of South Africa   │
│     Section 26(3)                                   │
│     "No one may be evicted from their home..."     │
├────────────────────────────────────────────────────┤
│ 📄  Prevention of Illegal Eviction Act             │
│     Section 4                                       │
└────────────────────────────────────────────────────┘
```

- The accordion is collapsed by default
- The count `N` in the toggle label reflects `citations.length`
- Empty `citations` array → entire Sources section is hidden

---

## Disclaimer Banner

Must appear below every AI answer:

> "MzansiLegal provides legal information, not legal advice. If you need professional legal assistance, contact Legal Aid SA: **0800 110 110**."

This text must be translated into all four supported languages. The phone number is displayed as-is across all locales.

---

## Fallback Behavior

| Scenario | UI Behavior |
|---|---|
| Backend error / timeout | Show error message: "Something went wrong — please try again" |
| Empty citations array | Answer displays normally; Sources section hidden |
| Stream cut mid-response | Display partial answer + "Response incomplete" warning |
| 0 related questions | Related questions section is hidden (no empty state needed) |

---

## Voice Output Contract

- "Listen in [language]" button is shown on every assistant message
- Locale is resolved from the current `next-intl` locale context
- Uses `window.speechSynthesis` with `lang` mapped as:
  - `en` → `en-ZA`
  - `zu` → `zu-ZA`
  - `st` → `st-ZA` (fallback: `en-ZA`)
  - `af` → `af-ZA`
- If `speechSynthesis` is unsupported, the button is hidden (no error shown)
