import createMiddleware from "next-intl/middleware";
import { NextResponse, type NextRequest } from "next/server";
import { routing } from "./i18n/routing";

const intlMiddleware = createMiddleware(routing);

/** Paths that require authentication (pattern: /[locale]/protected-path) */
const PROTECTED_PATTERNS = [
  /^\/(en|zu|st|af)\/contracts(\/.*)?$/,
  /^\/(en|zu|st|af)\/admin\/dashboard(\/.*)?$/,
];

/** Extract locale from a protected pathname match */
function getLocaleFromPath(pathname: string): string {
  const match = pathname.match(/^\/(en|zu|st|af)/);
  return match ? match[1] : routing.defaultLocale;
}

export function proxy(request: NextRequest) {
  const pathname = request.nextUrl.pathname;

  // ── Legacy /chat redirect ──────────────────────────────────────────────
  const localeMatch = pathname.match(/^\/(en|zu|st|af)\/chat\/?$/);

  if (pathname === "/chat") {
    return NextResponse.redirect(new URL(`/${routing.defaultLocale}/ask`, request.url));
  }

  if (localeMatch) {
    return NextResponse.redirect(new URL(`/${localeMatch[1]}/ask`, request.url));
  }

  // ── Route protection ───────────────────────────────────────────────────
  const isProtected = PROTECTED_PATTERNS.some((pattern) => pattern.test(pathname));

  if (isProtected) {
    const token = request.cookies.get("ml_token")?.value;
    if (!token) {
      const locale = getLocaleFromPath(pathname);
      const authUrl = new URL(`/${locale}/auth`, request.url);
      return NextResponse.redirect(authUrl);
    }
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
