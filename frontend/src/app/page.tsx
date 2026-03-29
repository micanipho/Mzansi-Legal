import { redirect } from "next/navigation";
import { createLocalizedPath, routing } from "@/i18n/routing";

// The proxy (proxy.ts) handles locale detection and redirection.
// This page is a fallback for environments where proxy is not running.
export default function RootPage() {
  redirect(createLocalizedPath(routing.defaultLocale, "/"));
}
