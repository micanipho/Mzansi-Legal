"use client";

import Link from "next/link";
import { useLocale, useTranslations } from "next-intl";
import { Scale } from "lucide-react";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import LocaleSwitcher from "@/components/layout/LocaleSwitcher";
import { C, fontSans, fontSerif } from "@/styles/theme";

export default function MinimalHeader() {
  const locale = useLocale();
  const tCommon = useTranslations("common");

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
          <LocaleSwitcher buttonStyle={{ background: C.muted, borderRadius: 12, fontSize: 14, fontWeight: 500 }} menuStyle={{ borderRadius: 12 }} itemStyle={{ fontSize: 14 }} />

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
