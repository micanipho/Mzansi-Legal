# Research: Auth Pages and Landing Page

**Feature**: feat/018-auth-landing-page
**Date**: 2026-03-31

> All R-001 through R-008 decisions below are carried forward from `feat/016-auth-pages-integration` research (2026-03-30) unchanged, except R-003 and R-009 which are updated/added to address the preferred language selector.

---

## R-001: How to Detect the User's Role After Sign-In

**Decision**: Decode the JWT token client-side to extract role claims.

**Rationale**: ABP Zero's `TokenAuthController` creates the JWT token by encoding the user's full `ClaimsIdentity`, which includes `ClaimTypes.Role` entries for every role the user belongs to (e.g., `"Admin"`, `"User"`). The token is a standard HS256 JWT so the payload is base64url-encoded and can be parsed without a signature check on the client. This avoids an extra network round-trip to `/api/services/app/Session/GetCurrentLoginInformations`.

The `isAdmin` flag is derived by checking whether the decoded `role` claim (or array of claims) includes `"Admin"`.

**Alternatives considered**:
- **Call `/api/services/app/Session/GetCurrentLoginInformations`** after sign-in: Returns `UserLoginInfoDto` which does not include roles in the current schema. Requires backend change — out of scope.
- **Store a separate `isAdmin` boolean in a cookie**: Derived from the same JWT decode, so no meaningful advantage over computing it from the token.

---

## R-002: Token Storage Strategy

**Decision**: Store the JWT and user session data exclusively in cookies — no `localStorage`.

Two cookies are written on sign-in:
- `ml_token` — raw JWT string; JS-readable (`HttpOnly: false`), `SameSite=Lax`, `path=/`, `max-age` matching token expiry. Readable by Next.js middleware for route protection and by client-side JS for attaching Bearer headers.
- `ml_user` — JSON-serialised `AuthUser` object (token field excluded); JS-readable, same attributes. Used by `AuthProvider` to restore session on mount without re-decoding the JWT.

Both cookies are cleared atomically on sign-out via `max-age=0`.

**Rationale**: Cookies survive page refreshes, work across tabs, and are readable by Next.js middleware for server-side route protection without a round-trip. `localStorage` tokens are XSS-accessible and not readable by middleware (causes content flashes). `SameSite=Lax` reduces CSRF risk.

**Alternatives considered**:
- **`localStorage` only**: XSS-accessible; not readable by middleware — requires JS-only in-page guards with content flashes.
- **`HttpOnly` cookies**: Prevents client JS from reading the token, requiring a backend proxy for API calls — out of scope for MVP.
- **`sessionStorage`**: Lost on tab close; degrades UX.

---

## R-003: Auth Context Shape & Provider Pattern (Updated for preferred language)

**Decision**: React Context (`AuthContext`) exposed by `AuthProvider`, consumed via `useAuth()` hook. `AuthUser` includes `preferredLanguage` derived from the JWT or session.

**Auth context state**:
```ts
interface AuthUser {
  userId: number;
  name: string;
  userName: string;
  emailAddress: string;
  isAdmin: boolean;
  token: string;
  expireInSeconds: number;
  expiresAt: number;
  preferredLanguage: string; // e.g. "en", "zu", "st", "af" — from registration or session
}

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  signIn: (credentials: SignInCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  signOut: () => void;
}
```

On mount, `AuthProvider` reads `ml_user` cookie, validates `expiresAt`, and restores session. If `preferredLanguage` is absent from the stored `ml_user` (e.g., for accounts registered before this feature), it defaults to `"en"`.

**Rationale**: Keeps global state minimal (BP.md rule). `preferredLanguage` is already a field on `AppUser` (feat/006); exposing it in the auth context makes it available for locale-selection flows without a separate API call.

---

## R-004: Role-Based Redirect Logic

**Decision**: Post-login redirect handled inside the `signIn` action of `AuthProvider`.

- `isAdmin === true` → `router.push(createLocalizedPath(locale, 'admin/dashboard'))`
- `isAdmin === false` → `router.push(createLocalizedPath(locale, appRoutes.home))`

Admin route protection: The admin dashboard page checks `user?.isAdmin` from `useAuth()`; if `false` or `null`, redirects to home page. No full middleware route guard added (in-page guard is sufficient for this scope).

---

## R-005: Primary Color — Deep Teal (Verification)

**Decision**: Primary color is `#0d7377` (deep teal). If `globals.css` and `theme.ts` still show olive `#5d7052`, update them as part of this feature's scope.

**Why still noted**: The `page.tsx` file examined on 2026-03-31 still contains `rgba(93,112,82,...)` references, confirming the teal migration was not applied in the 016 implementation. This feature must apply it.

**Change surface**:
- `globals.css`: `--ml-primary: #0d7377`, `--ml-primary-strong: #0a5e62`, `--ml-primary-fg: #f0fafa`
- `theme.ts`: `RGB.primary` → `"13, 115, 119"` — recompute `C.primary`, `C.primaryFg`, `C.primaryStrong`
- `AntdProvider.tsx`: `colorPrimary` → `"#0d7377"`
- `page.tsx`: All `rgba(93,112,82,…)` references → `rgba(13,115,119,…)`

---

## R-006: Landing Page Structure

**Decision**: Keep the existing `/[locale]/page.tsx` route as the landing page. Content sections (hero, stats, CTAs, categories, trending) are preserved; design is refreshed with teal color. Hero CTA adapts:
- Unauthenticated → "Get Started" (i18n key `heroCtaGuest`) → `/[locale]/auth`
- Authenticated → "Go to App" (i18n key `heroCtaUser`) → `/[locale]/ask`

No new content sections are added. The search bar on the landing page does not require authentication — it navigates to `/ask` with the query.

---

## R-007: Auth Page Route & Tab Structure

**Decision**: Single route `/[locale]/auth` with URL hash-controlled tabs: `#sign-in` (default) and `#register`. Tabs are also toggleable via inline "Don't have an account? Register" / "Already have an account? Sign in" links.

**Rationale**: Matches the existing `history/page.tsx` link pattern for `/auth`. Hash-based tab state allows direct-linking to registration (`/auth#register`) without additional route segments.

---

## R-008: Skills to Apply

| Task | Skill |
|---|---|
| Auth context + provider | `add-auth-provider` |
| Auth API service (`authService.ts`) | `add-service` |
| CSS/theme color update | `add-styling` |
| Git workflow | `follow-git-workflow` |

No `add-endpoint` skill needed — no new backend endpoints.

---

## R-009: Preferred Language in Registration (New for 018)

**Decision**: The registration form includes a language selector with four options: English (`en`), isiZulu (`zu`), Sesotho (`st`), Afrikaans (`af`). The selected value is passed to the backend's `Register` endpoint in a custom field.

**Backend mapping**: ABP Zero's `AccountAppService.Register` accepts `RegisterInput`. The `AppUser` entity (feat/006) already has a `PreferredLanguage` string field. To persist the preferred language at registration:
- Option A: Extend `RegisterInput` DTO with a `PreferredLanguage` field and update `AccountAppService` to set it on the new user — requires minor backend change.
- Option B: After registration + auto-sign-in, call a separate profile update endpoint to set `PreferredLanguage` — two sequential API calls.
- Option C: Store preferred language in the `ml_user` cookie only (client-side) — not persisted to the database.

**Chosen**: **Option C for MVP** — store `preferredLanguage` in `ml_user` cookie alongside other user data. This avoids any backend change. The value is set at registration time and re-read from the cookie on session restore. If the user changes their language preference later, that can be handled by a separate profile feature. This aligns with the feature scope (no new backend entities or DTO changes).

**Rationale**: The spec states "no backend changes required" for auth. Option A would require modifying a DTO and application service, which is out of scope for this milestone. Option C delivers the language preference in the session context where it matters most (locale-aware UI).

---

## R-010: Middleware Route Protection

**Decision**: Use Next.js middleware (`proxy.ts` / `middleware.ts`) to check for the presence of the `ml_token` cookie on protected routes (`/[locale]/contracts`, `/[locale]/admin/dashboard`). If the cookie is absent, redirect to `/[locale]/auth`.

**Rationale**: Middleware runs before the page renders, preventing content flash. It also means the in-page guards in `contracts/page.tsx` and `admin/dashboard/page.tsx` are fallbacks rather than the primary mechanism.

**Implementation note**: `next-intl` middleware already runs in `proxy.ts`. The route protection logic is added to the same middleware using `NextResponse.redirect` before passing to `next-intl`.
