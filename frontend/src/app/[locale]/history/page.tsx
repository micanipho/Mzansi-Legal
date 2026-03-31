"use client";

import { useState } from "react";
import { useLocale, useTranslations } from "next-intl";
import { MessageSquare, ChevronRight, Clock, PlusCircle } from "lucide-react";
import Link from "next/link";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, R, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";
import AuthGuard from "@/components/guards/AuthGuard";

interface HistoryItem {
  conversationId: string;
  firstQuestion: string;
  questionCount: number;
  startedAt: string;
  locale: string;
}

const MOCK_HISTORY: HistoryItem[] = [
  {
    conversationId: "conv-001",
    firstQuestion: "Can my landlord evict me without a court order?",
    questionCount: 4,
    startedAt: "2026-03-30T14:22:00Z",
    locale: "en",
  },
  {
    conversationId: "conv-002",
    firstQuestion: "What are my CCMA rights if I am unfairly dismissed?",
    questionCount: 2,
    startedAt: "2026-03-28T09:10:00Z",
    locale: "en",
  },
  {
    conversationId: "conv-003",
    firstQuestion: "Ingabe umqashi wami angabamba imali yami ye-deposit?",
    questionCount: 3,
    startedAt: "2026-03-26T17:44:00Z",
    locale: "zu",
  },
];

const LOCALE_NAMES: Record<string, string> = {
  en: "English",
  zu: "isiZulu",
  st: "Sesotho",
  af: "Afrikaans",
};

function formatDate(iso: string, locale: string): string {
  try {
    return new Intl.DateTimeFormat(locale === "zu" || locale === "st" ? "en-ZA" : `${locale}-ZA`, {
      year: "numeric",
      month: "short",
      day: "numeric",
    }).format(new Date(iso));
  } catch {
    return iso.slice(0, 10);
  }
}

export default function HistoryPage() {
  const t = useTranslations("history");
  const locale = useLocale();
  const [items] = useState<HistoryItem[]>(MOCK_HISTORY);

  return (
    <AuthGuard>
      <main
        className="page-shell"
        style={{ display: "flex", flexDirection: "column", gap: 40, fontFamily: fontSans }}
      >
      {/* Page header */}
      <section style={{ display: "flex", alignItems: "flex-end", justifyContent: "space-between", gap: 16, flexWrap: "wrap" }}>
        <div>
          <h1
            style={{
              fontFamily: fontSerif,
              fontSize: "clamp(2rem, 4vw, 2.8rem)",
              fontWeight: 700,
              color: C.fg,
              margin: "0 0 8px",
            }}
          >
            {t("title")}
          </h1>
          <p style={{ color: C.mutedFg, margin: 0, fontSize: 16 }}>
            {t("signInPrompt")}
          </p>
        </div>
        <Link
          href={createLocalizedPath(locale, appRoutes.ask)}
          style={{
            display: "inline-flex",
            alignItems: "center",
            gap: 8,
            background: C.primary,
            color: C.primaryFg,
            padding: "12px 24px",
            borderRadius: 9999,
            fontWeight: 700,
            fontSize: 14,
            textDecoration: "none",
            fontFamily: fontSans,
            flexShrink: 0,
          }}
        >
          <PlusCircle size={16} />
          {t("askQuestion")}
        </Link>
      </section>

      {items.length === 0 ? (
        /* Empty state */
        <section
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            gap: 24,
            padding: "80px 32px",
            textAlign: "center",
            background: C.card,
            border: `1px solid ${C.border}`,
            borderRadius: R.o2,
            boxShadow: shadowOrganic,
          }}
        >
          <div
            style={{
              width: 72,
              height: 72,
              borderRadius: 9999,
              background: C.muted,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: C.mutedFg,
            }}
          >
            <MessageSquare size={36} />
          </div>
          <div>
            <h2 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: "0 0 8px" }}>
              {t("empty")}
            </h2>
            <p style={{ color: C.mutedFg, margin: 0 }}>{t("signInPrompt")}</p>
          </div>
          <Link
            href={createLocalizedPath(locale, appRoutes.ask)}
            style={{
              background: C.primary,
              color: C.primaryFg,
              padding: "12px 32px",
              borderRadius: 9999,
              fontWeight: 700,
              textDecoration: "none",
              fontFamily: fontSans,
            }}
          >
            {t("askQuestion")}
          </Link>
        </section>
      ) : (
        /* Conversation list */
        <section style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          {items.map((item, idx) => {
            const radii = [R.o1, R.o2, R.o3, R.o4];
            const r = radii[idx % radii.length];
            const href = createLocalizedPath(locale, appRoutes.ask, `conversationId=${item.conversationId}`);

            return (
              <Link
                key={item.conversationId}
                href={href}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 20,
                  padding: "24px 28px",
                  background: C.card,
                  border: `1px solid ${C.border}`,
                  borderRadius: r,
                  boxShadow: "0 1px 4px rgba(0,0,0,0.04)",
                  textDecoration: "none",
                  transition: "box-shadow 0.18s ease, transform 0.18s ease",
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.boxShadow = shadowOrganic;
                  e.currentTarget.style.transform = "translateY(-2px)";
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.boxShadow = "0 1px 4px rgba(0,0,0,0.04)";
                  e.currentTarget.style.transform = "translateY(0)";
                }}
              >
                {/* Icon */}
                <div
                  style={{
                    width: 48,
                    height: 48,
                    borderRadius: 9999,
                    background: `rgba(74,90,58,0.1)`,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    color: C.primary,
                    flexShrink: 0,
                  }}
                >
                  <MessageSquare size={22} />
                </div>

                {/* Content */}
                <div style={{ flex: 1, minWidth: 0 }}>
                  <p
                    style={{
                      fontSize: 16,
                      fontWeight: 600,
                      color: C.fg,
                      margin: "0 0 6px",
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      whiteSpace: "nowrap",
                    }}
                  >
                    {item.firstQuestion.length > 120 ? `${item.firstQuestion.slice(0, 120)}…` : item.firstQuestion}
                  </p>
                  <div style={{ display: "flex", alignItems: "center", gap: 16, flexWrap: "wrap" }}>
                    <span
                      style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 4,
                        fontSize: 13,
                        color: C.mutedFg,
                        fontWeight: 500,
                      }}
                    >
                      <Clock size={13} />
                      {formatDate(item.startedAt, locale)}
                    </span>
                    <span
                      style={{
                        padding: "2px 10px",
                        borderRadius: 9999,
                        background: C.muted,
                        color: C.mutedFg,
                        fontSize: 12,
                        fontWeight: 700,
                      }}
                    >
                      {item.questionCount} {item.questionCount === 1 ? "question" : "questions"}
                    </span>
                    {item.locale !== "en" && (
                      <span
                        style={{
                          padding: "2px 10px",
                          borderRadius: 9999,
                          background: `rgba(74,90,58,0.08)`,
                          color: C.primary,
                          fontSize: 12,
                          fontWeight: 700,
                        }}
                      >
                        {LOCALE_NAMES[item.locale] ?? item.locale}
                      </span>
                    )}
                  </div>
                </div>

                {/* Arrow */}
                <ChevronRight size={20} color={C.mutedFg} style={{ flexShrink: 0 }} />
              </Link>
            );
          })}
        </section>
      )}
      </main>
    </AuthGuard>
  );
}
