"use client";

import { ReactNode } from "react";
import { useRequireAdmin } from "@/hooks/useRouteGuard";
import { Spin } from "antd";

interface AdminGuardProps {
  children: ReactNode;
}

/**
 * Guard component that requires Admin role.
 * Redirects to home if user is not an admin.
 * Redirects to /auth if user is not logged in.
 * Shows loading state while checking.
 */
export default function AdminGuard({ children }: AdminGuardProps) {
  const { isAuthenticated, isAdmin, isLoading } = useRequireAdmin();

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

  if (!isAuthenticated || !isAdmin) {
    // Redirect happening in useRequireAdmin hook
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
