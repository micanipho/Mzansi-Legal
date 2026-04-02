"use client";

import type { CSSProperties } from "react";
import { startTransition, useEffect, useRef, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { ChevronDown, Globe } from "lucide-react";
import { buildLocaleSwitchHref } from "@/i18n/localeSwitch";
import { supportedLocales } from "@/i18n/routing";
import { C, fontSans, shadowOrganic } from "@/styles/theme";

type LocaleSwitcherProps = {
  align?: "left" | "right";
  buttonStyle?: CSSProperties;
  menuStyle?: CSSProperties;
  itemStyle?: CSSProperties;
};

export default function LocaleSwitcher({
  align = "right",
  buttonStyle,
  menuStyle,
  itemStyle,
}: LocaleSwitcherProps) {
  const locale = useLocale();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const router = useRouter();
  const tCommon = useTranslations("common");
  const tNav = useTranslations("nav");

  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        setOpen(false);
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
      setOpen(false);
    });
  };

  return (
    <div ref={ref} style={{ position: "relative" }}>
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        aria-label={tNav("language")}
        aria-haspopup="listbox"
        aria-expanded={open}
        style={{
          display: "inline-flex",
          alignItems: "center",
          gap: 8,
          padding: "8px 14px",
          background: "rgba(255,255,255,0.7)",
          border: `1px solid ${C.border}`,
          borderRadius: 9999,
          cursor: "pointer",
          fontSize: 13,
          fontWeight: 700,
          color: C.fg,
          fontFamily: fontSans,
          ...buttonStyle,
        }}
      >
        <Globe size={15} />
        <span>{tCommon(`locales.${locale}`)}</span>
        <ChevronDown size={12} style={{ opacity: 0.7, transform: open ? "rotate(180deg)" : "none", transition: "transform 0.2s ease" }} />
      </button>

      {open ? (
        <div
          role="listbox"
          aria-label={tNav("language")}
          style={{
            position: "absolute",
            top: "calc(100% + 8px)",
            [align]: 0,
            minWidth: 160,
            padding: 6,
            borderRadius: 16,
            border: `1px solid ${C.border}`,
            background: "#fff",
            boxShadow: shadowOrganic,
            zIndex: 100,
            ...menuStyle,
          }}
        >
          {supportedLocales.map((nextLocale) => (
            <button
              key={nextLocale}
              type="button"
              role="option"
              aria-selected={nextLocale === locale}
              onClick={() => handleLocaleChange(nextLocale)}
              style={{
                width: "100%",
                display: "block",
                textAlign: "left",
                padding: "9px 12px",
                border: "none",
                borderRadius: 10,
                cursor: "pointer",
                fontSize: 13,
                fontWeight: nextLocale === locale ? 700 : 500,
                color: nextLocale === locale ? C.primary : C.fg,
                background: nextLocale === locale ? C.muted : "transparent",
                fontFamily: fontSans,
                ...itemStyle,
              }}
              onMouseEnter={(event) => {
                if (nextLocale !== locale) {
                  event.currentTarget.style.background = C.muted;
                }
              }}
              onMouseLeave={(event) => {
                if (nextLocale !== locale) {
                  event.currentTarget.style.background = "transparent";
                }
              }}
            >
              {tCommon(`locales.${nextLocale}`)}
            </button>
          ))}
        </div>
      ) : null}
    </div>
  );
}
