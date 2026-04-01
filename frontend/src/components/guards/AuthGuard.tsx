"use client";

import { ReactNode } from "react";
import { useRequireAuth } from "@/hooks/useRouteGuard";
import { Spin } from "antd";

interface AuthGuardProps {
  children: ReactNode;
}

/**
 * Guard component that requires authentication.
 * Redirects to /auth with returnUrl if user is not logged in.
 * Shows loading skeleton while checking auth status.
 */
export default function AuthGuard({ children }: AuthGuardProps) {
  const { isAuthenticated, isLoading } = useRequireAuth();

  if (isLoading) {
    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "60vh",
          width: "100%",
        }}
      >
        <Spin size="large" tip="Loading..." />
      </div>
    );
  }

  if (!isAuthenticated) {
    // Redirect happening in useRequireAuth hook
    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "60vh",
          width: "100%",
        }}
      >
        <Spin size="large" tip="Redirecting..." />
      </div>
    );
  }

  return <>{children}</>;
}
