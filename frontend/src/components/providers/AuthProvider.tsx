"use client";

import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { useRouter } from "next/navigation";
import { useLocale } from "next-intl";
import {
  signIn as apiSignIn,
  register as apiRegister,
  decodeJwtPayload,
  type SignInCredentials,
  type RegisterData,
} from "@/services/authService";
import { createLocalizedPath, appRoutes } from "@/i18n/routing";

// ─── AuthUser ────────────────────────────────────────────────────────────────

export interface AuthUser {
  userId: number;
  name: string;
  userName: string;
  emailAddress: string;
  isAdmin: boolean;
  token: string;
  expireInSeconds: number;
  expiresAt: number;
  preferredLanguage: string;
}

// ─── Context shape ───────────────────────────────────────────────────────────

export interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  signIn: (credentials: SignInCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  signOut: () => void;
}

// ─── Cookie helpers ──────────────────────────────────────────────────────────

const TOKEN_COOKIE = "ml_token";
const USER_COOKIE = "ml_user";

function setCookie(name: string, value: string, maxAge: number) {
  document.cookie = `${name}=${encodeURIComponent(value)}; path=/; max-age=${maxAge}; SameSite=Lax`;
}

function deleteCookie(name: string) {
  document.cookie = `${name}=; path=/; max-age=0; SameSite=Lax`;
}

function getCookie(name: string): string | null {
  if (typeof document === "undefined") return null;
  const prefix = `${name}=`;
  for (const part of document.cookie.split(";")) {
    const trimmed = part.trim();
    if (trimmed.startsWith(prefix)) {
      return decodeURIComponent(trimmed.slice(prefix.length));
    }
  }
  return null;
}

// ─── Context ─────────────────────────────────────────────────────────────────

export const AuthContext = createContext<AuthContextValue | null>(null);

// ─── Provider ────────────────────────────────────────────────────────────────

interface AuthProviderProps {
  children: ReactNode;
}

export default function AuthProvider({ children }: AuthProviderProps) {
  const router = useRouter();
  const locale = useLocale();

  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Restore session from ml_user cookie on mount
  useEffect(() => {
    try {
      const raw = getCookie(USER_COOKIE);
      if (raw) {
        const parsed = JSON.parse(raw) as AuthUser;
        // Validate the stored session hasn't expired
        if (parsed.expiresAt && Date.now() < parsed.expiresAt) {
          setUser(parsed);
        } else {
          // Expired — clear cookies
          deleteCookie(TOKEN_COOKIE);
          deleteCookie(USER_COOKIE);
        }
      }
    } catch {
      // Corrupt cookie — clear
      deleteCookie(TOKEN_COOKIE);
      deleteCookie(USER_COOKIE);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // ── signIn ──────────────────────────────────────────────────────────────

  const signIn = useCallback(
    async (credentials: SignInCredentials) => {
      const result = await apiSignIn(credentials);

      const payload = decodeJwtPayload(result.accessToken);

      // Extract name from JWT claims
      const nameClaim =
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
        payload["unique_name"] ||
        "";

      // Extract role — ABP may return a single string or array
      const roleClaim =
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      const isAdmin = Array.isArray(roleClaim)
        ? roleClaim.includes("Admin")
        : roleClaim === "Admin";

      // Extract email
      const emailClaim =
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
        payload["email"] ||
        credentials.userNameOrEmailAddress;

      const expiresAt = Date.now() + result.expireInSeconds * 1000;

      const authUser: AuthUser = {
        userId: result.userId,
        name: nameClaim as string,
        userName: credentials.userNameOrEmailAddress,
        emailAddress: emailClaim as string,
        isAdmin,
        token: result.accessToken,
        expireInSeconds: result.expireInSeconds,
        expiresAt,
        preferredLanguage: locale,
      };

      // Persist: raw JWT in ml_token, user object (without token) in ml_user
      setCookie(TOKEN_COOKIE, result.accessToken, result.expireInSeconds);
      const { token: _omit, ...userWithoutToken } = authUser;
      void _omit;
      setCookie(USER_COOKIE, JSON.stringify(userWithoutToken), result.expireInSeconds);

      setUser(authUser);

      // Role-based redirect
      if (isAdmin) {
        router.push(createLocalizedPath(locale, appRoutes.adminDashboard));
      } else {
        router.push(createLocalizedPath(locale, appRoutes.home));
      }
    },
    [locale, router]
  );

  // ── register ────────────────────────────────────────────────────────────

  const register = useCallback(
    async (data: RegisterData) => {
      // Call backend to create account (preferredLanguage not sent to server)
      await apiRegister(data);

      // Auto sign-in with the same credentials
      const result = await apiSignIn({
        userNameOrEmailAddress: data.emailAddress,
        password: data.password,
        rememberClient: false,
      });

      const payload = decodeJwtPayload(result.accessToken);

      const nameClaim =
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
        payload["unique_name"] ||
        `${data.name} ${data.surname}`;

      const roleClaim =
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      const isAdmin = Array.isArray(roleClaim)
        ? roleClaim.includes("Admin")
        : roleClaim === "Admin";

      const expiresAt = Date.now() + result.expireInSeconds * 1000;

      const authUser: AuthUser = {
        userId: result.userId,
        name: nameClaim as string,
        userName: data.userName,
        emailAddress: data.emailAddress,
        isAdmin,
        token: result.accessToken,
        expireInSeconds: result.expireInSeconds,
        expiresAt,
        // Merge preferredLanguage from RegisterData into AuthUser
        preferredLanguage: data.preferredLanguage,
      };

      // Write cookies — ml_user includes preferredLanguage
      setCookie(TOKEN_COOKIE, result.accessToken, result.expireInSeconds);
      const { token: _omit, ...userWithoutToken } = authUser;
      void _omit;
      setCookie(USER_COOKIE, JSON.stringify(userWithoutToken), result.expireInSeconds);

      setUser(authUser);

      // New registrations are never admin — always redirect home
      router.push(createLocalizedPath(locale, appRoutes.home));
    },
    [locale, router]
  );

  // ── signOut ─────────────────────────────────────────────────────────────

  const signOut = useCallback(() => {
    deleteCookie(TOKEN_COOKIE);
    deleteCookie(USER_COOKIE);
    setUser(null);
  }, []);

  // ── Context value ────────────────────────────────────────────────────────

  const value = useMemo<AuthContextValue>(
    () => ({ user, isLoading, signIn, register, signOut }),
    [user, isLoading, signIn, register, signOut]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
