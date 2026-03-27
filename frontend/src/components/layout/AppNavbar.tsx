"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useLocale } from "next-intl";
import { DownOutlined } from "@ant-design/icons";
import { Scale } from "lucide-react";
import { C, fontSerif, fontSans, shadowOrganic } from "@/styles/theme";

const NAV_LINKS = [
  { name: "Home",      path: "" },
  { name: "Ask",       path: "/chat" },
  { name: "Contracts", path: "/contracts" },
  { name: "My Rights", path: "/rights" },
  { name: "History",   path: "/history" },
];

export default function AppNavbar() {
  const locale  = useLocale();
  const pathname = usePathname();

  const isActive = (path: string) => {
    const full = `/${locale}${path}`;
    if (path === "") return pathname === full;
    return pathname.startsWith(full);
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
        style={{
          pointerEvents: "auto",
          background: "rgba(255,255,255,0.70)",
          backdropFilter: "blur(12px)",
          WebkitBackdropFilter: "blur(12px)",
          border: `1px solid rgba(222,216,207,0.5)`,
          borderRadius: 9999,
          padding: "8px 16px",
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          boxShadow: shadowOrganic,
          fontFamily: fontSans,
        }}
      >
        {/* Logo */}
        <Link
          href={`/${locale}`}
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
          <span style={{ fontFamily: fontSerif, fontWeight: 700, fontSize: 20, color: C.fg, letterSpacing: "-0.02em" }}>
            MzansiLegal
          </span>
        </Link>

        {/* Nav links */}
        <div style={{ display: "flex", alignItems: "center", gap: 4 }}>
          {NAV_LINKS.map(({ name, path }) => {
            const active = isActive(path);
            return (
              <Link
                key={name}
                href={`/${locale}${path}`}
                style={{
                  padding: "8px 16px",
                  borderRadius: 9999,
                  fontSize: 14,
                  fontWeight: 500,
                  color: active ? C.fg : C.mutedFg,
                  background: active ? C.muted : "transparent",
                  textDecoration: "none",
                }}
              >
                {name}
              </Link>
            );
          })}
        </div>

        {/* Right */}
        <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
          <button
            style={{
              display: "flex",
              alignItems: "center",
              gap: 4,
              fontSize: 14,
              fontWeight: 500,
              color: C.fg,
              background: "transparent",
              border: "none",
              cursor: "pointer",
              padding: "8px 12px",
              borderRadius: 9999,
              fontFamily: fontSans,
            }}
          >
            English <DownOutlined style={{ fontSize: 12, color: C.mutedFg }} />
          </button>
          <Link
            href={`/${locale}/chat`}
            style={{
              background: C.primary,
              color: C.primaryFg,
              padding: "10px 24px",
              borderRadius: 9999,
              fontSize: 14,
              fontWeight: 700,
              textDecoration: "none",
              fontFamily: fontSans,
            }}
          >
            Get started
          </Link>
        </div>
      </nav>
    </header>
  );
}
