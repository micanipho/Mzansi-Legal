"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { useEffect, useRef } from "react";
import { FileSearch, ArrowRight, UploadCloud, ShieldCheck, TriangleAlert } from "lucide-react";
import { demoContracts } from "@/components/contracts/contractData";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, R, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";
import { useAuth } from "@/hooks/useAuth";

function getScoreTone(score: number) {
  if (score >= 75) {
    return { fg: C.primary, bg: "rgba(93, 112, 82, 0.08)", border: "rgba(93, 112, 82, 0.2)" };
  }

  if (score >= 55) {
    return { fg: C.secondary, bg: "rgba(193, 140, 93, 0.08)", border: "rgba(193, 140, 93, 0.2)" };
  }

  return { fg: C.destructive, bg: "rgba(168, 84, 72, 0.08)", border: "rgba(168, 84, 72, 0.2)" };
}

export default function ContractsPage() {
  const locale = useLocale();
  const router = useRouter();
  const t = useTranslations("contracts");
  const { user, isLoading } = useAuth();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // In-page auth guard fallback (middleware handles most cases, this is backup)
  useEffect(() => {
    if (!isLoading && !user) {
      router.push(createLocalizedPath(locale, appRoutes.auth));
    }
  }, [isLoading, user, locale, router]);

  if (isLoading) {
    return (
      <main className="page-shell" style={{ display: "flex", alignItems: "center", justifyContent: "center", fontFamily: fontSans }}>
        <span style={{ color: C.mutedFg }}>{/* loading */}</span>
      </main>
    );
  }

  if (!user) {
    return null;
  }

  return (
    <main className="page-shell" style={{ display: "flex", flexDirection: "column", gap: 32, fontFamily: fontSans }}>
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
          <h1 style={{ margin: 0, fontFamily: fontSerif, fontSize: "clamp(2.4rem, 4vw, 3.6rem)", color: C.fg }}>
            {t("uploadTitle")}
          </h1>
          <p style={{ margin: 0, color: C.mutedFg, fontSize: 17, lineHeight: 1.7 }}>{t("uploadHint")}</p>
          <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
            <Link
              href={createLocalizedPath(locale, `${appRoutes.contracts}/${demoContracts[0].id}`)}
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: 8,
                padding: "12px 20px",
                borderRadius: 9999,
                background: C.primary,
                color: C.primaryFg,
                textDecoration: "none",
                fontWeight: 700,
              }}
            >
              <FileSearch size={16} />
              {t("viewFeatured")}
            </Link>
            <button
              onClick={() => fileInputRef.current?.click()}
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: 8,
                padding: "12px 18px",
                borderRadius: 9999,
                border: `1px solid ${C.border}`,
                color: C.mutedFg,
                background: "rgba(255,255,255,0.62)",
                fontWeight: 700,
                cursor: "pointer",
              }}
              aria-label={t("uploadButton")}
            >
              <UploadCloud size={16} />
              {t("uploadButton")}
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept=".pdf"
              aria-label={t("uploadButton")}
              style={{ display: "none" }}
              onChange={(e) => {
                const file = e.target.files?.[0];
                if (file) {
                  // Upload handling placeholder — file selected
                  e.target.value = "";
                }
              }}
            />
          </div>
          <p style={{ margin: 0, fontSize: 13, color: C.mutedFg }}>{t("pdfLimit")}</p>
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
          <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>{t("whyItMattersBody")}</p>
          <div style={{ display: "grid", gap: 12 }}>
            {[
              { icon: <ShieldCheck size={18} color={C.primary} />, text: t("benefits.fairness") },
              { icon: <TriangleAlert size={18} color={C.secondary} />, text: t("benefits.risk") },
              { icon: <ArrowRight size={18} color={C.destructive} />, text: t("benefits.nextStep") },
            ].map((item) => (
              <div key={item.text} style={{ display: "flex", alignItems: "center", gap: 10, color: C.fg }}>
                {item.icon}
                <span>{item.text}</span>
              </div>
            ))}
          </div>
        </article>
      </section>

      <section style={{ display: "flex", flexDirection: "column", gap: 18 }}>
        <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center", flexWrap: "wrap" }}>
          <div>
            <h2 style={{ margin: "0 0 6px", fontFamily: fontSerif, fontSize: 30, color: C.fg }}>{t("recentAnalyses")}</h2>
            <p style={{ margin: 0, color: C.mutedFg }}>{t("recentAnalysesHint")}</p>
          </div>
        </div>

        <div style={{ display: "grid", gap: 20 }}>
          {demoContracts.map((contract) => {
            const tone = getScoreTone(contract.score);
            const detailHref = createLocalizedPath(locale, `${appRoutes.contracts}/${contract.id}`);

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
                  <strong style={{ fontFamily: fontSerif, fontSize: 42, color: tone.fg, lineHeight: 1 }}>{contract.score}</strong>
                  <span style={{ color: C.mutedFg, fontWeight: 700, fontSize: 13 }}>{t("scoreSuffix")}</span>
                </div>

                <div style={{ minWidth: 0, display: "grid", gap: 10 }}>
                  <div style={{ display: "flex", gap: 10, flexWrap: "wrap", alignItems: "center" }}>
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
                      {contract.category}
                    </span>
                    <span style={{ color: C.mutedFg, fontSize: 13 }}>
                      {t("cardMeta", {
                        date: contract.uploadedAt,
                        pages: contract.pages,
                        clauses: contract.clauses,
                      })}
                    </span>
                  </div>
                  <div>
                    <h3 style={{ margin: "0 0 8px", fontFamily: fontSerif, fontSize: 28, color: C.fg }}>{contract.title}</h3>
                    <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>{contract.summary}</p>
                  </div>
                  <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                    {contract.tags.map((tag) => (
                      <span
                        key={tag}
                        style={{
                          padding: "4px 10px",
                          borderRadius: 9999,
                          background: "rgba(230, 220, 205, 0.6)",
                          color: C.fg,
                          fontWeight: 700,
                          fontSize: 12,
                        }}
                      >
                        {tag}
                      </span>
                    ))}
                  </div>
                </div>

                <div style={{ display: "flex", justifyContent: "flex-end" }}>
                  <Link
                    href={detailHref}
                    style={{
                      display: "inline-flex",
                      alignItems: "center",
                      gap: 8,
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
  );
}
