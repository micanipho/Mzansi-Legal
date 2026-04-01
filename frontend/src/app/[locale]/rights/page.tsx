"use client";

import { useLocale, useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Alert, Button } from "antd";
import { Minus, Plus, Share2, Play } from "lucide-react";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, R, shadowOrganic, fontSerif, fontSans } from "@/styles/theme";
import { useAuth } from "@/hooks/useAuth";

// Filter values are kept as stable English keys for category matching
const FILTERS = ["All", "Employment", "Housing", "Consumer", "Debt & Credit", "Tax", "Privacy"];

// Filter value → categories translation key
const FILTER_CAT_KEYS: Record<string, string> = {
  Employment: "employment",
  Housing: "housing",
  Consumer: "consumer",
  "Debt & Credit": "debtCredit",
  Tax: "tax",
  Privacy: "privacy",
};

interface RightCard {
  id: string;
  category: string;
  r: string;
  hasQuote: boolean;
}

const CARDS: RightCard[] = [
  { id: "eviction",        category: "Housing",      r: R.o1, hasQuote: true  },
  { id: "payslip",         category: "Employment",   r: R.o2, hasQuote: false },
  { id: "defectiveGoods",  category: "Consumer",     r: R.o3, hasQuote: false },
  { id: "interestRates",   category: "Debt & Credit",r: R.o4, hasQuote: false },
  { id: "unfairDismissal", category: "Employment",   r: R.o1, hasQuote: false },
  { id: "dataConsent",     category: "Privacy",      r: R.o2, hasQuote: false },
];

const LOCALE_NAMES: Record<string, string> = {
  en: "English",
  zu: "isiZulu",
  st: "Sesotho",
  af: "Afrikaans",
};

const TOTAL_TOPICS = 20;

export default function MyRightsPage() {
  const locale = useLocale();
  const router = useRouter();
  const t  = useTranslations("rights");
  const tc = useTranslations("categories");
  const tChat = useTranslations("chat");
  const { user } = useAuth();
  const [activeFilter, setActiveFilter] = useState("All");
  const [expanded, setExpanded]         = useState<string | null>(CARDS[0].id);
  const [exploredIds, setExploredIds]   = useState<Set<string>>(new Set([CARDS[0].id]));

  const explored = exploredIds.size;
  const percent  = Math.round((explored / TOTAL_TOPICS) * 100);

  const handleToggle = (id: string) => {
    const opening = expanded !== id;
    setExpanded(opening ? id : null);
    if (opening) {
      setExploredIds((prev) => new Set(prev).add(id));
    }
  };

  const handleSignIn = () => {
    const returnUrl = encodeURIComponent(`/${locale}${appRoutes.rights}`);
    router.push(`${createLocalizedPath(locale, appRoutes.auth)}?returnUrl=${returnUrl}`);
  };

  const getFilterLabel = (f: string) =>
    f === "All" ? t("allCategories") : tc(FILTER_CAT_KEYS[f] as Parameters<typeof tc>[0] ?? f);

  const filtered = activeFilter === "All"
    ? CARDS
    : CARDS.filter((c) => c.category === activeFilter);

  return (
    <main
      className="page-shell"
      style={{
        display: "flex",
        flexDirection: "column",
        gap: 40,
        fontFamily: fontSans,
      }}
    >
      {/* Header */}
      <section style={{ textAlign: "center", maxWidth: 640, margin: "0 auto" }}>
        <h1 style={{ fontFamily: fontSerif, fontSize: "clamp(2rem,5vw,3rem)", fontWeight: 700, color: C.fg, marginBottom: 16 }}>
          {t("title")}
        </h1>
        <p style={{ fontSize: 17, color: C.mutedFg }}>
          {t("headerDesc")}
        </p>
      </section>

      {/* Guest Banner - Only show for non-logged-in users */}
      {!user && (
        <Alert
          type="info"
          message={t("guestProgressBanner")}
          description={t("guestProgressDesc")}
          action={
            <Button
              type="primary"
              size="small"
              onClick={handleSignIn}
              style={{ whiteSpace: "nowrap" }}
            >
              {tChat("guestBannerAction")}
            </Button>
          }
          style={{ borderRadius: 12 }}
          showIcon
        />
      )}

      {/* Knowledge score - Only show for logged-in users */}
      {user && (
        <section
          style={{
            background: C.muted,
            border: `1px solid ${C.border}`,
            borderRadius: R.o2,
            padding: 32,
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            gap: 24,
            flexWrap: "wrap",
            boxShadow: "0 1px 4px rgba(0,0,0,0.04)",
          }}
        >
          <div style={{ flex: 1, minWidth: 200 }}>
            <h2 style={{ fontSize: 17, fontWeight: 700, color: C.fg, marginBottom: 16, fontFamily: fontSans }}>
              {t("knowledgeScore", { explored, total: TOTAL_TOPICS })}
            </h2>
            <div style={{ height: 16, background: C.border, borderRadius: 9999, overflow: "hidden" }}>
              <div style={{ height: "100%", background: C.primary, borderRadius: 9999, width: `${percent}%`, transition: "width 1s ease" }} />
            </div>
          </div>
          <div style={{ fontFamily: fontSerif, fontSize: 48, fontWeight: 700, color: C.primary, flexShrink: 0 }}>
            {percent}%
          </div>
        </section>
      )}

      {/* Filter tabs */}
      <section
        style={{ display: "flex", gap: 12, overflowX: "auto", paddingBottom: 4 }}
        className="hide-scrollbar"
      >
        {FILTERS.map((f) => (
          <button
            key={f}
            onClick={() => setActiveFilter(f)}
            aria-pressed={f === activeFilter}
            style={{
              whiteSpace: "nowrap",
              padding: "10px 24px",
              borderRadius: 9999,
              fontSize: 14,
              fontWeight: 700,
              border: `1px solid ${f === activeFilter ? C.primary : C.border}`,
              background: f === activeFilter ? C.primary : "transparent",
              color: f === activeFilter ? C.primaryFg : C.fg,
              cursor: "pointer",
              fontFamily: fontSans,
            }}
          >
            {getFilterLabel(f)}
          </button>
        ))}
      </section>

      {/* Rights grid */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(320px, 1fr))", gap: 24 }}>
        {filtered.map((card) => {
          const isExpanded = expanded === card.id;
          const titleKey  = `${card.id}Title`  as Parameters<typeof t>[0];
          const citeKey   = `${card.id}Cite`   as Parameters<typeof t>[0];
          const descKey   = `${card.id}Desc`   as Parameters<typeof t>[0];
          const detailKey = `${card.id}Detail` as Parameters<typeof t>[0];
          const quoteKey  = `${card.id}Quote`  as Parameters<typeof t>[0];

          return (
            <div
              key={card.id}
              style={{
                gridColumn: isExpanded ? "1 / -1" : undefined,
                background: C.card,
                border: `1px solid ${C.border}`,
                borderRadius: card.r,
                boxShadow: isExpanded ? shadowOrganic : "0 1px 4px rgba(0,0,0,0.04)",
                overflow: "hidden",
              }}
            >
              {/* Card header */}
              <div
                onClick={() => handleToggle(card.id)}
                style={{
                  padding: "24px 32px",
                  display: "flex",
                  alignItems: "flex-start",
                  justifyContent: "space-between",
                  gap: 16,
                  cursor: "pointer",
                }}
              >
                <div>
                  <span
                    style={{
                      display: "inline-block",
                      fontSize: 11,
                      fontWeight: 700,
                      padding: "3px 10px",
                      borderRadius: 9999,
                      background: `rgba(93,112,82,0.1)`,
                      color: C.primary,
                      marginBottom: 10,
                      fontFamily: fontSans,
                    }}
                  >
                    {tc(FILTER_CAT_KEYS[card.category] as Parameters<typeof tc>[0] ?? card.category)}
                  </span>
                  <h3
                    style={{
                      fontFamily: fontSans,
                      fontSize: isExpanded ? 22 : 17,
                      fontWeight: 700,
                      color: C.fg,
                      margin: "0 0 8px",
                      lineHeight: 1.3,
                    }}
                  >
                    {t(titleKey)}
                  </h3>
                  <span style={{ fontSize: 13, fontWeight: 700, color: C.primary }}>{t(citeKey)}</span>
                  {!isExpanded && (
                    <p style={{ fontSize: 14, color: C.mutedFg, margin: "8px 0 0", lineHeight: 1.5 }}>
                      {t(descKey)}
                    </p>
                  )}
                </div>
                <button
                  style={{
                    width: 40, height: 40,
                    borderRadius: 9999,
                    background: C.muted,
                    border: "none",
                    cursor: "pointer",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    color: C.fg,
                    flexShrink: 0,
                  }}
                  aria-label={isExpanded ? "Collapse" : "Expand"}
                  aria-expanded={isExpanded}
                >
                  {isExpanded ? <Minus size={20} /> : <Plus size={20} />}
                </button>
              </div>

              {/* Expanded content */}
              {isExpanded && (
                <div style={{ borderTop: `1px solid ${C.border}`, padding: "24px 32px", background: C.card }}>
                  <p style={{ fontSize: 17, color: C.fg, lineHeight: 1.7, marginBottom: 24 }}>
                    {t(detailKey)}
                  </p>

                  {card.hasQuote && (
                    <div
                      style={{
                        background: C.muted,
                        padding: 24,
                        borderRadius: 16,
                        borderLeft: `4px solid ${C.primary}`,
                        marginBottom: 32,
                      }}
                    >
                      <p style={{ fontFamily: fontSerif, fontSize: 17, color: C.mutedFg, fontStyle: "italic", margin: 0 }}>
                        {t(quoteKey)}
                      </p>
                    </div>
                  )}

                  <div style={{ display: "flex", flexWrap: "wrap", gap: 16 }}>
                    <button
                      onClick={() => router.push(createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(`Tell me more about: ${t(titleKey)}`)}`))}
                      style={{
                        background: C.primary,
                        color: C.primaryFg,
                        padding: "12px 24px",
                        borderRadius: 9999,
                        fontSize: 14,
                        fontWeight: 700,
                        border: "none",
                        cursor: "pointer",
                        fontFamily: fontSans,
                      }}
                    >
                      {t("askFollowUp")}
                    </button>
                    <button
                      style={{
                        background: "transparent",
                        border: `2px solid ${C.border}`,
                        color: C.fg,
                        padding: "12px 24px",
                        borderRadius: 9999,
                        fontSize: 14,
                        fontWeight: 700,
                        cursor: "pointer",
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        fontFamily: fontSans,
                      }}
                    >
                      <Play size={16} /> {tChat("listenIn", { language: LOCALE_NAMES[locale] ?? locale })}
                    </button>
                    <button
                      style={{
                        background: "transparent",
                        border: `2px solid ${C.border}`,
                        color: C.fg,
                        padding: "12px 24px",
                        borderRadius: 9999,
                        fontSize: 14,
                        fontWeight: 700,
                        cursor: "pointer",
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        fontFamily: fontSans,
                      }}
                    >
                      <Share2 size={16} /> {t("share")}
                    </button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </section>
    </main>
  );
}
