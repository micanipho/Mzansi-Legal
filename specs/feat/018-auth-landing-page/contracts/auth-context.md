# UI Contract: AuthContext & AuthProvider

**Feature**: feat/018-auth-landing-page

---

## AuthProvider

Wraps the application tree — placed in `[locale]/layout.tsx` alongside `AntdProvider`.

**Responsibilities**:
- On mount: read `ml_user` cookie; validate `expiresAt`; restore session or clear stale cookies.
- Expose `AuthContext` to all child components.
- Execute `signIn`, `register`, and `signOut` actions.
- On `signIn`/`register`: write `ml_token` and `ml_user` cookies.
- On `signOut`: clear both cookies via `max-age=0`, reset `user` to `null`.

**Placement**:
```tsx
// [locale]/layout.tsx
<AntdProvider>
  <AuthProvider>
    {children}
  </AuthProvider>
</AntdProvider>
```

---

## useAuth() Hook

```ts
const { user, isLoading, signIn, register, signOut } = useAuth();
```

| Return value | Type | Description |
|---|---|---|
| `user` | `AuthUser \| null` | `null` when signed out or loading |
| `isLoading` | `boolean` | `true` during session restore or auth in-flight |
| `signIn(credentials)` | `Promise<void>` | Throws on failure with user-friendly message |
| `register(data)` | `Promise<void>` | Throws on failure with user-friendly message |
| `signOut()` | `void` | Clears cookies and resets context synchronously |

---

## AppNavbar Integration

`AppNavbar` reads `useAuth()` and renders:

| Auth State | Navbar Renders |
|---|---|
| Signed out | "Sign In" link → `/[locale]/auth` |
| Signed in (non-admin) | User initials avatar + "Sign Out" dropdown item |
| Signed in (admin) | User initials avatar + "Admin Dashboard" link + "Sign Out" |

**Initials generation**: Take the first letter of `user.name`. If `user.name` has multiple words, take first letter of each (max 2), uppercase.

---

## Route Protection (Middleware)

`proxy.ts` (Next.js middleware) checks `ml_token` cookie for protected routes:

```
/[locale]/contracts          → requires ml_token; redirect to /[locale]/auth if absent
/[locale]/admin/dashboard    → requires ml_token; redirect to /[locale]/auth if absent
```

In-page guards in those pages serve as fallbacks for client-side navigation.

---

## Admin In-Page Guard Pattern

```ts
// admin/dashboard/page.tsx
const { user, isLoading } = useAuth();

if (isLoading) return <LoadingSpinner />;
if (!user) { router.push(createLocalizedPath(locale, 'auth')); return null; }
if (!user.isAdmin) { router.push(createLocalizedPath(locale, appRoutes.home)); return null; }
```

---

## Ask Page — Submit Guard

```ts
// QaChatPage.tsx (or ask/page.tsx submit handler)
const { user } = useAuth();

const handleSubmit = () => {
  if (!user) {
    router.push(createLocalizedPath(locale, 'auth'));
    return;
  }
  // proceed with question
};
```

---

## Landing Page Auth-Aware CTA

In `/[locale]/page.tsx`, the hero section adapts based on auth state:

| Auth State | CTA Label | Destination |
|---|---|---|
| Signed out | `t("home.heroCtaGuest")` ("Get Started") | `/[locale]/auth` |
| Signed in | `t("home.heroCtaUser")` ("Go to App") | `/[locale]/ask` |

---

## Translation Keys (new additions)

These keys must be added to all four locale files (`en.json`, `zu.json`, `st.json`, `af.json`):

### `home` namespace (new keys)

| Key | English | Notes |
|---|---|---|
| `heroCtaGuest` | "Get Started" | CTA for unauthenticated visitors |
| `heroCtaUser` | "Go to App" | CTA for signed-in users |

### `auth` namespace (new namespace)

| Key | English | Notes |
|---|---|---|
| `signInTitle` | "Welcome back" | Sign-in tab heading |
| `registerTitle` | "Create your account" | Register tab heading |
| `emailLabel` | "Email address" | Input label |
| `passwordLabel` | "Password" | Input label |
| `nameLabel` | "Full name" | Input label |
| `languageLabel` | "Preferred language" | Selector label |
| `signInButton` | "Sign in" | Submit button |
| `registerButton` | "Create account" | Submit button |
| `switchToRegister` | "Don't have an account? Register" | Tab switch link |
| `switchToSignIn` | "Already have an account? Sign in" | Tab switch link |
| `invalidCredentials` | "Incorrect email or password" | Error message |
| `emailTaken` | "An account with this email already exists" | Error message |
| `signOutMenuItem` | "Sign out" | Navbar dropdown item |
| `adminDashboardLink` | "Admin Dashboard" | Navbar link for admins |
