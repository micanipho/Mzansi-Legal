"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useLocale } from "next-intl";
import { useAuth } from "./useAuth";
import type { AuthUser } from "@/components/providers/AuthProvider";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";

export interface RouteGuardReturn {
  isAuthenticated: boolean;
  isAdmin: boolean;
  isLoading: boolean;
  user: AuthUser | null;
}

/**
 * Hook for route guarding logic.
 * Returns auth state and provides redirect functionality.
 */
export function useRouteGuard(): RouteGuardReturn {
  const { user, isLoading } = useAuth();
  const isAuthenticated = !!user && !isLoading;
  const isAdmin = !!user?.isAdmin;

  return {
    isAuthenticated,
    isAdmin,
    isLoading,
    user,
  };
}

/**
 * Hook that redirects unauthenticated users to the auth page.
 * Preserves returnUrl for post-login redirect.
 */
export function useRequireAuth() {
  const router = useRouter();
  const pathname = usePathname();
  const locale = useLocale();
  const { isAuthenticated, isLoading } = useRouteGuard();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      const returnUrl = encodeURIComponent(pathname);
      const authPath = createLocalizedPath(locale, appRoutes.auth);
      router.push(`${authPath}?returnUrl=${returnUrl}`);
    }
  }, [isAuthenticated, isLoading, router, pathname, locale]);

  return { isAuthenticated, isLoading };
}

/**
 * Hook that redirects non-admin users to home.
 * Requires user to be authenticated first.
 */
export function useRequireAdmin() {
  const router = useRouter();
  const locale = useLocale();
  const { isAuthenticated, isAdmin, isLoading } = useRouteGuard();

  useEffect(() => {
    if (!isLoading) {
      if (!isAuthenticated) {
        const authPath = createLocalizedPath(locale, appRoutes.auth);
        router.push(authPath);
      } else if (!isAdmin) {
        const homePath = createLocalizedPath(locale, "");
        router.push(homePath);
      }
    }
  }, [isAuthenticated, isAdmin, isLoading, router, locale]);

  return { isAuthenticated, isAdmin, isLoading };
}
