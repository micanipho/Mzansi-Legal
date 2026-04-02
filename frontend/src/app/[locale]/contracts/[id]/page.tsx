"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { useLocale, useTranslations } from "next-intl";
import {
  AlertCircle,
  AlertTriangle,
  ArrowLeft,
  CheckCircle,
  MessageSquareQuote,
  SendHorizonal,
} from "lucide-react";
import type { ContractFollowUpAnswer } from "@/components/contracts/contractData";
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
import { askContractQuestion } from "@/services/contract.service";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { C, fontSans, fontSerif, R, shadowOrganic } from "@/styles/theme";
import ContractDetailGuard from "./ContractDetailGuard";

type FollowUpMessage =
  | { id: string; role: "user"; text: string }
  | { id: string; role: "assistant"; text: string; answer: ContractFollowUpAnswer };

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

function getScoreRing(score: number) {
  const clamped = Math.max(0, Math.min(score, 100));
  const color = clamped >= 75 ? C.primary : clamped >= 55 ? C.secondary : C.destructive;

  return {
    color,
    background: `conic-gradient(${color} 0deg ${clamped * 3.6}deg, rgba(126, 107, 86, 0.18) ${clamped * 3.6}deg 360deg)`,
  };
}

function formatContractTime(value: string, locale: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(locale, {
    hour: "numeric",
    minute: "2-digit",
  }).format(date);
}

function getChatModeLabel(
  answerMode: ContractFollowUpAnswer["answerMode"],
  t: ReturnType<typeof useTranslations>
) {
  switch (answerMode) {
    case "direct":
      return t("followUpDirect");
    case "cautious":
      return t("followUpCautious");
    default:
      return t("followUpInsufficient");
  }
}

function getConfidenceLabel(confidenceBand: ContractFollowUpAnswer["confidenceBand"], tChat: ReturnType<typeof useTranslations>) {
  switch (confidenceBand) {
    case "high":
      return tChat("confidenceHigh");
    case "medium":
      return tChat("confidenceMedium");
    default:
      return tChat("confidenceLow");
  }
}

function getAuthorityLabel(authorityType: string, tChat: ReturnType<typeof useTranslations>) {
  return authorityType === "officialGuidance"
    ? tChat("authorityOfficialGuidance")
    : tChat("authorityBindingLaw");
}

function getSourceRoleLabel(sourceRole: string, tChat: ReturnType<typeof useTranslations>) {
  return sourceRole === "supporting"
    ? tChat("sourceRoleSupporting")
    : tChat("sourceRolePrimary");
}

function ContractDetailContent() {
  const { id } = useParams<{ id: string }>();
  const locale = useLocale();
  const t = useTranslations("contracts");
  const tChat = useTranslations("chat");
  const { selected, isPending, isError, errorMessage } = useContractsState();
  const { fetchById } = useContractsAction();
  const [followUpQuestion, setFollowUpQuestion] = useState("");
  const [followUpMessages, setFollowUpMessages] = useState<FollowUpMessage[]>([]);
  const [isFollowUpPending, setIsFollowUpPending] = useState(false);
  const [followUpError, setFollowUpError] = useState<string | null>(null);

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
  const scoreRing = getScoreRing(selected.healthScore);
  const groupedFlags = groupFlags(selected.flags);
  const strengths = selected.strengths.length > 0 ? selected.strengths : groupedFlags.standardFlags;

  const handleFollowUpSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const nextQuestion = followUpQuestion.trim();
    if (!id || !nextQuestion) {
      return;
    }

    setFollowUpQuestion("");
    setFollowUpError(null);
    setIsFollowUpPending(true);
    setFollowUpMessages((current) => [
      ...current,
      { id: `user-${Date.now()}`, role: "user", text: nextQuestion },
    ]);

    try {
      const answer = await askContractQuestion(id, nextQuestion, locale);
      setFollowUpMessages((current) => [
        ...current,
        {
          id: `assistant-${Date.now()}`,
          role: "assistant",
          text: answer.answerText,
          answer,
        },
      ]);
    } catch (error) {
      setFollowUpError(error instanceof Error ? error.message : t("followUpError"));
    } finally {
      setIsFollowUpPending(false);
    }
  };

  return (
    <main className="page-shell page-shell--narrow" style={{ display: "flex", flexDirection: "column", gap: 32 }}>
      <style jsx global>{`
        @keyframes contractBlobFloat {
          0%,
          100% {
            transform: rotate(-7deg) translateY(0);
          }
          50% {
            transform: rotate(-3deg) translateY(-6px);
          }
        }
      `}</style>

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

      <section style={{ display: "flex", gap: 28, alignItems: "stretch", flexWrap: "wrap" }} className="md-flex-row">
        <article
          className="surface-card grain-panel"
          style={{
            flex: "0 0 250px",
            minHeight: 260,
            borderRadius: R.o1,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            boxShadow: shadowOrganic,
            background: "linear-gradient(180deg, rgba(255,255,255,0.92), rgba(240,233,222,0.96))",
          }}
        >
          <div
            style={{
              width: 220,
              height: 220,
              padding: 14,
              borderRadius: "42% 58% 47% 53% / 44% 39% 61% 56%",
              background: scoreRing.background,
              border: `2px solid rgba(168, 84, 72, 0.35)`,
              boxShadow: "0 20px 45px rgba(87, 61, 45, 0.18)",
              animation: "contractBlobFloat 8s ease-in-out infinite",
            }}
          >
            <div
              style={{
                width: "100%",
                height: "100%",
                borderRadius: "46% 54% 59% 41% / 43% 58% 42% 57%",
                background: "linear-gradient(180deg, rgba(246,241,230,0.98), rgba(234,226,214,0.94))",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                flexDirection: "column",
                textAlign: "center",
              }}
            >
              <span style={{ color: C.mutedFg, fontSize: 12, fontWeight: 800, letterSpacing: "0.08em", textTransform: "uppercase" }}>
                {t("healthScore")}
              </span>
              <strong style={{ fontFamily: fontSerif, fontSize: 64, lineHeight: 1, color: scoreRing.color }}>
                {selected.healthScore}
              </strong>
              <span style={{ color: C.mutedFg, fontWeight: 700 }}>{t("scoreSuffix")}</span>
            </div>
          </div>
        </article>

        <article style={{ flex: 1, minWidth: 280, display: "flex", flexDirection: "column", gap: 22 }}>
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
          </div>

          <div>
            <h1 style={{ margin: "0 0 12px", fontFamily: fontSerif, fontSize: "clamp(2.2rem, 4vw, 3.6rem)", color: C.fg }}>
              {selected.displayTitle}
            </h1>
            <p style={{ margin: 0, color: C.mutedFg, fontSize: 17, lineHeight: 1.7 }}>{selected.summary}</p>
          </div>

          <article
            className="surface-card grain-panel"
            style={{
              borderRadius: 24,
              padding: 22,
              background: "linear-gradient(180deg, rgba(241, 233, 221, 0.92), rgba(228, 217, 203, 0.96))",
              boxShadow: shadowOrganic,
              display: "grid",
              gap: 16,
            }}
          >
            <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
              <div>
                <strong style={{ display: "block", color: C.fg, marginBottom: 4 }}>{t("uploadedOn")}</strong>
                <span style={{ color: C.mutedFg }}>{formatContractDate(selected.analysedAt, locale)}</span>
              </div>
              <div>
                <strong style={{ display: "block", color: C.fg, marginBottom: 4 }}>{t("analysedAt")}</strong>
                <span style={{ color: C.mutedFg }}>{formatContractTime(selected.analysedAt, locale)}</span>
              </div>
            </div>
            <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
              {[
                getContractTypeLabel(selected.contractType),
                selected.pageCount ? t("pageCountValue", { count: selected.pageCount }) : t("pageCountUnknown"),
                `${t("languageTag")} ${selected.language.toUpperCase()}`,
              ].map((tag) => (
                <span
                  key={tag}
                  style={{
                    padding: "7px 12px",
                    borderRadius: 9999,
                    background: "rgba(255,255,255,0.68)",
                    border: `1px solid ${C.border}`,
                    color: C.fg,
                    fontWeight: 700,
                    fontSize: 12,
                  }}
                >
                  {tag}
                </span>
              ))}
            </div>
          </article>
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
        style={{
          borderRadius: R.o2,
          padding: 28,
          boxShadow: shadowOrganic,
          display: "flex",
          flexDirection: "column",
          gap: 16,
          background: "linear-gradient(180deg, rgba(237,230,219,0.94), rgba(225,214,198,0.98))",
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <MessageSquareQuote size={20} color={C.secondary} />
          <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28 }}>{t("plainSummary")}</h2>
        </div>
        <p style={{ margin: 0, color: C.fg, lineHeight: 1.7 }}>{selected.summary}</p>
      </section>

      <section style={{ display: "grid", gap: 20 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <CheckCircle size={20} color={C.primary} />
          <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28 }}>{t("whatGreat")}</h2>
        </div>
        {strengths.length === 0 ? (
          <p style={{ margin: 0, color: C.mutedFg }}>{t("noGreatPoints")}</p>
        ) : null}
        {strengths.map((flag) => (
          <article
            key={`${flag.title}-${flag.clauseText}`}
            className="surface-card grain-panel"
            style={{ borderRadius: 24, padding: 24, borderLeft: `4px solid ${C.primary}`, boxShadow: shadowOrganic }}
          >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: C.primary, fontFamily: fontSans }}>{flag.title}</h3>
            <p style={{ margin: "0 0 12px", lineHeight: 1.7, color: C.fg }}>{flag.description}</p>
            <p style={{ margin: 0, lineHeight: 1.7, color: C.mutedFg }}>{flag.clauseText}</p>
          </article>
        ))}
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
            style={{ borderRadius: 24, padding: 24, borderLeft: `5px solid ${C.destructive}`, boxShadow: shadowOrganic }}
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
            style={{ borderRadius: 24, padding: 24, borderLeft: `5px solid ${C.secondary}`, boxShadow: shadowOrganic }}
          >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: C.secondary, fontFamily: fontSans }}>{flag.title}</h3>
            <p style={{ margin: "0 0 12px", lineHeight: 1.7, color: C.fg }}>{flag.description}</p>
            <p style={{ margin: "0 0 12px", lineHeight: 1.7, color: C.mutedFg }}>{flag.clauseText}</p>
            {flag.legislationCitation ? (
              <span
                style={{
                  display: "inline-flex",
                  padding: "5px 12px",
                  borderRadius: 9999,
                  background: "rgba(193, 140, 93, 0.12)",
                  color: C.secondary,
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

      <section
        className="surface-card grain-panel"
        style={{
          borderRadius: 24,
          padding: 24,
          display: "flex",
          alignItems: "center",
          gap: 16,
          boxShadow: shadowOrganic,
          borderLeft: `5px solid ${C.primary}`,
        }}
      >
        <CheckCircle size={22} color={C.primary} />
        <div>
          <h2 style={{ margin: "0 0 6px", fontFamily: fontSans, fontSize: 18, color: C.primary }}>{t("allStandard")}</h2>
          <p style={{ margin: 0, lineHeight: 1.6, color: C.fg }}>{t("standardSummary", { count: selected.greenFlagCount })}</p>
        </div>
      </section>

      <section
        className="surface-card grain-panel"
        style={{
          borderRadius: 28,
          padding: 28,
          boxShadow: shadowOrganic,
          display: "grid",
          gap: 20,
          background: "linear-gradient(180deg, rgba(252,250,245,0.96), rgba(239,232,221,0.96))",
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <MessageSquareQuote size={20} color={C.secondary} />
          <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28 }}>{t("followUp")}</h2>
        </div>

        {followUpMessages.length === 0 ? (
          <p style={{ margin: 0, color: C.mutedFg }}>{t("followUpEmpty")}</p>
        ) : null}

        <div style={{ display: "grid", gap: 16 }}>
          {followUpMessages.map((message) =>
            message.role === "user" ? (
              <article
                key={message.id}
                style={{
                  justifySelf: "end",
                  maxWidth: "min(680px, 100%)",
                  padding: "14px 18px",
                  borderRadius: 22,
                  background: "rgba(93, 112, 82, 0.12)",
                  color: C.fg,
                }}
              >
                {message.text}
              </article>
            ) : (
              <article
                key={message.id}
                className="surface-card grain-panel"
                style={{
                  borderRadius: 24,
                  padding: 22,
                  boxShadow: shadowOrganic,
                  display: "grid",
                  gap: 14,
                }}
              >
                <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                  <span
                    style={{
                      padding: "5px 12px",
                      borderRadius: 9999,
                      background: "rgba(193, 140, 93, 0.12)",
                      color: C.secondary,
                      fontWeight: 700,
                      fontSize: 12,
                    }}
                  >
                    {getChatModeLabel(message.answer.answerMode, t)}
                  </span>
                  <span
                    style={{
                      padding: "5px 12px",
                      borderRadius: 9999,
                      background: "rgba(93, 112, 82, 0.1)",
                      color: C.primary,
                      fontWeight: 700,
                      fontSize: 12,
                    }}
                  >
                    {tChat("confidenceLabel")}: {getConfidenceLabel(message.answer.confidenceBand, tChat)}
                  </span>
                </div>

                <p style={{ margin: 0, lineHeight: 1.75, color: C.fg }}>{message.text}</p>

                {message.answer.requiresUrgentAttention ? (
                  <p style={{ margin: 0, color: C.destructive, fontWeight: 700 }}>{tChat("urgentHint")}</p>
                ) : null}

                {message.answer.contractExcerpts.length > 0 ? (
                  <div style={{ display: "grid", gap: 10 }}>
                    <strong style={{ color: C.fg }}>{t("followUpContractExcerpts")}</strong>
                    {message.answer.contractExcerpts.map((excerpt) => (
                      <article
                        key={excerpt}
                        style={{
                          padding: "14px 16px",
                          borderRadius: 18,
                          background: "rgba(126, 107, 86, 0.08)",
                          color: C.mutedFg,
                          lineHeight: 1.7,
                        }}
                      >
                        {excerpt}
                      </article>
                    ))}
                  </div>
                ) : null}

                {message.answer.citations.length > 0 ? (
                  <div style={{ display: "grid", gap: 10 }}>
                    <strong style={{ color: C.fg }}>{t("followUpSources")}</strong>
                    {message.answer.citations.map((citation) => (
                      <article
                        key={`${citation.sourceTitle}-${citation.sourceLocator}-${citation.excerpt}`}
                        style={{
                          padding: "16px 18px",
                          borderRadius: 18,
                          background: "rgba(255,255,255,0.7)",
                          border: `1px solid ${C.border}`,
                          display: "grid",
                          gap: 8,
                        }}
                      >
                        <div style={{ display: "flex", gap: 8, flexWrap: "wrap", alignItems: "center" }}>
                          <strong style={{ color: C.fg }}>{citation.sourceTitle}</strong>
                          <span style={{ color: C.mutedFg }}>{citation.sourceLocator}</span>
                        </div>
                        <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                          <span style={{ color: C.secondary, fontSize: 12, fontWeight: 700 }}>
                            {getAuthorityLabel(citation.authorityType, tChat)}
                          </span>
                          <span style={{ color: C.mutedFg, fontSize: 12, fontWeight: 700 }}>
                            {getSourceRoleLabel(citation.sourceRole, tChat)}
                          </span>
                        </div>
                        <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>{citation.excerpt}</p>
                      </article>
                    ))}
                  </div>
                ) : null}
              </article>
            )
          )}
        </div>

        <form onSubmit={handleFollowUpSubmit} style={{ display: "grid", gap: 12 }}>
          <textarea
            value={followUpQuestion}
            onChange={(event) => setFollowUpQuestion(event.target.value)}
            placeholder={t("followUpPlaceholder")}
            rows={4}
            style={{
              width: "100%",
              borderRadius: 22,
              border: `1px solid ${C.border}`,
              padding: "16px 18px",
              fontFamily: fontSans,
              fontSize: 15,
              resize: "vertical",
              background: "rgba(255,255,255,0.76)",
              color: C.fg,
            }}
          />
          {followUpError ? (
            <p style={{ margin: 0, color: C.destructive, fontWeight: 600 }}>{followUpError}</p>
          ) : null}
          <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap", alignItems: "center" }}>
            <span style={{ color: C.mutedFg, fontSize: 13 }}>{t("followUpHint")}</span>
            <button
              type="submit"
              disabled={isFollowUpPending || !followUpQuestion.trim()}
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: 8,
                padding: "12px 18px",
                borderRadius: 9999,
                border: "none",
                background: C.primary,
                color: C.primaryFg,
                fontWeight: 700,
                cursor: isFollowUpPending ? "progress" : "pointer",
                opacity: isFollowUpPending || !followUpQuestion.trim() ? 0.75 : 1,
              }}
            >
              <SendHorizonal size={16} />
              {isFollowUpPending ? t("followUpLoading") : t("followUpSend")}
            </button>
          </div>
        </form>
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
