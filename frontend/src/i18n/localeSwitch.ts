import type { ReadonlyURLSearchParams } from "next/navigation";
import { getLocalizedRouteForPathname } from "@/i18n/routing";

interface LocaleSwitchHrefOptions {
  pathname: string;
  currentLocale: string;
  nextLocale: string;
  searchParams?: ReadonlyURLSearchParams | URLSearchParams | null;
}

function toSafeQueryString(searchParams?: ReadonlyURLSearchParams | URLSearchParams | null): string | undefined {
  if (!searchParams) {
    return undefined;
  }

  const query = new URLSearchParams();
  searchParams.forEach((value, key) => {
    if (value) {
      query.append(key, value);
    }
  });

  const serialized = query.toString();
  return serialized || undefined;
}

export function buildLocaleSwitchHref({
  pathname,
  currentLocale,
  nextLocale,
  searchParams,
}: LocaleSwitchHrefOptions): string {
  return getLocalizedRouteForPathname(
    pathname,
    currentLocale,
    nextLocale,
    toSafeQueryString(searchParams),
  );
}
