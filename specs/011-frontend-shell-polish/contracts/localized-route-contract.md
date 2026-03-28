# Contract: Localized Route Behavior

## Purpose

Define the expected user-facing route behavior for the polished frontend shell.

## Canonical Routes

For each supported locale, the shell must expose these canonical routes:

- `/{locale}` -> home
- `/{locale}/ask` -> primary question-and-answer journey
- `/{locale}/contracts` -> contract analysis list
- `/{locale}/contracts/{id}` -> contract analysis detail
- `/{locale}/rights` -> rights explorer
- `/{locale}/admin/dashboard` -> admin dashboard

## Compatibility Route

- `/{locale}/chat` is a compatibility route only.
- It must resolve to the ask journey without presenting itself as the canonical shell destination.

## Behavioral Rules

1. Shell navigation and page-level calls to action must target canonical routes.
2. Direct visits to a compatibility route must land the user in the equivalent ask journey.
3. Locale changes must attempt to preserve the current route family and any safe route parameters.
4. If a route cannot be preserved safely during a locale change, the user must land on the localized home route rather than an invalid page.
5. Contract detail routes must return an intentional state for valid, loading, empty, or unavailable content.

## Acceptance Contract

- No shell navigation link may point to `/chat` as the preferred destination.
- All required canonical routes must be addressable directly by URL.
- A tester must be able to move from home to ask, contracts list, contract detail, rights, and admin dashboard in each supported locale.
