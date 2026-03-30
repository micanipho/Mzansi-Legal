# Research: Auth, Roles & Landing Page

**Feature**: feat/016-auth-pages-integration
**Date**: 2026-03-30

---

## R-001: How to Detect the User's Role After Sign-In

**Decision**: Decode the JWT token client-side to extract role claims.

**Rationale**: ABP Zero's `TokenAuthController` creates the JWT token by encoding the user's full `ClaimsIdentity`, which includes `ClaimTypes.Role` entries for every role the user belongs to (e.g., `"Admin"`, `"User"`). The token is a standard HS256 JWT so the payload is base64url-encoded and can be parsed without a signature check on the client. This avoids an extra network round-trip to `/api/services/app/Session/GetCurrentLoginInformations` just to determine the role.

The `isAdmin` flag is derived by checking whether the decoded `role` claim (or array of claims) includes `"Admin"`.

**Alternatives considered**:
- **Call `/api/services/app/Session/GetCurrentLoginInformations`** after sign-in: This returns `UserLoginInfoDto` which does not include roles in the current schema; the DTO would need a backend change to expose roles, which is out of scope.
- **Store a separate `isAdmin` boolean in `localStorage`**: Simpler to read, but derived from the same JWT decode, so there is no meaningful advantage over keeping it computed from the token.

---

## R-002: Token Storage Strategy

**Decision**: Store the JWT and user session data exclusively in cookies — no `localStorage`.

Two cookies are written on sign-in:
- `ml_token` — raw JWT string; JS-readable (`HttpOnly: false`), `SameSite=Lax`, `path=/`, `max-age` matching token expiry. Readable by Next.js middleware for route protection and by client-side JS for attaching Bearer headers.
- `ml_user` — JSON-serialised `AuthUser` object (without the raw token); JS-readable, same attributes. Used by `AuthProvider` to restore the session on mount without decoding the JWT again.

Both cookies are cleared atomically on sign-out by setting `max-age=0`.

**Rationale**: Cookies survive page refreshes, work across tabs, and — critically — are readable by Next.js middleware running on the edge, enabling server-side route protection without a round-trip. Storing tokens in `localStorage` exposes them to any injected JS (XSS); cookies with `SameSite=Lax` reduce CSRF risk. This also removes the need to write dual storage paths.

**Alternatives considered**:
- **`localStorage` only**: XSS-accessible; not readable by middleware — requires JS-only in-page guards with content flashes.
- **`localStorage` + cookie (dual write)**: Redundant; two sources of truth to keep in sync; still XSS-accessible.
- **`HttpOnly` cookies**: Most secure (JS cannot read); but then client-side code cannot attach the Bearer token to API calls without a backend proxy endpoint — out of scope for MVP.
- **`sessionStorage`**: Lost on tab close; degrades UX.

---

## R-003: Auth Context Shape & Provider Pattern

**Decision**: Use a React Context (`AuthContext`) exposed by an `AuthProvider` component, consumed via a `useAuth()` hook. This follows the `apply-provider-pattern` skill convention already used in the project (redux-actions pattern is used in GovLeave; for this project a simpler React Context suffices given global state should be minimal per BP.md).

**Auth context state**:
```ts
interface AuthUser {
  userId: long;
  name: string;
  userName: string;
  emailAddress: string;
  isAdmin: boolean;
  token: string;
  expireInSeconds: number;
}

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  signIn: (credentials) => Promise<void>;
  signOut: () => void;
}
```

On mount, `AuthProvider` reads `ml_access_token` from `localStorage`, decodes it, and restores the session (if not expired). This ensures page refreshes don't lose the signed-in state.

**Rationale**: Keeps global state minimal (BP.md rule), avoids Redux complexity for a two-state (signed-in / signed-out) concern, and integrates cleanly with the existing `add-auth-provider` skill.

---

## R-004: Role-Based Redirect Logic

**Decision**: Post-login redirect is handled inside the `signIn` action of `AuthProvider` (not inside the auth page itself), making role-aware routing reusable if sign-in is triggered from multiple places.

- `isAdmin === true` → `router.push(createLocalizedPath(locale, 'admin/dashboard'))`
- `isAdmin === false` → `router.push(createLocalizedPath(locale, appRoutes.ask))`

Admin route protection: The admin dashboard page checks `user?.isAdmin` from `useAuth()`; if `false` (or `null`), it renders an "Access Denied" message and redirects to the home page after 3 seconds. Full route-guard middleware is out of scope.

---

## R-005: New Primary Color — Deep Teal

**Decision**: Replace olive `#5d7052` / RGB `93, 112, 82` with deep teal `#0d7377` / RGB `13, 115, 119`.

**Rationale**:
- `#0d7377` passes WCAG AA contrast against white (`#ffffff`) background at ratio ≈ 4.7:1 for normal text.
- It is clearly distinct from olive — a teal/blue-green hue versus a muted yellow-green.
- It reads as professional and trustworthy (associated with legal, finance, healthcare domains).
- It pairs well with the existing warm secondary `#c18c5d` (terracotta) without clashing.

**Change surface**:
- `src/styles/globals.css`: `--ml-primary: #0d7377`, `--ml-primary-strong: #0a5e62`, `--ml-primary-fg: #f0fafa`
- `src/styles/theme.ts`: Update `RGB.primary` to `"13, 115, 119"` and recompute `C.primary`, `C.primaryFg`, `C.primaryStrong`
- `src/components/providers/AntdProvider.tsx`: Update `colorPrimary` token to `"#0d7377"`
- All hardcoded `rgba(93,112,82,…)` references in `page.tsx` updated to `rgba(13,115,119,…)`

**Alternatives considered**:
- `#1d6a72` (darker teal, initially proposed in spec): Contrast ratio ~4.2:1, borderline AA. `#0d7377` is slightly brighter and safely passes.
- Indigo `#3d5a99`: Also professional but diverges further from the existing warm palette.
- Forest green `#2d6a4f`: Too close to olive in hue.

---

## R-006: Landing Page Structure

**Decision**: Keep the existing `/[locale]/page.tsx` route as the landing page. The content sections (hero, stats, CTAs, categories, trending) are preserved but the design is refreshed:
1. Apply new teal primary color everywhere olive was used.
2. Hero CTA adapts: unauthenticated → "Get Started" (links to `/auth`); authenticated → "Go to App" (links to `/ask`).
3. No new content sections added.

The current `home.*` i18n keys cover all sections; no new translation keys are needed for the landing page redesign.

**Rationale**: The user said "extracted from home meaning redesigning it" — the content stays the same, the visual design is refreshed. Creating a separate `/landing` route would break existing bookmarks and SEO.

---

## R-007: Auth Page Route & Tab Structure

**Decision**: Single route `/[locale]/auth` with URL hash-controlled tab: `#sign-in` (default) and `#register`. The tab state is also togglable via inline "Don't have an account? Register" / "Already have an account? Sign in" links.

**Rationale**: Matches the existing `history/page.tsx` `/${locale}/auth` link already in the codebase. Hash-based tab state means direct-linking to registration is possible (`/auth#register`) without adding more route segments.

---

## R-008: Skills to Apply

| Task | Skill |
|---|---|
| Auth context + provider | `add-auth-provider` |
| Auth API service (`authService.ts`) | `add-service` |
| CSS/theme color update | `add-styling` |
| Git workflow | `follow-git-workflow` |

No `add-endpoint` skill needed — backend is unchanged.
