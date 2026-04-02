"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect } from "react";
import { useLocale, useTranslations } from "next-intl";
import {
  AlertCircle,
  AlertTriangle,
  ArrowLeft,
  CheckCircle,
  MessageSquareQuote,
} from "lucide-react";
import {
  ContractsProvider,
  useContractsAction,
  useContractsState,
} from "@/providers/contracts-provider";
import {
  formatContractDate,
  getContractTypeLabel,
  getContractVerdict,
  groupFlags,
} from "@/components/contracts/contractData";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, fontSans, fontSerif, R, shadowOrganic } from "@/styles/theme";
import ContractDetailGuard from "./ContractDetailGuard";

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

function ContractDetailContent() {
  const { id } = useParams<{ id: string }>();
  const locale = useLocale();
  const t = useTranslations("contracts");
  const { selected, isPending, isError, errorMessage } = useContractsState();
  const { fetchById } = useContractsAction();

  useEffect(() => {
    if (!id) {
      return;
    }

    void fetchById(id, locale);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, locale]);

  if (isPending && !selected) {
    return (
      <main className="page-shell page-shell--narrow" style={{ display: "grid", gap: 16 }}>
        <p style={{ margin: 0, color: C.mutedFg, fontFamily: fontSans }}>{t("loading")}</p>
      </main>
    );
  }

  if (isError || !selected) {
    return (
      <main className="page-shell page-shell--narrow" style={{ display: "grid", gap: 16 }}>
        <Link
          href={createLocalizedPath(locale, appRoutes.contracts)}
          style={{ color: C.mutedFg, textDecoration: "none", fontWeight: 600, width: "fit-content" }}
        >
          {t("backToContracts")}
        </Link>
        <article className="surface-card grain-panel" style={{ borderRadius: 24, padding: 24, boxShadow: shadowOrganic }}>
          <h1 style={{ margin: "0 0 10px", fontFamily: fontSerif, fontSize: 28, color: C.destructive }}>
            {t("detailErrorTitle")}
          </h1>
          <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>
            {errorMessage ?? t("detailErrorBody")}
          </p>
        </article>
      </main>
    );
  }

  const verdict = getContractVerdict(selected.healthScore);
  const verdictTone = getVerdictTone(verdict);
  const groupedFlags = groupFlags(selected.flags);

  return (
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
          <strong style={{ fontFamily: fontSerif, fontSize: 64, lineHeight: 1, color: C.fg }}>
            {selected.healthScore}
          </strong>
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
              {t(`verdict.${verdict}`)}
            </span>
            <span style={{ color: C.mutedFg, fontSize: 14 }}>
              {t("detailMeta", {
                date: formatContractDate(selected.analysedAt, locale),
                language: selected.language,
                type: getContractTypeLabel(selected.contractType),
              })}
            </span>
          </div>

          <div>
            <h1 style={{ margin: "0 0 12px", fontFamily: fontSerif, fontSize: "clamp(2.2rem, 4vw, 3.4rem)", color: C.fg }}>
              {selected.displayTitle}
            </h1>
            <p style={{ margin: 0, color: C.mutedFg, fontSize: 17, lineHeight: 1.7 }}>{selected.summary}</p>
          </div>
        </article>
      </section>

      <section className="responsive-three-grid">
        <article className="surface-card grain-panel" style={{ borderRadius: R.o2, padding: 24, textAlign: "center", boxShadow: shadowOrganic }}>
          <strong style={{ display: "block", fontFamily: fontSerif, fontSize: 40, color: C.destructive }}>
            {selected.redFlagCount}
          </strong>
          <span style={{ color: C.mutedFg, fontWeight: 700 }}>{t("redFlags")}</span>
        </article>
        <article className="surface-card grain-panel" style={{ borderRadius: R.o3, padding: 24, textAlign: "center", boxShadow: shadowOrganic }}>
          <strong style={{ display: "block", fontFamily: fontSerif, fontSize: 40, color: C.secondary }}>
            {selected.amberFlagCount}
          </strong>
          <span style={{ color: C.mutedFg, fontWeight: 700 }}>{t("caution")}</span>
        </article>
        <article className="surface-card grain-panel" style={{ borderRadius: R.o4, padding: 24, textAlign: "center", boxShadow: shadowOrganic }}>
          <strong style={{ display: "block", fontFamily: fontSerif, fontSize: 40, color: C.primary }}>
            {selected.greenFlagCount}
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
        <p style={{ margin: 0, color: C.fg, lineHeight: 1.7 }}>{selected.summary}</p>
      </section>

      <section style={{ display: "grid", gap: 20 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <AlertCircle size={20} color={C.destructive} />
          <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28 }}>{t("redFlags")}</h2>
        </div>
        {groupedFlags.redFlags.length === 0 ? (
          <p style={{ margin: 0, color: C.mutedFg }}>{t("noRedFlags")}</p>
        ) : null}
        {groupedFlags.redFlags.map((flag) => (
          <article
            key={`${flag.title}-${flag.clauseText}`}
            className="surface-card grain-panel"
            style={{ borderRadius: 24, padding: 24, borderLeft: `4px solid ${C.destructive}`, boxShadow: shadowOrganic }}
          >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: C.destructive, fontFamily: fontSans }}>{flag.title}</h3>
            <p style={{ margin: "0 0 12px", lineHeight: 1.7, color: C.fg }}>{flag.description}</p>
            <p style={{ margin: "0 0 12px", lineHeight: 1.7, color: C.mutedFg }}>{flag.clauseText}</p>
            {flag.legislationCitation ? (
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
                {flag.legislationCitation}
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
        {groupedFlags.cautionFlags.length === 0 ? (
          <p style={{ margin: 0, color: C.mutedFg }}>{t("noCautionFlags")}</p>
        ) : null}
        {groupedFlags.cautionFlags.map((flag) => (
          <article
            key={`${flag.title}-${flag.clauseText}`}
            className="surface-card grain-panel"
            style={{ borderRadius: 24, padding: 24, borderLeft: `4px solid ${C.secondary}`, boxShadow: shadowOrganic }}
          >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: C.secondary, fontFamily: fontSans }}>{flag.title}</h3>
            <p style={{ margin: "0 0 12px", lineHeight: 1.7, color: C.fg }}>{flag.description}</p>
            <p style={{ margin: 0, lineHeight: 1.7, color: C.mutedFg }}>{flag.clauseText}</p>
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
          <p style={{ margin: 0, lineHeight: 1.6, color: C.fg }}>{t("standardSummary", { count: selected.greenFlagCount })}</p>
        </div>
      </section>
    </main>
  );
}

export default function ContractDetailPage() {
  return (
    <ContractsProvider>
      <ContractDetailGuard>
        <ContractDetailContent />
      </ContractDetailGuard>
    </ContractsProvider>
  );
}
