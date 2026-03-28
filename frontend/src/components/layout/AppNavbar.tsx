"use client";

import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { Scale } from "lucide-react";
import { startTransition } from "react";
import {
  appRoutes,
  createLocalizedPath,
  supportedLocales,
} from "@/i18n/routing";
import { buildLocaleSwitchHref } from "@/i18n/localeSwitch";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

const NAV_LINKS = [
  { labelKey: "home", path: appRoutes.home },
  { labelKey: "ask", path: appRoutes.ask },
  { labelKey: "contracts", path: appRoutes.contracts },
  { labelKey: "rights", path: appRoutes.rights },
  { labelKey: "dashboard", path: appRoutes.adminDashboard },
] as const;

export default function AppNavbar() {
  const locale = useLocale();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const router = useRouter();
  const tNav = useTranslations("nav");
  const tCommon = useTranslations("common");

  const isActive = (path: string) => {
    const full = createLocalizedPath(locale, path);
    if (path === "") {
      return pathname === full;
    }

    if (path === appRoutes.ask) {
      return pathname === full || pathname === createLocalizedPath(locale, appRoutes.legacyChat);
    }

    return pathname.startsWith(full);
  };

  const handleLocaleChange = (nextLocale: string) => {
    const nextHref = buildLocaleSwitchHref({
      pathname,
      currentLocale: locale,
      nextLocale,
      searchParams,
    });
    startTransition(() => {
      router.push(nextHref);
    });
  };

  return (
    <header
      style={{
        position: "sticky",
        top: 16,
        zIndex: 50,
        padding: "0 16px",
        maxWidth: 1280,
        margin: "0 auto",
        width: "100%",
        pointerEvents: "none",
      }}
    >
      <nav
        className="app-navbar-shell grain-panel"
        style={{
          pointerEvents: "auto",
          background: "rgba(255,255,255,0.72)",
          backdropFilter: "blur(12px)",
          WebkitBackdropFilter: "blur(12px)",
          border: "1px solid rgba(222,216,207,0.5)",
          borderRadius: 9999,
          padding: "10px 16px",
          boxShadow: shadowOrganic,
          fontFamily: fontSans,
        }}
      >
        <Link
          href={createLocalizedPath(locale, appRoutes.home)}
          style={{ display: "flex", alignItems: "center", gap: 12, textDecoration: "none" }}
        >
          <div
            style={{
              width: 40,
              height: 40,
              background: C.primary,
              borderRadius: 9999,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: C.primaryFg,
              flexShrink: 0,
            }}
          >
            <Scale size={20} color={C.primaryFg} />
          </div>
          <span
            style={{
              fontFamily: fontSerif,
              fontWeight: 700,
              fontSize: 20,
              color: C.fg,
              letterSpacing: "-0.02em",
            }}
          >
            MzansiLegal
          </span>
        </Link>

        <div className="app-navbar-links">
          {NAV_LINKS.map(({ labelKey, path }) => {
            const active = isActive(path);
            return (
              <Link
                key={labelKey}
                href={createLocalizedPath(locale, path)}
                className="app-navbar-link"
                aria-current={active ? "page" : undefined}
                style={{
                  padding: "8px 16px",
                  borderRadius: 9999,
                  fontSize: 14,
                  fontWeight: 600,
                  color: active ? C.fg : C.mutedFg,
                  background: active ? C.muted : "transparent",
                  textDecoration: "none",
                  whiteSpace: "nowrap",
                }}
              >
                {tNav(labelKey)}
              </Link>
            );
          })}
        </div>

        <div className="app-navbar-actions">
          <label style={{ display: "flex", minWidth: 0 }}>
            <select
              className="app-navbar-locale"
              value={locale}
              onChange={(event) => handleLocaleChange(event.target.value)}
              aria-label={tNav("language")}
            >
              {supportedLocales.map((supportedLocale) => (
                <option key={supportedLocale} value={supportedLocale}>
                  {tCommon(`locales.${supportedLocale}`)}
                </option>
              ))}
            </select>
          </label>

          <Link
            href={createLocalizedPath(locale, appRoutes.ask)}
            className="app-navbar-link"
            style={{
              background: C.primary,
              color: C.primaryFg,
              padding: "10px 24px",
              borderRadius: 9999,
              fontSize: 14,
              fontWeight: 700,
              textDecoration: "none",
              fontFamily: fontSans,
              whiteSpace: "nowrap",
            }}
          >
            {tNav("getStarted")}
          </Link>
        </div>
      </nav>
    </header>
  );
}
