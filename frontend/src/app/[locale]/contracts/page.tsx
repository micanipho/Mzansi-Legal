"use client";

import { Skeleton } from "antd";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { DragEvent, useEffect, useRef, useState } from "react";
import {
  ArrowRight,
  ShieldCheck,
  TriangleAlert,
  UploadCloud,
} from "lucide-react";
import RetryNotice from "@/components/feedback/RetryNotice";
import {
  formatContractDate,
  getContractTypeLabel,
} from "@/components/contracts/contractData";
import AuthGuard from "@/components/guards/AuthGuard";
import { useOnlineStatus } from "@/hooks/useOnlineStatus";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import {
  useContractsAction,
  useContractsState,
  ContractsProvider,
} from "@/providers/contracts-provider";
import { C, fontSans, fontSerif, R, shadowOrganic } from "@/styles/theme";

function getScoreTone(score: number) {
  if (score >= 75) {
    return {
      fg: C.primary,
      bg: "rgba(93, 112, 82, 0.08)",
      border: "rgba(93, 112, 82, 0.2)",
    };
  }

  if (score >= 55) {
    return {
      fg: C.secondary,
      bg: "rgba(193, 140, 93, 0.08)",
      border: "rgba(193, 140, 93, 0.2)",
    };
  }

  return {
    fg: C.destructive,
    bg: "rgba(168, 84, 72, 0.08)",
    border: "rgba(168, 84, 72, 0.2)",
  };
}

function ContractsContent() {
  const locale = useLocale();
  const router = useRouter();
  const t = useTranslations("contracts");
  const isOnline = useOnlineStatus();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragActive, setIsDragActive] = useState(false);
  const {
    items: contracts,
    isPending,
    isError,
    errorMessage,
  } = useContractsState();
  const { fetchAll, analyse } = useContractsAction();

  useEffect(() => {
    void fetchAll(locale);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [locale]);

  const handleSelectedFile = async (file: File) => {
    const result = await analyse(file, locale);
    router.push(
      createLocalizedPath(locale, `${appRoutes.contracts}/${result.id}`),
    );
  };

  const handleDrop = async (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setIsDragActive(false);

    const file = event.dataTransfer.files?.[0];
    if (!file) {
      return;
    }

    try {
      await handleSelectedFile(file);
    } catch {
      // Provider state already captures the upload error for the UI.
    }
  };

  return (
    <AuthGuard>
      <main
        className="page-shell"
        style={{
          display: "flex",
          flexDirection: "column",
          gap: 32,
          fontFamily: fontSans,
        }}
      >
        <section className="responsive-two-grid">
          <article
            className="surface-card grain-panel"
            style={{
              borderRadius: R.o2,
              padding: 32,
              boxShadow: shadowOrganic,
              display: "flex",
              flexDirection: "column",
              gap: 18,
            }}
          >
            <span
              style={{
                width: "fit-content",
                padding: "6px 14px",
                borderRadius: 9999,
                background: "rgba(93, 112, 82, 0.1)",
                color: C.primary,
                fontWeight: 800,
                fontSize: 12,
                letterSpacing: "0.08em",
                textTransform: "uppercase",
              }}
            >
              {t("title")}
            </span>
            <h1
              style={{
                margin: 0,
                fontFamily: fontSerif,
                fontSize: "clamp(2.4rem, 4vw, 3.6rem)",
                color: C.fg,
              }}
            >
              {t("uploadTitle")}
            </h1>
            <p
              style={{
                margin: 0,
                color: C.mutedFg,
                fontSize: 17,
                lineHeight: 1.7,
              }}
            >
              {t("uploadHint")}
            </p>
            <div
              role="button"
              tabIndex={0}
              onClick={() => fileInputRef.current?.click()}
              onKeyDown={(event) => {
                if (event.key === "Enter" || event.key === " ") {
                  event.preventDefault();
                  fileInputRef.current?.click();
                }
              }}
              onDragOver={(event) => {
                event.preventDefault();
                setIsDragActive(true);
              }}
              onDragLeave={() => setIsDragActive(false)}
              onDrop={(event) => void handleDrop(event)}
              style={{
                borderRadius: 28,
                border: `2px dashed ${isDragActive ? C.primary : "rgba(126, 107, 86, 0.28)"}`,
                background: isDragActive
                  ? "rgba(93, 112, 82, 0.08)"
                  : "rgba(255,255,255,0.55)",
                padding: 28,
                display: "grid",
                gap: 10,
                cursor: isPending ? "progress" : "pointer",
                transition: "all 160ms ease",
                minHeight: 220,
              }}
              aria-label={t("uploadButton")}
            >
              <div
                style={{
                  display: "inline-flex",
                  alignItems: "center",
                  gap: 10,
                  color: C.primary,
                  fontWeight: 800,
                }}
              >
                <UploadCloud size={18} />
                {isPending ? t("loading") : t("dragDropTitle")}
              </div>
              <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>
                {t("dragDropHint")}
              </p>
              <p style={{ margin: 0, fontSize: 13, color: C.mutedFg }}>
                {t("pdfLimit")}
              </p>
              <p style={{ margin: 0, fontSize: 13, color: C.mutedFg }}>
                {t("mobileScanHint")}
              </p>
            </div>
            <input
              ref={fileInputRef}
              type="file"
              accept=".pdf,application/pdf"
              aria-label={t("uploadButton")}
              style={{ display: "none" }}
              onChange={async (event) => {
                const file = event.target.files?.[0];
                if (!file) {
                  return;
                }

                try {
                  await handleSelectedFile(file);
                } catch {
                  // Provider state already captures the upload error for the UI.
                } finally {
                  event.target.value = "";
                }
              }}
            />
            {isError && errorMessage ? (
              <RetryNotice
                compact
                title={isOnline ? "Upload failed" : "You're offline right now"}
                description={errorMessage}
                onRetry={() => fileInputRef.current?.click()}
                isOffline={!isOnline}
              />
            ) : null}
          </article>

          <article
            className="surface-card grain-panel"
            style={{
              borderRadius: R.o1,
              padding: 28,
              boxShadow: shadowOrganic,
              display: "grid",
              gap: 18,
              alignContent: "start",
            }}
          >
            <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
              <ShieldCheck size={20} color={C.primary} />
              <strong style={{ color: C.fg }}>{t("whyItMattersTitle")}</strong>
            </div>
            <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>
              {t("whyItMattersBody")}
            </p>
            <div style={{ display: "grid", gap: 12 }}>
              {[
                {
                  icon: <ShieldCheck size={18} color={C.primary} />,
                  text: t("benefits.fairness"),
                },
                {
                  icon: <TriangleAlert size={18} color={C.secondary} />,
                  text: t("benefits.risk"),
                },
                {
                  icon: <ArrowRight size={18} color={C.destructive} />,
                  text: t("benefits.nextStep"),
                },
              ].map((item) => (
                <div
                  key={item.text}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: 10,
                    color: C.fg,
                  }}
                >
                  {item.icon}
                  <span>{item.text}</span>
                </div>
              ))}
            </div>
          </article>
        </section>

        <section style={{ display: "flex", flexDirection: "column", gap: 18 }}>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              gap: 12,
              alignItems: "center",
              flexWrap: "wrap",
            }}
          >
            <div>
              <h2
                style={{
                  margin: "0 0 6px",
                  fontFamily: fontSerif,
                  fontSize: 30,
                  color: C.fg,
                }}
              >
                {t("recentAnalyses")}
              </h2>
              <p style={{ margin: 0, color: C.mutedFg }}>
                {t("recentAnalysesHint")}
              </p>
            </div>
          </div>

          {contracts.length === 0 && !isPending ? (
            <article
              className="surface-card grain-panel"
              style={{
                borderRadius: 28,
                padding: 28,
                boxShadow: shadowOrganic,
                display: "grid",
                gap: 10,
              }}
            >
              <h3
                style={{
                  margin: 0,
                  fontFamily: fontSerif,
                  fontSize: 28,
                  color: C.fg,
                }}
              >
                {t("emptyTitle")}
              </h3>
              <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>
                {t("emptyBody")}
              </p>
            </article>
          ) : null}

          {isPending && contracts.length === 0 ? (
            <div style={{ display: "grid", gap: 20 }}>
              {Array.from({ length: 3 }).map((_, index) => (
                <article
                  key={index}
                  className="surface-card grain-panel"
                  style={{
                    borderRadius: 28,
                    padding: 24,
                    boxShadow: shadowOrganic,
                  }}
                >
                  <Skeleton
                    active
                    paragraph={{ rows: 2 }}
                    title={{ width: "55%" }}
                  />
                </article>
              ))}
            </div>
          ) : null}

          <div style={{ display: "grid", gap: 20 }}>
            {contracts.map((contract) => {
              const tone = getScoreTone(contract.healthScore);
              const detailHref = createLocalizedPath(
                locale,
                `${appRoutes.contracts}/${contract.id}`,
              );

              return (
                <article
                  key={contract.id}
                  className="surface-card grain-panel responsive-card-rail"
                  style={{
                    borderRadius: 28,
                    padding: 24,
                    boxShadow: shadowOrganic,
                  }}
                >
                  <div
                    style={{
                      background: tone.bg,
                      border: `1px solid ${tone.border}`,
                      borderRadius: 24,
                      minHeight: 120,
                      display: "flex",
                      flexDirection: "column",
                      alignItems: "center",
                      justifyContent: "center",
                    }}
                  >
                    <strong
                      style={{
                        fontFamily: fontSerif,
                        fontSize: 42,
                        color: tone.fg,
                        lineHeight: 1,
                      }}
                    >
                      {contract.healthScore}
                    </strong>
                    <span
                      style={{
                        color: C.mutedFg,
                        fontWeight: 700,
                        fontSize: 13,
                      }}
                    >
                      {t("scoreSuffix")}
                    </span>
                  </div>

                  <div style={{ minWidth: 0, display: "grid", gap: 10 }}>
                    <div
                      style={{
                        display: "flex",
                        gap: 10,
                        flexWrap: "wrap",
                        alignItems: "center",
                      }}
                    >
                      <span
                        style={{
                          padding: "5px 12px",
                          borderRadius: 9999,
                          background: C.muted,
                          color: C.fg,
                          fontWeight: 700,
                          fontSize: 12,
                        }}
                      >
                        {getContractTypeLabel(contract.contractType)}
                      </span>
                      <span style={{ color: C.mutedFg, fontSize: 13 }}>
                        {t("cardMeta", {
                          date: formatContractDate(contract.analysedAt, locale),
                          red: contract.redFlagCount,
                          amber: contract.amberFlagCount,
                          green: contract.greenFlagCount,
                        })}
                      </span>
                    </div>
                    <div>
                      <h3
                        style={{
                          margin: "0 0 8px",
                          fontFamily: fontSerif,
                          fontSize: 28,
                          color: C.fg,
                        }}
                      >
                        {contract.displayTitle}
                      </h3>
                      <p
                        style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}
                      >
                        {contract.summary}
                      </p>
                    </div>
                  </div>

                  <div style={{ display: "flex", justifyContent: "flex-end" }}>
                    <Link
                      href={detailHref}
                      style={{
                        display: "inline-flex",
                        alignItems: "center",
                        gap: 8,
                        minHeight: 44,
                        padding: "12px 18px",
                        borderRadius: 9999,
                        textDecoration: "none",
                        background: C.primary,
                        color: C.primaryFg,
                        fontWeight: 700,
                        whiteSpace: "nowrap",
                      }}
                    >
                      {t("openAnalysis")}
                      <ArrowRight size={16} />
                    </Link>
                  </div>
                </article>
              );
            })}
          </div>
        </section>
      </main>
    </AuthGuard>
  );
}

export default function ContractsPage() {
  return (
    <ContractsProvider>
      <ContractsContent />
    </ContractsProvider>
  );
}
