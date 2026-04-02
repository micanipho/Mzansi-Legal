"use client";

import { C, fontSans, R, shadowOrganic } from "@/styles/theme";
import { useTranslations } from "next-intl";
import type { IChatMessage } from "@/providers/chat-provider/context";
import CitationList from "./CitationList";
import ReactMarkdown from "react-markdown";

interface ChatMessageProps {
  message: IChatMessage;
}

function getModeStyles(answerMode?: IChatMessage["answerMode"]) {
  switch (answerMode) {
    case "cautious":
      return {
        background: "rgba(251,191,36,0.12)",
        border: "1px solid rgba(251,191,36,0.4)",
        color: "#92400e",
      };
    case "clarification":
      return {
        background: "rgba(59,130,246,0.08)",
        border: "1px solid rgba(59,130,246,0.24)",
        color: "#1d4ed8",
      };
    case "insufficient":
      return {
        background: "rgba(148,163,184,0.12)",
        border: "1px solid rgba(148,163,184,0.28)",
        color: "#334155",
      };
    default:
      return null;
  }
}

function getUrgentStyles() {
  return {
    background: "rgba(239,68,68,0.08)",
    border: "1px solid rgba(239,68,68,0.22)",
    color: "#991b1b",
  };
}

export default function ChatMessage({ message }: ChatMessageProps) {
  const t = useTranslations("chat");

  if (message.type === "user") {
    return (
      <div style={{ display: "flex", justifyContent: "flex-end" }}>
        <div
          style={{
            background: C.primary,
            color: C.primaryFg,
            padding: "14px 18px",
            borderRadius: R.o2,
            borderBottomRightRadius: 4,
            maxWidth: "min(85%, 680px)",
            boxShadow: "0 1px 4px rgba(0,0,0,0.08)",
          }}
        >
          <p style={{ fontSize: 17, margin: 0, fontFamily: fontSans }}>
            {message.text}
          </p>
        </div>
      </div>
    );
  }

  const isError = message.status === "error";
  const modeStyles = getModeStyles(message.answerMode);
  const urgentStyles = getUrgentStyles();
  const citations = message.citations ?? [];
  const hasPrimarySource = citations.some(
    (citation) => citation.sourceRole === "primary",
  );
  const hasSupportingSource = citations.some(
    (citation) => citation.sourceRole === "supporting",
  );
  const hasGuidanceCitations = citations.some(
    (citation) => citation.authorityType === "officialGuidance",
  );
  const hasBindingLawCitations = citations.some(
    (citation) => citation.authorityType === "bindingLaw",
  );
  const modeTitle =
    message.answerMode === "cautious"
      ? t("cautiousLabel")
      : message.answerMode === "clarification"
        ? t("clarificationLabel")
        : message.answerMode === "insufficient"
          ? t("insufficientLabel")
          : null;
  const modeBody =
    message.answerMode === "cautious"
      ? t("cautiousHint")
      : message.answerMode === "clarification"
        ? t("clarificationHint")
        : message.answerMode === "insufficient"
          ? t("insufficientHint")
          : null;
  const confidenceText =
    message.confidenceBand === "high"
      ? t("confidenceHigh")
      : message.confidenceBand === "medium"
        ? t("confidenceMedium")
        : message.confidenceBand === "low"
          ? t("confidenceLow")
          : null;
  const summaryBadges = [
    hasPrimarySource ? t("sourceRolePrimary") : null,
    hasSupportingSource ? t("sourceRoleSupporting") : null,
    hasGuidanceCitations ? t("authorityOfficialGuidance") : null,
  ].filter(Boolean) as string[];

  return (
    <div style={{ display: "flex", justifyContent: "flex-start" }}>
      <div
        className="grain-panel"
        style={{
          background: isError ? "rgba(254,226,226,0.4)" : C.card,
          border: `1px solid ${isError ? "#FCA5A5" : C.border}`,
          borderRadius: R.o1,
          borderBottomLeftRadius: 4,
          maxWidth: "min(85%, 680px)",
          boxShadow: shadowOrganic,
          overflow: "hidden",
          padding: "18px 20px",
          display: "flex",
          flexDirection: "column",
          gap: 12,
        }}
      >
        {message.requiresUrgentAttention && (
          <div
            role="alert"
            style={{
              ...urgentStyles,
              borderRadius: 8,
              padding: "10px 14px",
              fontSize: 13,
              lineHeight: 1.5,
              fontFamily: fontSans,
            }}
          >
            <strong style={{ display: "block", marginBottom: 4 }}>
              {t("urgentLabel")}
            </strong>
            <span>{t("urgentHint")}</span>
          </div>
        )}

        {modeStyles && modeTitle && modeBody && (
          <div
            role={message.answerMode === "clarification" ? "alert" : "status"}
            style={{
              ...modeStyles,
              borderRadius: 8,
              padding: "10px 14px",
              fontSize: 13,
              lineHeight: 1.5,
              fontFamily: fontSans,
            }}
          >
            <strong style={{ display: "block", marginBottom: 4 }}>
              {modeTitle}
            </strong>
            <span>{modeBody}</span>
          </div>
        )}

        {summaryBadges.length > 0 && (
          <div
            style={{
              display: "flex",
              flexWrap: "wrap",
              gap: 8,
            }}
          >
            {summaryBadges.map((badge) => (
              <span
                key={badge}
                style={{
                  background: "rgba(15,23,42,0.05)",
                  border: "1px solid rgba(15,23,42,0.08)",
                  borderRadius: 999,
                  color: C.mutedFg,
                  fontFamily: fontSans,
                  fontSize: 12,
                  fontWeight: 700,
                  padding: "5px 10px",
                }}
              >
                {badge}
              </span>
            ))}
          </div>
        )}

        {!isError && confidenceText && (
          <div
            style={{
              alignSelf: "flex-start",
              background: "rgba(15,23,42,0.05)",
              border: "1px solid rgba(15,23,42,0.08)",
              borderRadius: 999,
              color: C.mutedFg,
              fontFamily: fontSans,
              fontSize: 12,
              fontWeight: 600,
              padding: "6px 10px",
            }}
          >
            {t("confidenceLabel")}: {confidenceText}
          </div>
        )}

        {hasGuidanceCitations && (
          <div
            style={{
              background: "rgba(59,130,246,0.06)",
              border: "1px solid rgba(59,130,246,0.16)",
              borderRadius: 8,
              padding: "10px 14px",
              color: "#1e3a8a",
              fontFamily: fontSans,
              fontSize: 13,
              lineHeight: 1.5,
            }}
          >
            <strong style={{ display: "block", marginBottom: 4 }}>
              {t("guidanceNoticeTitle")}
            </strong>
            <span>
              {hasBindingLawCitations
                ? t("guidanceNoticeWithLaw")
                : t("guidanceNoticeGuidanceOnly")}
            </span>
          </div>
        )}

        <div
          style={{
            fontSize: 17,
            color: isError ? "#DC2626" : C.fg,
            lineHeight: 1.7,
            fontFamily: fontSans,
          }}
        >
          <ReactMarkdown
            components={{
              p: ({ children }) => (
                <p style={{ margin: "0 0 12px 0" }}>{children}</p>
              ),
              strong: ({ children }) => (
                <strong style={{ fontWeight: 600 }}>{children}</strong>
              ),
              ol: ({ children }) => (
                <ol style={{ paddingLeft: 20, margin: "8px 0" }}>{children}</ol>
              ),
              ul: ({ children }) => (
                <ul style={{ paddingLeft: 20, margin: "8px 0" }}>{children}</ul>
              ),
              li: ({ children }) => (
                <li style={{ marginBottom: 6 }}>{children}</li>
              ),
            }}
          >
            {message.text ?? ""}
          </ReactMarkdown>
        </div>

        {message.answerMode === "clarification" &&
          message.clarificationQuestion && (
            <div
              style={{
                background: "rgba(59,130,246,0.06)",
                borderLeft: "3px solid rgba(59,130,246,0.5)",
                padding: "12px 14px",
                borderRadius: 8,
                color: C.fg,
                fontFamily: fontSans,
                lineHeight: 1.6,
              }}
            >
              <strong style={{ display: "block", marginBottom: 6 }}>
                {t("clarificationQuestionLabel")}
              </strong>
              <span>{message.clarificationQuestion}</span>
            </div>
          )}

        {message.citations && message.citations.length > 0 && (
          <CitationList citations={message.citations} />
        )}
      </div>
    </div>
  );
}
