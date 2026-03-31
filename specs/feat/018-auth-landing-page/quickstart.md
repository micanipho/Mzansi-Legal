# Quickstart: Auth Pages and Landing Page

**Feature**: feat/018-auth-landing-page
**Date**: 2026-03-31

---

## Prerequisites

- Node.js 18+, npm 9+
- .NET 9 SDK
- PostgreSQL running (Docker: `mzansi-pg` container) — see project README
- Backend running on `https://localhost:44301` (or configured `NEXT_PUBLIC_API_BASE_URL`)

---

## 1. Start the Backend

```bash
cd backend
dotnet run --project src/backend.Web.Host
```

The ABP Zero API will be available at `https://localhost:44301`.

To verify the admin seed account exists:
```
POST https://localhost:44301/api/TokenAuth/Authenticate
{
  "userNameOrEmailAddress": "admin@mzansilegal.co.za",
  "password": "<admin-password-from-deployment-guide>",
  "rememberClient": false
}
```
Expect `200 OK` with an `accessToken`.

---

## 2. Start the Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend available at `http://localhost:3000`.

---

## 3. Verify Auth Flows

### Registration
1. Navigate to `http://localhost:3000/en/auth#register`
2. Fill in name, email, password, select preferred language
3. Submit → expect redirect to `http://localhost:3000/en/` with initials in navbar

### Sign In
1. Navigate to `http://localhost:3000/en/auth`
2. Enter registered email + password
3. Submit → expect redirect to home (user) or `/en/admin/dashboard` (admin)

### Route Protection
1. Sign out (or clear cookies)
2. Navigate directly to `http://localhost:3000/en/contracts`
3. Expect redirect to `/en/auth`
4. Navigate to `http://localhost:3000/en/admin/dashboard`
5. Expect redirect to `/en/auth`

### Ask Page Guard
1. Sign out, navigate to `/en/ask`
2. Type a question and submit
3. Expect redirect to `/en/auth`

### Landing Page CTA
1. Signed out: `/en/` hero shows "Get Started" → `/en/auth`
2. Signed in: `/en/` hero shows "Go to App" → `/en/ask`

---

## 4. Cookie Inspection

Open DevTools → Application → Cookies → `localhost`:
- `ml_token`: raw JWT string
- `ml_user`: JSON with `name`, `emailAddress`, `isAdmin`, `preferredLanguage`, `expiresAt`

To sign out manually: clear both cookies and reload.

---

## 5. Troubleshooting

| Issue | Fix |
|---|---|
| "Invalid credentials" on admin sign-in | Check admin seed password in `HostRoleAndUserCreator.cs` |
| Redirect loop on `/contracts` | Check `proxy.ts` — ensure `ml_token` cookie is being read correctly |
| Initials not showing | Check `useAuth()` returns non-null `user.name`; inspect `ml_user` cookie content |
| Language not persisting | Check `ml_user` cookie includes `preferredLanguage` field after registration |
