# Quickstart: Auth, Roles & Landing Page

**Feature**: feat/016-auth-pages-integration

---

## Prerequisites

- Backend running at `https://localhost:44311` (or set `NEXT_PUBLIC_BASE_URL` in `.env.local`)
- At least one user account in the database:
  - An **admin** user (role = `Admin`) — created via ABP Zero seed or admin UI
  - A **regular** user (role = `User`) — can be registered via the new auth page

## Running the Frontend

```bash
cd frontend
npm install        # (if first time or after package changes)
npm run dev
```

App runs at `http://localhost:3000`.

---

## Testing Sign-In Flow

1. Navigate to `http://localhost:3000/en/auth`
2. Enter admin credentials → expect redirect to `/en/admin/dashboard`
3. Enter regular user credentials → expect redirect to `/en/ask`
4. Enter wrong password → expect inline error, email field preserved
5. Click "Don't have an account? Register" → form switches to registration

## Testing Registration Flow

1. Navigate to `http://localhost:3000/en/auth#register`
2. Fill in all fields → submit
3. Expect: auto sign-in + redirect to `/en/ask`
4. Check DevTools → Application → Cookies → `localhost`: `ml_token` and `ml_user` present. No localStorage entries written.

## Testing Role-Based Nav

1. Sign in as admin → check navbar shows "Admin" link
2. Sign in as regular user → check navbar does NOT show "Admin" link
3. As regular user, navigate directly to `/en/admin/dashboard` → expect access-denied redirect

## Testing Sign-Out

1. Click "Sign Out" in navbar (or avatar dropdown)
2. Check DevTools → Application → Cookies: `ml_token` and `ml_user` should be absent
3. Navbar shows "Sign In" link. No localStorage entries were ever written.

## Testing Landing Page

1. Visit `http://localhost:3000/en` while signed out → hero CTA reads "Get Started" linking to `/en/auth`
2. Sign in, return to `http://localhost:3000/en` → hero CTA reads "Go to App" linking to `/en/ask`
3. Verify no olive color remains — all primary-colored elements should be deep teal (`#0d7377`)

## Testing i18n

1. Switch locale to Zulu (`/zu/auth`) → all labels, placeholders, buttons in Zulu
2. Repeat for Sesotho (`/st/auth`) and Afrikaans (`/af/auth`)

---

## Checking the JWT for Roles (Debug)

Open browser DevTools console and run:

```js
// Read ml_token from cookies
const token = document.cookie.split('; ').find(r => r.startsWith('ml_token='))?.split('=')[1];
const payload = JSON.parse(atob(token.split('.')[1]));
console.log(payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']);
// Expected: "Admin" for admins, "User" for regular users
```

---

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `NEXT_PUBLIC_BASE_URL` | `https://localhost:44311` | Backend API root |
| `NEXT_PUBLIC_DEFAULT_LOCALE` | `en` | Default language |
