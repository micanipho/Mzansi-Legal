"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { useState } from "react";
import {
  Search, Mic, FileText, MessageCircle, TrendingUp,
  Shield, Home as HomeIcon, Briefcase, CreditCard,
  Calculator, Lock, AlertTriangle,
} from "lucide-react";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, R, shadowOrganic, fontSerif, fontSans } from "@/styles/theme";

const STATS = [
  { num: "2,847", labelKey: "statQuestionsAnswered" as const, r: R.o1 },
  { num: "13",    labelKey: "statActsIndexed"        as const, r: R.o2 },
  { num: "4",     labelKey: "statLanguages"           as const, r: R.o3 },
  { num: "342",   labelKey: "statContractsAnalysed"   as const, r: R.o4 },
];

const CATEGORIES = [
  { Icon: Briefcase,     titleKey: "catEmploymentTitle" as const, descKey: "catEmploymentDesc" as const, tagKey: "legal"     as const, tagColor: `rgba(93,112,82,0.12)`,   tagText: C.primary,   r: R.o1 },
  { Icon: HomeIcon,      titleKey: "catHousingTitle"    as const, descKey: "catHousingDesc"    as const, tagKey: "legal"     as const, tagColor: `rgba(93,112,82,0.12)`,   tagText: C.primary,   r: R.o2 },
  { Icon: Shield,        titleKey: "catConsumerTitle"   as const, descKey: "catConsumerDesc"   as const, tagKey: "legal"     as const, tagColor: `rgba(93,112,82,0.12)`,   tagText: C.primary,   r: R.o3 },
  { Icon: CreditCard,    titleKey: "catDebtTitle"       as const, descKey: "catDebtDesc"       as const, tagKey: "financial" as const, tagColor: `rgba(193,140,93,0.12)`,  tagText: C.secondary, r: R.o4 },
  { Icon: Calculator,    titleKey: "catTaxTitle"        as const, descKey: "catTaxDesc"        as const, tagKey: "financial" as const, tagColor: `rgba(193,140,93,0.12)`,  tagText: C.secondary, r: R.o1 },
  { Icon: Lock,          titleKey: "catPrivacyTitle"    as const, descKey: "catPrivacyDesc"    as const, tagKey: "legal"     as const, tagColor: `rgba(93,112,82,0.12)`,   tagText: C.primary,   r: R.o2 },
  { Icon: FileText,      titleKey: "catContractTitle"   as const, descKey: "catContractDesc"   as const, tagKey: "contracts" as const, tagColor: `rgba(230,220,205,0.55)`,   tagText: C.secondary, r: R.o3 },
  { Icon: TrendingUp,    titleKey: "catInsuranceTitle"  as const, descKey: "catInsuranceDesc"  as const, tagKey: "financial" as const, tagColor: `rgba(193,140,93,0.12)`,  tagText: C.secondary, r: R.o4 },
  { Icon: AlertTriangle, titleKey: "catSafetyTitle"     as const, descKey: "catSafetyDesc"     as const, tagKey: "legal"     as const, tagColor: `rgba(93,112,82,0.12)`,   tagText: C.primary,   r: R.o1 },
];

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
        gap: 96,
        fontFamily: fontSans,
      }}
    >
      {/* ── Hero ─────────────────────────────────────────────── */}
      <section style={{ display: "flex", flexDirection: "column", alignItems: "center", textAlign: "center", maxWidth: 768, margin: "0 auto", marginTop: 32 }}>
        <div
          style={{
            display: "inline-flex",
            alignItems: "center",
            gap: 8,
            background: C.muted,
            padding: "6px 16px",
            borderRadius: 9999,
            marginBottom: 32,
            border: `1px solid rgba(222,216,207,0.5)`,
          }}
        >
          <span style={{ width: 8, height: 8, borderRadius: "50%", background: C.primary, display: "inline-block" }} />
          <span style={{ fontSize: 14, fontWeight: 500, color: C.fg }}>
            {t("heroBadge")}
          </span>
        </div>

        <h1
          style={{
            fontFamily: fontSerif,
            fontSize: "clamp(3rem, 7vw, 4.5rem)",
            fontWeight: 800,
            color: C.primary,
            lineHeight: 1.1,
            marginBottom: 24,
            letterSpacing: "-0.02em",
          }}
        >
          {t("title")}
          <br />
          <span style={{ color: C.fg }}>{t("heroSubtitle")}</span>
        </h1>

        <p style={{ fontSize: 18, color: C.mutedFg, marginBottom: 40, maxWidth: 600 }}>
          {t("heroDesc")}
        </p>

        {/* Search bar */}
        <div style={{ width: "100%", maxWidth: 640, position: "relative", display: "flex", alignItems: "center", marginBottom: 24 }}>
          <div style={{ position: "absolute", left: 24, color: C.mutedFg, display: "flex" }}>
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
              background: "#fff",
              border: `2px solid ${C.border}`,
              borderRadius: 9999,
              padding: "16px 160px 16px 56px",
              fontSize: 18,
              outline: "none",
              fontFamily: fontSans,
              color: C.fg,
            }}
          />
          <div style={{ position: "absolute", right: 8, display: "flex", alignItems: "center", gap: 8 }}>
            <button
              onClick={() => router.push(createLocalizedPath(locale, appRoutes.ask))}
              style={{
                width: 40, height: 40,
                borderRadius: 9999,
                background: C.muted,
                border: "none",
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: C.primary,
              }}
              aria-label="Voice search"
            >
              <Mic size={20} />
            </button>
            <button
              onClick={() => ask()}
              style={{
                background: C.primary,
                color: C.primaryFg,
                padding: "12px 24px",
                borderRadius: 9999,
                fontWeight: 700,
                fontSize: 14,
                border: "none",
                cursor: "pointer",
                fontFamily: fontSans,
              }}
            >
              {t("askButton")}
            </button>
          </div>
        </div>

        <div style={{ display: "flex", flexWrap: "wrap", justifyContent: "center", gap: 16, fontSize: 14 }}>
          {[
            { text: t("quickLink1"), href: createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(t("quickLink1"))}`) },
            { text: t("quickLink2"), href: createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(t("quickLink2"))}`) },
            { text: t("quickLink3"), href: createLocalizedPath(locale, appRoutes.contracts) },
          ].map(({ text, href }) => (
            <Link
              key={text}
              href={href}
              style={{
                color: C.mutedFg,
                borderBottom: `1px dotted ${C.mutedFg}`,
                textDecoration: "none",
              }}
            >
              {text}
            </Link>
          ))}
        </div>
      </section>

      {/* ── Stats ────────────────────────────────────────────── */}
      <section
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(2, 1fr)",
          gap: 16,
        }}
        className="md-grid-4"
      >
        {STATS.map(({ num, labelKey, r }) => (
          <div
            key={labelKey}
            style={{
              background: C.card,
              border: `1px solid ${C.border}`,
              padding: 24,
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              textAlign: "center",
              borderRadius: r,
              boxShadow: shadowOrganic,
            }}
          >
            <span style={{ fontSize: 36, fontFamily: fontSerif, fontWeight: 700, color: C.primary, marginBottom: 8, display: "block" }}>
              {num}
            </span>
            <span style={{ fontSize: 14, fontWeight: 500, color: C.mutedFg }}>
              {t(labelKey)}
            </span>
          </div>
        ))}
      </section>

      {/* ── CTAs ─────────────────────────────────────────────── */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: 24 }}>
        <Link
          href={createLocalizedPath(locale, appRoutes.contracts)}
          style={{
            background: C.card,
            border: `2px solid ${C.accent}`,
            padding: 32,
            borderRadius: R.o2,
            boxShadow: shadowOrganic,
            display: "flex",
            flexDirection: "column",
            gap: 16,
            textDecoration: "none",
          }}
        >
          <div style={{ width: 48, height: 48, background: C.accent, borderRadius: 9999, display: "flex", alignItems: "center", justifyContent: "center", color: C.secondary }}>
            <FileText size={24} />
          </div>
          <div>
            <h3 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: "0 0 8px" }}>{t("ctaContractTitle")}</h3>
            <p style={{ color: C.mutedFg, margin: 0 }}>{t("ctaContractDesc")}</p>
          </div>
        </Link>

        <Link
          href={createLocalizedPath(locale, appRoutes.ask)}
          style={{
            background: C.card,
            border: `1px solid ${C.border}`,
            padding: 32,
            borderRadius: R.o1,
            boxShadow: shadowOrganic,
            display: "flex",
            flexDirection: "column",
            gap: 16,
            textDecoration: "none",
          }}
        >
          <div style={{ width: 48, height: 48, background: C.muted, borderRadius: 9999, display: "flex", alignItems: "center", justifyContent: "center", color: C.primary }}>
            <MessageCircle size={24} />
          </div>
          <div>
            <h3 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: "0 0 8px" }}>{t("ctaAskTitle")}</h3>
            <p style={{ color: C.mutedFg, margin: 0 }}>{t("ctaAskDesc")}</p>
          </div>
        </Link>
      </section>

      {/* ── Browse by category ───────────────────────────────── */}
      <section>
        <h2 style={{ fontFamily: fontSerif, fontSize: 30, fontWeight: 700, color: C.fg, marginBottom: 32 }}>
          {t("categoriesTitle")}
        </h2>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))", gap: 24 }}>
          {CATEGORIES.map(({ Icon, titleKey, descKey, tagKey, tagColor, tagText, r }) => (
            <Link
              key={titleKey}
              href={createLocalizedPath(locale, appRoutes.rights)}
              style={{
                background: C.card,
                border: `1px solid ${C.border}`,
                padding: 24,
                display: "flex",
                flexDirection: "column",
                alignItems: "flex-start",
                gap: 16,
                textDecoration: "none",
                borderRadius: r,
                boxShadow: "0 1px 4px rgba(0,0,0,0.04)",
              }}
            >
              <div style={{ display: "flex", justifyContent: "space-between", width: "100%", alignItems: "flex-start" }}>
                <div style={{ padding: 12, borderRadius: 9999, background: tagColor, color: tagText }}>
                  <Icon size={20} />
                </div>
                <span style={{ fontSize: 11, fontWeight: 700, padding: "4px 12px", borderRadius: 9999, background: tagColor, color: tagText }}>
                  {tc(tagKey)}
                </span>
              </div>
              <div>
                <h3 style={{ fontSize: 17, fontWeight: 700, color: C.fg, margin: "0 0 4px", fontFamily: fontSans }}>{t(titleKey)}</h3>
                <p style={{ fontSize: 14, color: C.mutedFg, margin: 0 }}>{t(descKey)}</p>
              </div>
            </Link>
          ))}
        </div>
      </section>

      {/* ── Trending ─────────────────────────────────────────── */}
      <section
        style={{
          background: C.card,
          border: `1px solid ${C.border}`,
          borderRadius: 24,
          padding: "32px 40px",
          boxShadow: shadowOrganic,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 24 }}>
          <TrendingUp size={22} color={C.secondary} />
          <h2 style={{ fontFamily: fontSerif, fontSize: 26, fontWeight: 700, color: C.fg, margin: 0 }}>
            {t("trendingHeader")}
          </h2>
        </div>
        <div style={{ display: "flex", flexDirection: "column" }}>
          {(TRENDING[locale] ?? TRENDING.en).map(({ q, catKey }, i) => (
            <div
              key={i}
              style={{ borderBottom: i < (TRENDING[locale] ?? TRENDING.en).length - 1 ? `1px solid ${C.border}` : undefined }}
            >
              <button
                onClick={() => ask(q)}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 16,
                  padding: "20px 0",
                  background: "transparent",
                  border: "none",
                  cursor: "pointer",
                  textAlign: "left",
                  width: "100%",
                }}
              >
                <div
                  style={{
                    width: 40, height: 40,
                    flexShrink: 0,
                    background: `rgba(93,112,82,0.1)`,
                    color: C.primary,
                    fontFamily: fontSerif,
                    fontWeight: 700,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    borderRadius: R.o1,
                    fontSize: 16,
                  }}
                >
                  {i + 1}
                </div>
                <p style={{ flex: 1, fontSize: 16, fontWeight: 500, color: C.fg, margin: 0, lineHeight: 1.4 }}>{q}</p>
                <span
                  style={{
                    fontSize: 13,
                    color: C.mutedFg,
                    background: C.muted,
                    padding: "4px 12px",
                    borderRadius: 9999,
                    whiteSpace: "nowrap",
                  }}
                >
                  {tc(catKey as Parameters<typeof tc>[0])}
                </span>
              </button>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}
