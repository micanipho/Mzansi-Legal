# Data Model: Auth, Roles & Landing Page

**Feature**: feat/016-auth-pages-integration
**Date**: 2026-03-30

> This feature introduces no new backend entities or database migrations.
> All data models below are frontend-only (TypeScript interfaces + localStorage).

---

## Frontend Data Model

### AuthUser

Represents a signed-in user as stored in React context and derived from the decoded JWT.

| Field | Type | Source | Notes |
|---|---|---|---|
| `userId` | `number` | JWT claim `sub` or `nameid` | Long integer user ID from ABP |
| `name` | `string` | JWT claim `unique_name` or session | Display name |
| `userName` | `string` | JWT claim | Login username |
| `emailAddress` | `string` | JWT claim | User's email |
| `isAdmin` | `boolean` | JWT `role` claim contains `"Admin"` | Drives routing and nav visibility |
| `token` | `string` | `AuthenticateResultModel.accessToken` | Raw JWT string |
| `expireInSeconds` | `number` | `AuthenticateResultModel.expireInSeconds` | Used to compute expiry timestamp |
| `expiresAt` | `number` | `Date.now() + expireInSeconds * 1000` | Absolute expiry for session validation |

**Persistence**: Stored exclusively in cookies — `ml_token` (raw JWT) and `ml_user` (serialised `AuthUser` JSON, token field excluded). No `localStorage` usage.

**Validation rules**:
- Session is considered expired when `Date.now() > expiresAt`.
- `isAdmin` defaults to `false` if the JWT `role` claim is absent or unparseable.

---

### SignInCredentials

Input shape for the sign-in form — maps directly to `AuthenticateModel` on the backend.

| Field | Type | Validation |
|---|---|---|
| `userNameOrEmailAddress` | `string` | Required; max 256 chars |
| `password` | `string` | Required; max 32 chars |
| `rememberClient` | `boolean` | Optional; defaults to `false` |

---

### RegisterData

Input shape for the registration form — maps to `RegisterInput` on the backend.

| Field | Type | Validation |
|---|---|---|
| `name` | `string` | Required; max 64 chars |
| `surname` | `string` | Required; max 64 chars |
| `userName` | `string` | Required; max 256 chars; must not be an email unless it matches `emailAddress` |
| `emailAddress` | `string` | Required; valid email format; max 256 chars |
| `password` | `string` | Required; max 32 chars |

---

### AuthContextValue

The shape of the React Context value exposed to all components via `useAuth()`.

```ts
interface AuthContextValue {
  user: AuthUser | null;      // null = signed out
  isLoading: boolean;          // true during restore-from-storage or in-flight sign-in/register
  signIn: (credentials: SignInCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  signOut: () => void;
}
```

---

## Cookie Storage

| Cookie | Value | `HttpOnly` | Purpose |
|---|---|---|---|
| `ml_token` | Raw JWT string | `false` (JS-readable) | Bearer token for API calls; readable by middleware for route protection |
| `ml_user` | `JSON.stringify(AuthUser)` (token field omitted) | `false` (JS-readable) | Session restore on mount without JWT re-decode |

Both cookies share: `path=/`, `SameSite=Lax`, `max-age` = `expireInSeconds` from the auth response.
Both are cleared atomically by `signOut()` via `max-age=0`.

**No localStorage is used.**

---

## Backend API Contracts (existing — no changes)

See [contracts/auth-api.md](./contracts/auth-api.md) for the full request/response shapes.

Summary:
- `POST /api/TokenAuth/Authenticate` → `{ accessToken, encryptedAccessToken, expireInSeconds, userId }`
- `POST /api/services/app/Account/Register` → `{ canLogin: bool, ... }`

After registration, the `signIn` action is called automatically using the submitted credentials.
