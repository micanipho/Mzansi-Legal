const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE ??
  process.env.NEXT_PUBLIC_BASE_URL ??
  "http://localhost:21021";
const LOCAL_API_BASE_CANDIDATES = ["http://localhost:21021", "http://localhost:5000"];

// ABP Zero multi-tenant header — ID 1 is the default tenant seeded on first run
const ABP_HEADERS = {
  "Content-Type": "application/json",
  "Abp-TenantId": "1",
};

function isLocalBrowserSession(): boolean {
  if (typeof window === "undefined") {
    return false;
  }

  return (
    window.location.hostname === "localhost" ||
    window.location.hostname === "127.0.0.1"
  );
}

function getApiBaseCandidates(): string[] {
  if (!isLocalBrowserSession()) {
    return [API_BASE];
  }

  const candidates = [...LOCAL_API_BASE_CANDIDATES];

  if (!candidates.includes(API_BASE)) {
    candidates.push(API_BASE);
  }

  for (const localBase of LOCAL_API_BASE_CANDIDATES) {
    if (!candidates.includes(localBase)) {
      candidates.push(localBase);
    }
  }

  return candidates;
}

async function fetchWithFallback(
  path: string,
  init: RequestInit
): Promise<Response> {
  let lastNetworkError: TypeError | null = null;

  for (const apiBase of getApiBaseCandidates()) {
    try {
      return await fetch(`${apiBase}${path}`, init);
    } catch (error) {
      if (!(error instanceof TypeError)) {
        throw error;
      }

      lastNetworkError = error;
    }
  }

  throw lastNetworkError ?? new TypeError("Failed to fetch");
}

// ─── Request / Response Types ────────────────────────────────────────────────

export interface SignInCredentials {
  userNameOrEmailAddress: string;
  password: string;
  rememberClient: boolean;
}

export interface RegisterData {
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  password: string;
  preferredLanguage: string;
}

export interface AuthenticateResultModel {
  accessToken: string;
  encryptedAccessToken: string;
  expireInSeconds: number;
  userId: number;
}

export interface RegisterResultModel {
  canLogin: boolean;
}

// ─── JWT Decode Helper ───────────────────────────────────────────────────────

export interface JwtPayload {
  /** name claim */
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"?: string;
  /** role claim — may be a string or string[] */
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string | string[];
  /** nameidentifier (userId) */
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"?: string;
  /** email */
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"?: string;
  /** ABP unique_name claim */
  unique_name?: string;
  email?: string;
  sub?: string;
  exp?: number;
  iat?: number;
  [key: string]: unknown;
}

/**
 * Decode a JWT token payload on the client side (no signature verification).
 */
export function decodeJwtPayload(token: string): JwtPayload {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) return {};
    // Replace URL-safe chars and pad
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);
    const jsonStr = atob(padded);
    return JSON.parse(jsonStr) as JwtPayload;
  } catch {
    return {};
  }
}

// ─── API Functions ───────────────────────────────────────────────────────────

/**
 * Authenticate a user with ABP Zero's TokenAuth endpoint.
 * Returns the raw authenticate result including accessToken and expireInSeconds.
 */
export async function signIn(
  credentials: SignInCredentials
): Promise<AuthenticateResultModel> {
  const res = await fetchWithFallback("/api/TokenAuth/Authenticate", {
    method: "POST",
    headers: ABP_HEADERS,
    body: JSON.stringify(credentials),
  });

  const json = await res.json();

  if (!res.ok) {
    const message = json?.error?.message || json?.error?.details || "Invalid credentials";
    const err = new Error(message) as Error & { status: number };
    err.status = res.status;
    throw err;
  }

  return json.result as AuthenticateResultModel;
}

/**
 * Register a new user account.
 * NOTE: preferredLanguage is NOT sent to the backend — it is stored
 * only in the ml_user cookie by AuthProvider after successful registration.
 */
export async function register(
  data: RegisterData
): Promise<RegisterResultModel> {
  // Backend does not accept preferredLanguage — omit it
  const { preferredLanguage: _omit, ...payload } = data;
  void _omit;

  const res = await fetchWithFallback("/api/services/app/Account/Register", {
    method: "POST",
    headers: ABP_HEADERS,
    body: JSON.stringify(payload),
  });

  const json = await res.json();

  if (!res.ok) {
    const message = json?.error?.message || json?.error?.details || "Registration failed";
    const err = new Error(message) as Error & { status: number };
    err.status = res.status;
    throw err;
  }

  return json.result as RegisterResultModel;
}
