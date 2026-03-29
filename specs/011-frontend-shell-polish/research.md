# Research: Frontend Shell Polish

## Decision: Make `/ask` the canonical route and preserve `/chat` as a compatibility path

**Rationale**: The product language in the spec and demo narrative uses "ask", so the shell should reflect that consistently. Keeping a compatibility path avoids breaking bookmarks, links, or current in-app references while allowing the visible navigation and primary calls to action to move to the new route.

**Alternatives considered**:
- Rename the route and remove `/chat` immediately: rejected because it creates avoidable breakage for existing links and internal references.
- Keep `/chat` and only relabel the nav item as "Ask": rejected because it leaves the product feeling inconsistent in the URL layer.

## Decision: Locale switching should preserve the current user journey, including path parameters and query string when possible

**Rationale**: The spec's strongest multilingual promise is continuity. A user who changes language on a rights page or contract detail page should not be dumped back to home. Preserving the journey makes the locale switcher feel like a first-class product feature rather than a route reset.

**Alternatives considered**:
- Always redirect to localized home after a language change: rejected because it breaks task continuity and weakens the multilingual value proposition.
- Persist language only for future navigation and keep the current page unchanged: rejected because it creates confusing mixed-language shell behavior.

## Decision: Add a contract detail page as a shell-first route backed by representative contract analysis data

**Rationale**: The frontend milestone needs a real `/contracts/[id]` journey now, while deeper backend persistence can continue independently. A shell-first approach allows the contracts list and detail experience to be demonstrated coherently without blocking on domain completion.

**Alternatives considered**:
- Wait for backend persistence before adding the route: rejected because it delays the full demo journey for no product-shell reason.
- Reuse the current contracts list page as an overloaded detail surface: rejected because it hides route depth and weakens the notion of a saved analysis artifact.

## Decision: Move shared brand colors into CSS variables and keep component tokens derived from those variables

**Rationale**: CSS variables are the cleanest way to make the visual system consistent across plain CSS, Ant Design theming, and inline or component-level styles that still exist in the current frontend. This also makes later visual iteration cheaper and keeps the organic palette centralized.

**Alternatives considered**:
- Keep color constants only in TypeScript: rejected because global shell styling and future page-level CSS cannot reuse the tokens cleanly.
- Fully redesign styling with a different styling framework first: rejected because the current feature is a polish pass, not a framework migration.

## Decision: Add a reusable paper-grain layer as a shell-level atmospheric component

**Rationale**: The current shell already uses blurred organic shapes. A paper-grain overlay complements that direction and can be applied globally without changing each page independently. It also satisfies the requirement for a visible shared texture treatment.

**Alternatives considered**:
- Add texture separately to each page: rejected because it duplicates work and risks visual inconsistency.
- Use a purely flat background with no overlay: rejected because it misses the product-shell polish goal.

## Decision: Introduce one lightweight dashboard visualization area using `@ant-design/charts`

**Rationale**: The admin dashboard needs at least one visual insight region, and the project already uses Ant Design. `@ant-design/charts` fits the existing component ecosystem and offers a faster path to a credible dashboard visualization than building a custom chart area from scratch.

**Alternatives considered**:
- Skip charts and use only stat cards: rejected because the spec explicitly calls for a visual insight area.
- Use a different charting library: rejected because it adds unnecessary design and dependency inconsistency when an Ant Design-aligned option exists.

## Decision: Keep the sticky pill navbar but adapt it for smaller viewports instead of forcing a desktop-only layout

**Rationale**: The existing floating shell is already visually strong. The problem is not the concept but its current rigidity. A compact/mobile-safe navigation treatment retains the distinctive look while meeting the responsiveness requirement.

**Alternatives considered**:
- Replace the pill navbar with a standard header: rejected because it loses an existing distinctive part of the visual identity.
- Leave the current navbar unchanged and accept horizontal overflow: rejected because it violates the mobile usability success criteria.
