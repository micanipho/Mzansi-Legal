"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { useState } from "react";
import { Search, FileText, MessageCircle, TrendingUp } from "lucide-react";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, R, shadowOrganic, fontSerif, fontSans } from "@/styles/theme";
import { useAuth } from "@/hooks/useAuth";



const TRENDING: Record<string, { q: string; catKey: string }[]> = {
  en: [
    { q: "Can my employer force me to work on public holidays?",      catKey: "employment" },
    { q: "What is the maximum interest rate a mashonisa can charge?", catKey: "debtCredit" },
    { q: "Can my landlord withhold my deposit?",                      catKey: "housing"    },
    { q: "How do I apply for a protection order against harassment?", catKey: "safety"     },
    { q: "What are my rights if I am retrenched?",                    catKey: "employment" },
  ],
  zu: [
    { q: "Ingabe umqashi wami angangiphoqa ukusebenza ezinsukwini zomphakathi?", catKey: "employment" },
    { q: "Ngingayibuyisela yini imoto engiyithenge isinamaphutha?",              catKey: "consumer"   },
    { q: "Ingabe umnikazi wendlu angabamba idipozithi yami?",                    catKey: "housing"    },
    { q: "Ngifaka kanjani isicelo sokuphephela ekuhlukunyezweni?",               catKey: "safety"     },
    { q: "Yimaphi amalungelo ami uma ngixoshiwe emsebenzini?",                   catKey: "employment" },
  ],
  st: [
    { q: "Na mophatlalatsi wa ka a ka nkgatelela ho sebetsa matsatsi a boikhohomoso?", catKey: "employment" },
    { q: "Nka etsa eng ha landlord e sa batle ho kgutlisa deposit ya ka?",             catKey: "housing"    },
    { q: "Ke bokae bo phahameng ba tswala ba ya ho lefshwa ke mashonisa?",             catKey: "debtCredit" },
    { q: "Ke etsa jwang kopo ya taelo ya tshireletso?",                                catKey: "safety"     },
    { q: "Ke na le ditokelo tse feng ha ke ntshwa mosebetsing?",                       catKey: "employment" },
  ],
  af: [
    { q: "Kan my werkgewer my dwing om op openbare vakansiedae te werk?",    catKey: "employment" },
    { q: "Wat is die maksimum rentekoers wat 'n mashonisa kan hef?",         catKey: "debtCredit" },
    { q: "Kan my verhuurder my deposito weerhou?",                           catKey: "housing"    },
    { q: "Hoe dien ek aansoek in vir 'n beskermingsbevel teen teistering?",  catKey: "safety"     },
    { q: "Wat is my regte as ek afgedank word?",                             catKey: "employment" },
  ],
};

export default function HomePage() {
  const router = useRouter();
  const locale = useLocale();
  const t  = useTranslations("home");
  const tc = useTranslations("categories");
  const [query, setQuery] = useState("");
  const { user } = useAuth();

  const ask = (q?: string) => {
    const text = (q ?? query).trim();
    if (!text) return;
    router.push(createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(text)}`));
  };

  return (
    <main
      className="page-shell"
      style={{
        display: "flex",
        flexDirection: "column",
        gap: 64,
        fontFamily: fontSans,
      }}
    >
      {/* Hero Section */}
      <section style={{ display: "flex", flexDirection: "column", alignItems: "center", textAlign: "center", maxWidth: 720, margin: "0 auto", marginTop: 48 }}>
        <h1
          style={{
            fontFamily: fontSerif,
            fontSize: "clamp(2.5rem, 6vw, 4rem)",
            fontWeight: 800,
            color: C.primary,
            lineHeight: 1.1,
            marginBottom: 16,
            letterSpacing: "-0.02em",
          }}
        >
          {t("title")}
          <br />
          <span style={{ color: C.fg }}>{t("heroSubtitle")}</span>
        </h1>

        <p style={{ fontSize: 17, color: C.mutedFg, marginBottom: 40, maxWidth: 560 }}>
          {t("heroDesc")}
        </p>

        {/* Search bar */}
        <div style={{ width: "100%", maxWidth: 600, position: "relative", display: "flex", alignItems: "center", marginBottom: 32 }}>
          <div style={{ position: "absolute", left: 20, color: C.mutedFg }}>
            <Search size={20} />
          </div>
          <input
            type="text"
            placeholder={t("askPlaceholder")}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && ask()}
            style={{
              width: "100%",
              background: C.card,
              border: `2px solid ${C.border}`,
              borderRadius: 16,
              padding: "16px 120px 16px 52px",
              fontSize: 16,
              outline: "none",
              fontFamily: fontSans,
              color: C.fg,
            }}
            aria-label="Search legal questions"
          />
          <button
            onClick={() => ask()}
            disabled={!query.trim()}
            style={{
              position: "absolute",
              right: 8,
              background: C.primary,
              color: C.primaryFg,
              padding: "10px 24px",
              borderRadius: 12,
              fontWeight: 700,
              fontSize: 14,
              border: "none",
              cursor: query.trim() ? "pointer" : "not-allowed",
              fontFamily: fontSans,
              opacity: query.trim() ? 1 : 0.5,
            }}
          >
            {t("askButton")}
          </button>
        </div>

        {/* Quick links */}
        <div style={{ display: "flex", flexWrap: "wrap", justifyContent: "center", gap: 12, fontSize: 14, marginBottom: 16 }}>
          {[
            { text: t("quickLink1"), href: createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(t("quickLink1"))}`) },
            { text: t("quickLink2"), href: createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(t("quickLink2"))}`) },
          ].map(({ text, href }) => (
            <Link
              key={text}
              href={href}
              style={{
                color: C.mutedFg,
                textDecoration: "none",
                padding: "6px 14px",
                borderRadius: 8,
                background: C.muted,
                fontSize: 13,
                fontWeight: 500,
                transition: "background 0.2s",
              }}
            >
              {text}
            </Link>
          ))}
        </div>
      </section>

      {/* Main CTAs */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(300px, 1fr))", gap: 20, maxWidth: 900, margin: "0 auto", width: "100%" }}>
        <Link
          href={createLocalizedPath(locale, appRoutes.ask)}
          style={{
            background: C.primary,
            color: C.primaryFg,
            padding: 32,
            borderRadius: 20,
            boxShadow: `0 4px 16px ${C.primary}33`,
            display: "flex",
            flexDirection: "column",
            gap: 12,
            textDecoration: "none",
            transition: "transform 0.2s, box-shadow 0.2s",
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = "translateY(-4px)";
            e.currentTarget.style.boxShadow = `0 8px 24px ${C.primary}44`;
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = "translateY(0)";
            e.currentTarget.style.boxShadow = `0 4px 16px ${C.primary}33`;
          }}
        >
          <MessageCircle size={32} />
          <div>
            <h3 style={{ fontFamily: fontSerif, fontSize: 22, fontWeight: 700, margin: "0 0 8px" }}>{t("ctaAskTitle")}</h3>
            <p style={{ margin: 0, opacity: 0.9, fontSize: 15 }}>{t("ctaAskDesc")}</p>
          </div>
        </Link>

        <Link
          href={createLocalizedPath(locale, appRoutes.contracts)}
          style={{
            background: C.card,
            border: `1px solid ${C.border}`,
            padding: 32,
            borderRadius: 20,
            boxShadow: shadowOrganic,
            display: "flex",
            flexDirection: "column",
            gap: 12,
            textDecoration: "none",
            transition: "transform 0.2s, box-shadow 0.2s",
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = "translateY(-4px)";
            e.currentTarget.style.boxShadow = "0 8px 24px rgba(0,0,0,0.1)";
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = "translateY(0)";
            e.currentTarget.style.boxShadow = shadowOrganic;
          }}
        >
          <FileText size={32} color={C.primary} />
          <div>
            <h3 style={{ fontFamily: fontSerif, fontSize: 22, fontWeight: 700, color: C.fg, margin: "0 0 8px" }}>{t("ctaContractTitle")}</h3>
            <p style={{ color: C.mutedFg, margin: 0, fontSize: 15 }}>{t("ctaContractDesc")}</p>
          </div>
        </Link>
      </section>

      {/* Trending Questions */}
      <section
        style={{
          background: C.card,
          border: `1px solid ${C.border}`,
          borderRadius: 20,
          padding: 32,
          boxShadow: shadowOrganic,
          maxWidth: 900,
          margin: "0 auto",
          width: "100%",
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 20 }}>
          <TrendingUp size={20} color={C.secondary} />
          <h2 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: 0 }}>
            {t("trendingHeader")}
          </h2>
        </div>
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          {(TRENDING[locale] ?? TRENDING.en).slice(0, 3).map(({ q }, i) => (
            <button
              key={i}
              onClick={() => ask(q)}
              style={{
                display: "flex",
                alignItems: "center",
                gap: 14,
                padding: 16,
                background: C.muted,
                border: "none",
                borderRadius: 12,
                cursor: "pointer",
                textAlign: "left",
                width: "100%",
                transition: "background 0.2s",
              }}
              onMouseEnter={(e) => e.currentTarget.style.background = C.border}
              onMouseLeave={(e) => e.currentTarget.style.background = C.muted}
            >
              <span style={{ fontSize: 14, fontWeight: 700, color: C.primary, minWidth: 24 }}>
                {i + 1}.
              </span>
              <p style={{ flex: 1, fontSize: 15, fontWeight: 500, color: C.fg, margin: 0, lineHeight: 1.4 }}>{q}</p>
            </button>
          ))}
        </div>
      </section>
    </main>
  );
}
