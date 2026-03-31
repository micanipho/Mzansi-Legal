"use client";

import { usePathname } from "next/navigation";
import AppNavbar from "@/components/layout/AppNavbar";
import MinimalHeader from "@/components/layout/MinimalHeader";

export default function ConditionalNav() {
  const pathname = usePathname();

  // Detect route type
  const isAuthPage = /^\/(en|zu|st|af)\/auth/.test(pathname);
  const isLandingPage = /^\/(en|zu|st|af)\/?$/.test(pathname);
  const shouldShowFullNavbar = !isAuthPage && !isLandingPage;
  const shouldShowMinimalHeader = isLandingPage;

  if (shouldShowFullNavbar) {
    return <AppNavbar />;
  }

  if (shouldShowMinimalHeader) {
    return <MinimalHeader />;
  }

  // Auth pages - no navigation
  return null;
}
