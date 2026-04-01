import { defineRouting } from "next-intl/routing";

export const routing = defineRouting({
  locales: ["en", "zu", "st", "af"],
  defaultLocale: "en",
});

export const supportedLocales = [...routing.locales];

export const appRoutes = {
  home: "",
  ask: "/ask",
  legacyChat: "/chat",
  auth: "/auth",
  contracts: "/contracts",
  contractDetailPattern: "/contracts/[id]",
  rights: "/rights",
  history: "/history",
  adminDashboard: "/admin/dashboard",
} as const;

export function isSupportedLocale(locale: string): locale is (typeof supportedLocales)[number] {
  return supportedLocales.includes(locale as (typeof supportedLocales)[number]);
}

export function stripLocaleFromPath(pathname: string): string {
  const segments = pathname.split("/").filter(Boolean);
  if (segments.length === 0) {
    return "/";
  }

  if (isSupportedLocale(segments[0])) {
    const remainder = segments.slice(1).join("/");
    return remainder ? `/${remainder}` : "/";
  }

  return pathname || "/";
}

export function normalizeShellPath(pathname: string): string {
  const strippedPath = stripLocaleFromPath(pathname);
  if (strippedPath === appRoutes.legacyChat || strippedPath.startsWith(`${appRoutes.legacyChat}/`)) {
    return strippedPath.replace(appRoutes.legacyChat, appRoutes.ask);
  }

  return strippedPath;
}

export function createLocalizedPath(locale: string, pathname: string, queryString?: string): string {
  const canonicalPath = normalizeShellPath(pathname);
  const path = canonicalPath === "/" ? `/${locale}` : `/${locale}${canonicalPath}`;
  if (!queryString) {
    return path;
  }

  return `${path}?${queryString}`;
}

export function getLocalizedRouteForPathname(
  pathname: string,
  currentLocale: string,
  nextLocale: string,
  queryString?: string,
): string {
  const normalizedPath = pathname.startsWith(`/${currentLocale}`)
    ? pathname
    : createLocalizedPath(currentLocale, pathname, queryString);

  return createLocalizedPath(nextLocale, normalizedPath, queryString);
}
