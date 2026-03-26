import { redirect } from "next/navigation";

// The proxy (proxy.ts) handles locale detection and redirection.
// This page is a fallback for environments where proxy is not running.
export default function RootPage() {
  redirect("/en");
}
