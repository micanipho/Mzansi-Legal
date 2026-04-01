import type { ReactNode } from "react";

/**
 * Auth-specific layout that removes the navbar for a clean, focused authentication experience.
 * Overrides the parent [locale]/layout.tsx for routes under /auth.
 */
export default function AuthLayout({
  children,
}: {
  children: ReactNode;
}) {
  return <>{children}</>;
}
