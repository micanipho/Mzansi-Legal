# Data Model: Auth Pages and Landing Page

**Feature**: feat/018-auth-landing-page
**Date**: 2026-03-31

> This feature introduces no new backend entities or database migrations.
> All data models below are frontend-only (TypeScript interfaces + cookies).

---

## Frontend Data Model

### AuthUser

Represents a signed-in user stored in React context and derived from the decoded JWT + registration data.

| Field | Type | Source | Notes |
|---|---|---|---|
| `userId` | `number` | JWT claim `sub` or `nameid` | Integer user ID from ABP |
| `name` | `string` | JWT claim `unique_name` or session refresh | Display name for navbar initials |
| `userName` | `string` | JWT claim | Login username |
| `emailAddress` | `string` | JWT claim | User email |
| `isAdmin` | `boolean` | JWT `role` claim contains `"Admin"` | Drives role-based routing and admin nav link |
| `token` | `string` | `AuthenticateResultModel.accessToken` | Raw JWT; excluded from `ml_user` cookie |
| `expireInSeconds` | `number` | `AuthenticateResultModel.expireInSeconds` | Used to compute absolute expiry |
| `expiresAt` | `number` | `Date.now() + expireInSeconds * 1000` | Absolute timestamp for session validation |
| `preferredLanguage` | `string` | Registration form selection | One of `"en"`, `"zu"`, `"st"`, `"af"`; defaults to `"en"` if absent |

**Session restore rules**:
- On mount, `AuthProvider` reads `ml_user` cookie and parses to `AuthUser`.
- If `Date.now() > expiresAt`, the session is expired — clear cookies and set `user = null`.
- If `preferredLanguage` is absent, default to `"en"`.
- `isAdmin` defaults to `false` if the `role` claim is absent or unparseable.

---

### SignInCredentials

Input shape for the sign-in form — maps to `AuthenticateModel` on the backend.

| Field | Type | Validation |
|---|---|---|
| `userNameOrEmailAddress` | `string` | Required; max 256 chars |
| `password` | `string` | Required; max 32 chars |
| `rememberClient` | `boolean` | Optional; always `false` for MVP |

---

### RegisterData

Input shape for the registration form — maps to `RegisterInput` on the backend plus client-side language preference.

| Field | Type | Validation |
|---|---|---|
| `name` | `string` | Required; max 64 chars |
| `surname` | `string` | Required; max 64 chars; defaults to same as `name` if blank (ABP requires it) |
| `userName` | `string` | Required; max 256 chars; set equal to `emailAddress` for simplicity |
| `emailAddress` | `string` | Required; valid email format; max 256 chars |
| `password` | `string` | Required; min 6 chars; max 32 chars |
| `preferredLanguage` | `string` | Required; one of `"en"`, `"zu"`, `"st"`, `"af"`; defaults to `"en"` |

**Backend note**: `preferredLanguage` is NOT sent to the ABP `Register` endpoint (which does not accept this field). It is stored in `ml_user` cookie after successful registration + auto-sign-in.

---

### AuthContextValue

The shape of the React Context value exposed to all components via `useAuth()`.

```ts
interface AuthContextValue {
  user: AuthUser | null;      // null = signed out or loading
  isLoading: boolean;          // true during session restore or in-flight auth
  signIn: (credentials: SignInCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  signOut: () => void;
}
```

---

## Cookie Storage

| Cookie | Value | `HttpOnly` | `SameSite` | `path` | `max-age` |
|---|---|---|---|---|---|
| `ml_token` | Raw JWT string | `false` (JS-readable) | `Lax` | `/` | `expireInSeconds` from auth response |
| `ml_user` | `JSON.stringify(AuthUser)` — `token` field excluded | `false` (JS-readable) | `Lax` | `/` | `expireInSeconds` from auth response |

Both cookies are cleared atomically on `signOut()` via `max-age=0`.

**No localStorage is used.**

---

## JWT Payload Structure (ABP Zero standard)

After base64url-decoding the `accessToken` payload:

```json
{
  "sub": "1",
  "jti": "guid",
  "iat": 1711740000,
  "nbf": 1711740000,
  "exp": 1711826400,
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "sipho",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "User"
}
```

**Role claim key**: `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`

```ts
const roles = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
const isAdmin = Array.isArray(roles)
  ? roles.includes("Admin")
  : roles === "Admin";
```

---

## Backend (no changes)

No new entities, migrations, or DTOs. The one backend change is updating the seeded admin account credentials in `HostRoleAndUserCreator.cs` — this does not affect the data model.

See [contracts/auth-api.md](./contracts/auth-api.md) for endpoint request/response shapes.
