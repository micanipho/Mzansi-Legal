"use client";

import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { Scale, Globe } from "lucide-react";
import { startTransition, useState, useRef, useEffect } from "react";
import { appRoutes, createLocalizedPath, supportedLocales } from "@/i18n/routing";
import { buildLocaleSwitchHref } from "@/i18n/localeSwitch";
import { C, fontSans, fontSerif } from "@/styles/theme";

const LOCALE_NAMES: Record<string, string> = {
  en: "English",
  zu: "isiZulu",
  st: "Sesotho",
  af: "Afrikaans",
};

export default function MinimalHeader() {
  const locale = useLocale();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const router = useRouter();
  const tCommon = useTranslations("common");

  const [langDropdownOpen, setLangDropdownOpen] = useState(false);
  const langDropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (langDropdownRef.current && !langDropdownRef.current.contains(e.target as Node)) {
        setLangDropdownOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleLocaleChange = (nextLocale: string) => {
    const nextHref = buildLocaleSwitchHref({
      pathname,
      currentLocale: locale,
      nextLocale,
      searchParams,
    });
    startTransition(() => {
      router.push(nextHref);
      setLangDropdownOpen(false);
    });
  };

  return (
    <header
      style={{
        position: "sticky",
        top: 0,
        zIndex: 50,
        background: "rgba(255,255,255,0.85)",
        backdropFilter: "blur(12px)",
        WebkitBackdropFilter: "blur(12px)",
        borderBottom: `1px solid ${C.border}`,
        fontFamily: fontSans,
      }}
    >
      <nav
        style={{
          maxWidth: 1280,
          margin: "0 auto",
          padding: "16px 24px",
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: 24,
        }}
      >
        {/* Logo */}
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

        {/* Right side: Language selector + Sign In */}
        <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
          {/* Language Selector */}
          <div style={{ position: "relative" }} ref={langDropdownRef}>
            <button
              onClick={() => setLangDropdownOpen(!langDropdownOpen)}
              style={{
                display: "flex",
                alignItems: "center",
                gap: 8,
                padding: "8px 14px",
                background: C.muted,
                border: `1px solid ${C.border}`,
                borderRadius: 12,
                cursor: "pointer",
                fontSize: 14,
                fontWeight: 500,
                color: C.fg,
                fontFamily: fontSans,
              }}
              aria-label="Select language"
              aria-expanded={langDropdownOpen}
            >
              <Globe size={16} />
              <span>{LOCALE_NAMES[locale] || locale.toUpperCase()}</span>
            </button>

            {langDropdownOpen && (
              <div
                style={{
                  position: "absolute",
                  top: "calc(100% + 8px)",
                  right: 0,
                  background: "#fff",
                  border: `1px solid ${C.border}`,
                  borderRadius: 12,
                  boxShadow: "0 4px 16px rgba(0,0,0,0.1)",
                  minWidth: 160,
                  padding: 8,
                  zIndex: 100,
                }}
              >
                {supportedLocales.map((loc) => (
                  <button
                    key={loc}
                    onClick={() => handleLocaleChange(loc)}
                    style={{
                      width: "100%",
                      textAlign: "left",
                      padding: "10px 12px",
                      background: locale === loc ? C.muted : "transparent",
                      border: "none",
                      borderRadius: 8,
                      cursor: "pointer",
                      fontSize: 14,
                      fontWeight: locale === loc ? 600 : 400,
                      color: C.fg,
                      fontFamily: fontSans,
                      transition: "background 0.2s",
                    }}
                    onMouseEnter={(e) => {
                      if (locale !== loc) {
                        e.currentTarget.style.background = C.muted;
                      }
                    }}
                    onMouseLeave={(e) => {
                      if (locale !== loc) {
                        e.currentTarget.style.background = "transparent";
                      }
                    }}
                  >
                    {LOCALE_NAMES[loc]}
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Sign In Button */}
          <Link
            href={createLocalizedPath(locale, appRoutes.auth)}
            style={{
              display: "inline-block",
              padding: "10px 24px",
              background: C.primary,
              color: C.primaryFg,
              borderRadius: 12,
              fontWeight: 600,
              fontSize: 14,
              textDecoration: "none",
              fontFamily: fontSans,
              transition: "transform 0.2s, box-shadow 0.2s",
              boxShadow: "0 2px 8px rgba(74,90,58,0.2)",
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = "translateY(-1px)";
              e.currentTarget.style.boxShadow = "0 4px 12px rgba(74,90,58,0.3)";
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = "translateY(0)";
              e.currentTarget.style.boxShadow = "0 2px 8px rgba(74,90,58,0.2)";
            }}
          >
            {tCommon("signIn")}
          </Link>
        </div>
      </nav>
    </header>
  );
}
