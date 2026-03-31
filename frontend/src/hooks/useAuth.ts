"use client";

import { useContext } from "react";
import { AuthContext, type AuthContextValue } from "@/components/providers/AuthProvider";

/**
 * Access the auth context from any client component inside AuthProvider.
 * Throws if used outside an AuthProvider tree.
 */
export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (ctx === null) {
    throw new Error("useAuth must be used within an <AuthProvider>. Ensure AuthProvider wraps the component tree.");
  }
  return ctx;
}
