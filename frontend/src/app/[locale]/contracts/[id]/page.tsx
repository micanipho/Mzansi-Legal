import Link from "next/link";
import { notFound } from "next/navigation";
import { getTranslations } from "next-intl/server";
import { AlertCircle, AlertTriangle, ArrowLeft, CheckCircle, FileText, MessageSquareQuote } from "lucide-react";
import { getContractById } from "@/components/contracts/contractData";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, R, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";
import ContractDetailGuard from "./ContractDetailGuard";

interface ContractDetailPageProps {
  params: Promise<{ locale: string; id: string }>;
}

function getVerdictTone(verdict: "good" | "review" | "high-risk") {
  switch (verdict) {
    case "good":
      return {
        labelColor: C.primary,
        background: "rgba(93, 112, 82, 0.09)",
        border: "rgba(93, 112, 82, 0.2)",
      };
    case "review":
      return {
        labelColor: C.secondary,
        background: "rgba(193, 140, 93, 0.09)",
        border: "rgba(193, 140, 93, 0.2)",
      };
    default:
      return {
        labelColor: C.destructive,
        background: "rgba(168, 84, 72, 0.09)",
        border: "rgba(168, 84, 72, 0.2)",
      };
  }
}

export default async function ContractDetailPage({ params }: ContractDetailPageProps) {
  const { locale, id } = await params;
  const t = await getTranslations("contracts");
  const contract = getContractById(id);

  if (!contract) {
    notFound();
  }

  const verdictTone = getVerdictTone(contract.verdict);

  return (
    <ContractDetailGuard>
    <main className="page-shell page-shell--narrow" style={{ display: "flex", flexDirection: "column", gap: 32 }}>
      <Link
        href={createLocalizedPath(locale, appRoutes.contracts)}
        style={{
          display: "inline-flex",
          alignItems: "center",
          gap: 8,
          color: C.mutedFg,
          textDecoration: "none",
          width: "fit-content",
          fontWeight: 600,
        }}
      >
        <ArrowLeft size={16} />
        {t("backToContracts")}
      </Link>

      <section style={{ display: "flex", gap: 24, alignItems: "stretch", flexWrap: "wrap" }} className="md-flex-row">
        <article
          className="surface-card grain-panel"
          style={{
            flex: "0 0 180px",
            minHeight: 180,
            borderRadius: R.o1,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flexDirection: "column",
            boxShadow: shadowOrganic,
          }}
        >
          <strong style={{ fontFamily: fontSerif, fontSize: 64, lineHeight: 1, color: C.fg }}>{contract.score}</strong>
          <span style={{ color: C.mutedFg, fontWeight: 700 }}>{t("scoreSuffix")}</span>
        </article>

        <article style={{ flex: 1, minWidth: 280, display: "flex", flexDirection: "column", gap: 20 }}>
          <div style={{ display: "flex", gap: 12, flexWrap: "wrap", alignItems: "center" }}>
            <span
              style={{
                padding: "8px 14px",
                borderRadius: 9999,
                background: verdictTone.background,
                border: `1px solid ${verdictTone.border}`,
                color: verdictTone.labelColor,
                fontWeight: 800,
                fontSize: 13,
              }}
            >
              {t(`verdict.${contract.verdict}`)}
            </span>
            <span style={{ color: C.mutedFg, fontSize: 14 }}>
              {t("detailMeta", {
                date: contract.uploadedAt,
                pages: contract.pages,
                clauses: contract.clauses,
                language: contract.language,
              })}
            </span>
          </div>

          <div>
            <h1 style={{ margin: "0 0 12px", fontFamily: fontSerif, fontSize: "clamp(2.2rem, 4vw, 3.4rem)", color: C.fg }}>
              {contract.title}
            </h1>
            <p style={{ margin: 0, color: C.mutedFg, fontSize: 17, lineHeight: 1.7 }}>{contract.summary}</p>
          </div>

          <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
            {contract.tags.map((tag) => (
              <span
                key={tag}
                style={{
                  padding: "6px 14px",
                  borderRadius: 9999,
                  background: C.muted,
                  color: C.fg,
                  fontWeight: 700,
                  fontSize: 13,
                }}
              >
                {tag}
              </span>
            ))}
          </div>
        </article>
      </section>

      <section className="responsive-three-grid">
        <article
          className="surface-card grain-panel"
          style={{ borderRadius: R.o2, padding: 24, textAlign: "center", boxShadow: shadowOrganic }}
        >
          <strong style={{ display: "block", fontFamily: fontSerif, fontSize: 40, color: C.destructive }}>
            {contract.redFlags.length}
          </strong>
          <span style={{ color: C.mutedFg, fontWeight: 700 }}>{t("redFlags")}</span>
        </article>
        <article
          className="surface-card grain-panel"
          style={{ borderRadius: R.o3, padding: 24, textAlign: "center", boxShadow: shadowOrganic }}
        >
          <strong style={{ display: "block", fontFamily: fontSerif, fontSize: 40, color: C.secondary }}>
            {contract.cautionFlags.length}
          </strong>
          <span style={{ color: C.mutedFg, fontWeight: 700 }}>{t("caution")}</span>
        </article>
        <article
          className="surface-card grain-panel"
          style={{ borderRadius: R.o4, padding: 24, textAlign: "center", boxShadow: shadowOrganic }}
        >
          <strong style={{ display: "block", fontFamily: fontSerif, fontSize: 40, color: C.primary }}>
            {contract.standardCount}
          </strong>
          <span style={{ color: C.mutedFg, fontWeight: 700 }}>{t("standard")}</span>
        </article>
      </section>

      <section
        className="surface-card grain-panel"
        style={{ borderRadius: R.o2, padding: 28, boxShadow: shadowOrganic, display: "flex", flexDirection: "column", gap: 16 }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <MessageSquareQuote size={20} color={C.secondary} />
          <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28 }}>{t("plainSummary")}</h2>
        </div>
        <p style={{ margin: 0, color: C.fg, lineHeight: 1.7 }}>{contract.recommendation}</p>
        <Link
          href={createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(`${t("followUpPrompt")} ${contract.title}`)}`)}
          style={{
            display: "inline-flex",
            width: "fit-content",
            alignItems: "center",
            gap: 8,
            padding: "12px 20px",
            borderRadius: 9999,
            background: C.primary,
            color: C.primaryFg,
            textDecoration: "none",
            fontWeight: 700,
            fontFamily: fontSans,
          }}
        >
          <FileText size={16} />
          {t("askAboutContract")}
        </Link>
      </section>

      <section style={{ display: "grid", gap: 20 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <AlertCircle size={20} color={C.destructive} />
          <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28 }}>{t("redFlags")}</h2>
        </div>
        {contract.redFlags.map((flag) => (
          <article
            key={flag.title}
            className="surface-card grain-panel"
            style={{
              borderRadius: 24,
              padding: 24,
              borderLeft: `4px solid ${C.destructive}`,
              boxShadow: shadowOrganic,
            }}
          >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: C.destructive, fontFamily: fontSans }}>{flag.title}</h3>
            <p style={{ margin: "0 0 12px", lineHeight: 1.7, color: C.fg }}>{flag.detail}</p>
            {flag.citation ? (
              <span
                style={{
                  display: "inline-flex",
                  padding: "5px 12px",
                  borderRadius: 9999,
                  background: "rgba(168, 84, 72, 0.1)",
                  color: C.destructive,
                  fontWeight: 700,
                  fontSize: 12,
                }}
              >
                {flag.citation}
              </span>
            ) : null}
          </article>
        ))}
      </section>

      <section style={{ display: "grid", gap: 20 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <AlertTriangle size={20} color={C.secondary} />
          <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28 }}>{t("caution")}</h2>
        </div>
        {contract.cautionFlags.map((flag) => (
          <article
            key={flag.title}
            className="surface-card grain-panel"
            style={{
              borderRadius: 24,
              padding: 24,
              borderLeft: `4px solid ${C.secondary}`,
              boxShadow: shadowOrganic,
            }}
          >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: C.secondary, fontFamily: fontSans }}>{flag.title}</h3>
            <p style={{ margin: 0, lineHeight: 1.7, color: C.fg }}>{flag.detail}</p>
          </article>
        ))}
      </section>

      <section
        className="surface-card grain-panel"
        style={{ borderRadius: 24, padding: 24, display: "flex", alignItems: "center", gap: 16, boxShadow: shadowOrganic }}
      >
        <CheckCircle size={22} color={C.primary} />
        <div>
          <h2 style={{ margin: "0 0 6px", fontFamily: fontSans, fontSize: 18, color: C.primary }}>{t("allStandard")}</h2>
          <p style={{ margin: 0, lineHeight: 1.6, color: C.fg }}>{t("standardSummary", { count: contract.standardCount })}</p>
        </div>
      </section>
    </main>
    </ContractDetailGuard>
  );
}
