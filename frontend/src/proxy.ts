import createMiddleware from "next-intl/middleware";
import { NextResponse } from "next/server";
import { routing } from "./i18n/routing";

const intlMiddleware = createMiddleware(routing);

export function proxy(request: Parameters<typeof intlMiddleware>[0]) {
  const pathname = request.nextUrl.pathname;
  const localeMatch = pathname.match(/^\/(en|zu|st|af)\/chat\/?$/);

  if (pathname === "/chat") {
    return NextResponse.redirect(new URL(`/${routing.defaultLocale}/ask`, request.url));
  }

  if (localeMatch) {
    return NextResponse.redirect(new URL(`/${localeMatch[1]}/ask`, request.url));
  }

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
