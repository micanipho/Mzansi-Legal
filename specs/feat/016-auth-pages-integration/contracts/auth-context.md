# UI Contract: AuthContext & AuthProvider

**Feature**: feat/016-auth-pages-integration

---

## AuthProvider

Wraps the application tree (added to `[locale]/layout.tsx` alongside `AntdProvider`).

**Responsibilities**:
- On mount: read `ml_user` cookie; validate expiry against `expiresAt` field; restore session or clear stale cookies.
- Expose `AuthContext` to all child components.
- Execute `signIn`, `register`, and `signOut` actions.
- On `signIn`/`register`: write `ml_token` and `ml_user` cookies. On `signOut`: clear both cookies via `max-age=0`.

**Placement**: Inside `[locale]/layout.tsx`, wrapping `{children}` after `AntdProvider`.

---

## useAuth() Hook

```ts
const { user, isLoading, signIn, register, signOut } = useAuth();
```

| Return value | Type | Description |
|---|---|---|
| `user` | `AuthUser \| null` | `null` when signed out or loading |
| `isLoading` | `boolean` | `true` during session restore or auth in-flight |
| `signIn(credentials)` | `Promise<void>` | Throws on failure with a user-friendly message |
| `register(data)` | `Promise<void>` | Throws on failure with a user-friendly message |
| `signOut()` | `void` | Clears storage and resets context synchronously |

---

## AppNavbar Integration

The `AppNavbar` component reads `useAuth()` and renders:

| State | Nav change |
|---|---|
| Signed out | "Sign In" link → `/[locale]/auth` |
| Signed in (non-admin) | User display name + "Sign Out" button |
| Signed in (admin) | User display name + "Admin" link → `/[locale]/admin/dashboard` + "Sign Out" |

---

## Admin Route Protection (in-page check)

The admin dashboard page (`/[locale]/admin/dashboard/page.tsx`) uses this pattern at the top of the component:

```ts
const { user, isLoading } = useAuth();

if (isLoading) return <LoadingSpinner />;
if (!user?.isAdmin) {
  // Show "Access Denied" and redirect after 3 s
  router.push(createLocalizedPath(locale, appRoutes.home));
  return <AccessDenied />;
}
```

No Next.js middleware is added — in-page guard is sufficient for this scope.

---

## Landing Page Auth-Aware CTA

In `/[locale]/page.tsx`, the hero CTA button reads `useAuth()`:

| State | CTA label | CTA destination |
|---|---|---|
| Signed out | "Get Started" / locale key `heroCtaGuest` | `/[locale]/auth` |
| Signed in | "Go to App" / locale key `heroCtaUser` | `/[locale]/ask` |
