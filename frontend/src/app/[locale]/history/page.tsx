"use client";

import { Skeleton } from "antd";
import { useLocale, useTranslations } from "next-intl";
import { MessageSquare, ChevronRight, Clock, PlusCircle } from "lucide-react";
import Link from "next/link";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, R, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";
import AuthGuard from "@/components/guards/AuthGuard";
import RetryNotice from "@/components/feedback/RetryNotice";
import { useOnlineStatus } from "@/hooks/useOnlineStatus";
import {
  HistoryProvider,
  useHistoryAction,
  useHistoryState,
} from "@/providers/history-provider";

const LOCALE_NAMES: Record<string, string> = {
  en: "English",
  zu: "isiZulu",
  st: "Sesotho",
  af: "Afrikaans",
};

function formatDate(iso: string, locale: string): string {
  try {
    return new Intl.DateTimeFormat(
      locale === "zu" || locale === "st" ? "en-ZA" : `${locale}-ZA`,
      {
        year: "numeric",
        month: "short",
        day: "numeric",
      },
    ).format(new Date(iso));
  } catch {
    return iso.slice(0, 10);
  }
}

function HistoryContent() {
  const t = useTranslations("history");
  const locale = useLocale();
  const isOnline = useOnlineStatus();
  const {
    items,
    isPending: isLoading,
    isError,
    errorMessage,
  } = useHistoryState();
  const { fetchAll } = useHistoryAction();

  if (isLoading) {
    return (
      <main
        className="page-shell"
        style={{ display: "grid", gap: 18, fontFamily: fontSans }}
      >
        {Array.from({ length: 3 }).map((_, index) => (
          <section
            key={index}
            className="surface-card grain-panel"
            style={{
              padding: 24,
              borderRadius: R.o2,
              boxShadow: shadowOrganic,
            }}
          >
            <Skeleton active paragraph={{ rows: 5 }} title={{ width: "70%" }} />
          </section>
        ))}
      </main>
    );
  }

  if (isError) {
    return (
      <main
        className="page-shell"
        style={{ display: "grid", gap: 18, fontFamily: fontSans }}
      >
        <RetryNotice
          title={
            isOnline
              ? "We couldn't load your history"
              : "You're offline right now"
          }
          description={errorMessage ?? "Please try again in a moment."}
          onRetry={fetchAll}
          isOffline={!isOnline}
        />
      </main>
    );
  }

  return (
    <AuthGuard>
      <main
        className="page-shell"
        style={{
          display: "flex",
          flexDirection: "column",
          gap: 40,
          fontFamily: fontSans,
        }}
      >
        <section
          style={{
            display: "flex",
            alignItems: "flex-end",
            justifyContent: "space-between",
            gap: 16,
            flexWrap: "wrap",
          }}
        >
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
              <h2
                style={{
                  fontFamily: fontSerif,
                  fontSize: 24,
                  fontWeight: 700,
                  color: C.fg,
                  margin: "0 0 8px",
                }}
              >
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
          <section
            style={{ display: "flex", flexDirection: "column", gap: 16 }}
          >
            {items.map((item, idx) => {
              const radii = [R.o1, R.o2, R.o3, R.o4];
              const r = radii[idx % radii.length];
              const href = createLocalizedPath(
                locale,
                appRoutes.ask,
                `conversationId=${item.conversationId}`,
              );

              return (
                <div
                  key={item.conversationId}
                  className="grain-panel"
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: 20,
                    padding: "24px 28px",
                    background: C.card,
                    border: `1px solid ${C.border}`,
                    borderRadius: r,
                    boxShadow: shadowOrganic,
                  }}
                >
                  <Link
                    href={href}
                    style={{
                      display: "flex",
                      alignItems: "center",
                      gap: 20,
                      textDecoration: "none",
                    }}
                  >
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
                        {item.firstQuestion.length > 120
                          ? `${item.firstQuestion.slice(0, 120)}...`
                          : item.firstQuestion}
                      </p>
                      <div
                        style={{
                          display: "flex",
                          alignItems: "center",
                          gap: 16,
                          flexWrap: "wrap",
                        }}
                      >
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
                          {item.questionCount}{" "}
                          {item.questionCount === 1 ? "question" : "questions"}
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

                    <ChevronRight
                      size={20}
                      color={C.mutedFg}
                      style={{ flexShrink: 0 }}
                    />
                  </Link>

                  {item.messages.length > 0 && (
                    <div
                      style={{
                        display: "flex",
                        flexDirection: "column",
                        gap: 12,
                      }}
                    >
                      {item.messages.map((message) => (
                        <div
                          key={message.messageId}
                          style={{
                            alignSelf:
                              message.type === "user"
                                ? "flex-end"
                                : "flex-start",
                            maxWidth: "min(85%, 680px)",
                            padding: "14px 18px",
                            borderRadius: 18,
                            background:
                              message.type === "user" ? C.primary : C.muted,
                            color:
                              message.type === "user"
                                ? C.primaryFg
                                : C.fg,
                            fontFamily: fontSans,
                            lineHeight: 1.6,
                            whiteSpace: "pre-wrap",
                          }}
                        >
                          {message.text}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </section>
        )}
      </main>
    </AuthGuard>
  );
}

export default function HistoryPage() {
  return (
    <HistoryProvider>
      <HistoryContent />
    </HistoryProvider>
  );
}
