# Contract: Frontend Shell Experience

## Purpose

Define the expected shell behavior shared across the polished frontend experience.

## Language Selector Contract

1. The language selector must be visible in the global shell.
2. The selector must offer English, isiZulu, Sesotho, and Afrikaans.
3. Changing language must update visible shell copy and route prefix.
4. The selected language must persist through subsequent in-app navigation until changed again.

## Visual System Contract

1. Shared brand colors must be defined once and reused across shell pages.
2. The shell must include a visible paper-style background texture treatment.
3. The texture treatment must be subtle enough to preserve readability.
4. Home, ask, contracts list, contract detail, rights, and admin dashboard must all render with the same visual identity.

## Dashboard Contract

1. The admin dashboard must contain summary content suitable for platform storytelling.
2. The admin dashboard must include at least one visual insight region.
3. If live data is unavailable, the dashboard must show an intentional empty state rather than a broken-looking surface.

## Responsive Contract

1. Primary shell navigation must remain usable on mobile-width screens.
2. Required shell pages must avoid horizontal scrolling in their primary content area.
3. Keyboard navigation and visible focus behavior must remain available for major shell controls.
