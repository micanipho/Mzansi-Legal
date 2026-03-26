import createMiddleware from "next-intl/middleware";
import { routing } from "./i18n/routing";

const intlMiddleware = createMiddleware(routing);

export function proxy(request: Parameters<typeof intlMiddleware>[0]) {
  return intlMiddleware(request);
}

export const config = {
  matcher: [
    // Match root
    "/",
    // Match locale-prefixed paths
    "/(en|zu|st|af)/:path*",
    // Match all paths except Next.js internals and static files
    "/((?!_next|_vercel|api|.*\\..*).*)",
  ],
};
