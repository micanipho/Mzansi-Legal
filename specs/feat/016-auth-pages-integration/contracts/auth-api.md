# API Contract: Authentication Endpoints

**Feature**: feat/016-auth-pages-integration
**Backend**: ABP Zero — endpoints exist; **no backend changes required**

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
1. Decode the `accessToken` (base64url decode payload, no signature verification needed).
2. Extract `role` claim(s) — check if `"Admin"` is included.
3. Persist `ml_access_token` and `ml_auth_user` to `localStorage`.
4. Redirect based on `isAdmin`.

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
1. If `canLogin === true`, automatically call `POST /api/TokenAuth/Authenticate` with the same `userNameOrEmailAddress` (use `userName`) and `password`.
2. Follow the sign-in success flow above.

---

## Session Info (read-only, used for display name refresh)

**Endpoint**: `GET /api/services/app/Session/GetCurrentLoginInformations`

**Headers**: `Authorization: Bearer {accessToken}`

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
  "tenant": null,
  "application": { ... }
}
```

**Usage**: Called once on `AuthProvider` mount if a stored token is found, to refresh display name. Not called during sign-in (name is derived from JWT claims for speed).

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
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "User",
  "AspNet.Identity.SecurityStamp": "..."
}
```

The `role` claim key is `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`.
For admin users this value is `"Admin"` (may also be an array `["Admin", "User"]`).

**isAdmin detection**:
```ts
const roles = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
const isAdmin = Array.isArray(roles)
  ? roles.includes("Admin")
  : roles === "Admin";
```
