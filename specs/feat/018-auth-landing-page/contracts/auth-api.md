# API Contract: Authentication Endpoints

**Feature**: feat/018-auth-landing-page
**Backend**: ABP Zero — endpoints exist; **no backend changes required** (except admin seed update)

---

## Sign In

**Endpoint**: `POST /api/TokenAuth/Authenticate`

**Request**:
```json
{
  "userNameOrEmailAddress": "string (required, max 256)",
  "password": "string (required, max 32)",
  "rememberClient": false
}
```

**Success Response** `200 OK`:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "encryptedAccessToken": "string",
  "expireInSeconds": 86400,
  "userId": 1
}
```

**Error Responses**:

| HTTP Status | ABP Error Code | When |
|---|---|---|
| `400 Bad Request` | `UserEmailNotConfirmed` | Email not confirmed (if enabled) |
| `401 Unauthorized` | `InvalidUserNameOrPassword` | Wrong credentials |
| `400 Bad Request` | `UserIsNotActive` | Account deactivated |

**Frontend action after success**:
1. Decode `accessToken` payload (base64url decode — no signature verification needed).
2. Extract `role` claim → compute `isAdmin`.
3. Extract `name`, `userName`, `emailAddress` from claims.
4. Write `ml_token` and `ml_user` cookies with `max-age = expireInSeconds`.
5. Redirect based on `isAdmin` (admin → `/[locale]/admin/dashboard`, user → `/[locale]/`).

---

## Register

**Endpoint**: `POST /api/services/app/Account/Register`

**Request**:
```json
{
  "name": "string (required, max 64)",
  "surname": "string (required, max 64)",
  "userName": "string (required, max 256)",
  "emailAddress": "string (required, valid email, max 256)",
  "password": "string (required, max 32)",
  "captchaResponse": null
}
```

> **Note**: `preferredLanguage` is NOT sent to this endpoint. It is stored in the `ml_user` cookie after the post-registration sign-in completes.

**Success Response** `200 OK`:
```json
{
  "canLogin": true
}
```

**Error Responses**:

| HTTP Status | When |
|---|---|
| `400 Bad Request` | Username already taken, email already registered, or validation failure |
| `400 Bad Request` | Username is an email but differs from `emailAddress` |

**Frontend action after success**:
1. If `canLogin === true`, automatically call `POST /api/TokenAuth/Authenticate` with `userNameOrEmailAddress = userName` and same `password`.
2. On sign-in success, merge `preferredLanguage` from the registration form into `AuthUser` before writing `ml_user` cookie.
3. Redirect to home page (newly registered users are not admins).

---

## Session Info (read-only display refresh)

**Endpoint**: `GET /api/services/app/Session/GetCurrentLoginInformations`

**Headers**: `Authorization: Bearer {ml_token}`

**Success Response** `200 OK`:
```json
{
  "user": {
    "id": 1,
    "name": "Sipho",
    "surname": "Dlamini",
    "userName": "sipho",
    "emailAddress": "sipho@example.com"
  },
  "tenant": null
}
```

**Usage**: Called once on `AuthProvider` mount if a stored token is found, to refresh `name` / `emailAddress` (in case the user updated their profile elsewhere). Not called during sign-in (name is derived from JWT claims for speed).

---

## JWT Payload Structure

```json
{
  "sub": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "sipho",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "User",
  "exp": 1711826400
}
```

**isAdmin detection**:
```ts
const roles = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
const isAdmin = Array.isArray(roles)
  ? roles.includes("Admin")
  : roles === "Admin";
```
