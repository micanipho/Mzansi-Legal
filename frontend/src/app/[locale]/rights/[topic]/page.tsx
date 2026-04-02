"use client";

import { Empty, Skeleton, message } from "antd";
import { useLocale, useTranslations } from "next-intl";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { ArrowLeft, ArrowRight, Minus, Play, Plus, Share2 } from "lucide-react";
import {
  getRightCardRadius,
  getTopicLabelKey,
  mergeRightsProgressIds,
  readRightsProgress,
  type RightsAcademyLesson,
  type RightsAcademyTrack,
  writeRightsProgress,
} from "@/components/rights/rightsData";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import RetryNotice from "@/components/feedback/RetryNotice";
import { useOnlineStatus } from "@/hooks/useOnlineStatus";
import {
  getRightsAcademy,
  getRightsAcademyProgress,
  saveRightsAcademyProgress,
} from "@/services/faq.service";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

const LOCALE_NAMES: Record<string, string> = {
  en: "English",
  zu: "isiZulu",
  st: "Sesotho",
  af: "Afrikaans",
};
const SPEECH_LANGUAGES: Record<string, string> = {
  en: "en-ZA",
  zu: "zu-ZA",
  st: "st-ZA",
  af: "af-ZA",
};

export default function RightsTrackPage() {
  const locale = useLocale();
  const router = useRouter();
  const params = useParams<{ topic: string }>();
  const topic = Array.isArray(params?.topic)
    ? params.topic[0]
    : (params?.topic ?? "");
  const t = useTranslations("rights");
  const tc = useTranslations("categories");
  const tChat = useTranslations("chat");
  const tCommon = useTranslations("common");
  const isOnline = useOnlineStatus();

  const [tracks, setTracks] = useState<RightsAcademyTrack[]>([]);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [exploredIds, setExploredIds] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => setExploredIds(readRightsProgress()), []);

  const syncProgress = async (nextIds: string[]) => {
    writeRightsProgress(nextIds);
    setExploredIds(nextIds);

    try {
      const saved = await saveRightsAcademyProgress(nextIds);
      const merged = mergeRightsProgressIds(nextIds, saved.exploredLessonIds);
      writeRightsProgress(merged);
      setExploredIds(merged);
    } catch {
      // Keep local progress even if the server sync fails.
    }
  };

  const loadTrack = async (cancelledRef?: { current: boolean }) => {
    const cancelled = cancelledRef?.current ?? false;
    setIsLoading(true);
    setErrorMessage(null);
    try {
      const localProgress = readRightsProgress();
      const [result, progressResult] = await Promise.all([
        getRightsAcademy(locale),
        getRightsAcademyProgress(),
      ]);
      if (cancelledRef?.current || cancelled) return;
      setTracks(result.tracks);
      const mergedProgress = mergeRightsProgressIds(
        localProgress,
        progressResult.exploredLessonIds,
      );
      writeRightsProgress(mergedProgress);
      setExploredIds(mergedProgress);
    } catch {
      if (cancelledRef?.current || cancelled) return;
      setErrorMessage(tCommon("error"));
      setTracks([]);
    } finally {
      if (!cancelledRef?.current && !cancelled) setIsLoading(false);
    }
  };

  useEffect(() => {
    const cancelledRef = { current: false };
    void loadTrack(cancelledRef);
    return () => {
      cancelledRef.current = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [locale, tCommon]);

  const activeTrack = useMemo(
    () => tracks.find((track) => track.topicKey === topic) ?? null,
    [topic, tracks],
  );
  const activeTrackLabel = activeTrack
    ? getTopicLabelKey(activeTrack.topicKey)
      ? tc(getTopicLabelKey(activeTrack.topicKey) as Parameters<typeof tc>[0])
      : activeTrack.categoryName
    : "";
  const exploredCount = activeTrack
    ? activeTrack.lessons.filter((lesson) => exploredIds.includes(lesson.id))
        .length
    : 0;
  const progressPercent = activeTrack?.lessons.length
    ? Math.round((exploredCount / activeTrack.lessons.length) * 100)
    : 0;

  const handleToggle = (itemId: string) => {
    const opening = expandedId !== itemId;
    setExpandedId(opening ? itemId : null);
    if (!opening || exploredIds.includes(itemId)) return;
    const updated = [...exploredIds, itemId];
    void syncProgress(updated);
  };

  const askFollowUp = (item: RightsAcademyLesson) => {
    const query = item.askQuery || item.title;
    router.push(
      createLocalizedPath(
        locale,
        appRoutes.ask,
        `q=${encodeURIComponent(query)}`,
      ),
    );
  };

  const listenToItem = (item: RightsAcademyLesson) => {
    if (typeof window === "undefined" || !("speechSynthesis" in window))
      return void message.warning(t("listenUnsupported"));
    const utterance = new SpeechSynthesisUtterance(
      `${item.title}. ${item.explanation}`,
    );
    utterance.lang = SPEECH_LANGUAGES[locale] ?? locale;
    window.speechSynthesis.cancel();
    window.speechSynthesis.speak(utterance);
  };

  const shareItem = async (item: RightsAcademyLesson) => {
    if (typeof window === "undefined") return;
    const shareUrl = `${window.location.origin}${createLocalizedPath(locale, `${appRoutes.rights}/${topic}`)}#${item.id}`;
    try {
      if (navigator.share) {
        await navigator.share({
          title: item.title,
          text: item.summary,
          url: shareUrl,
        });
        return;
      }
      if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(shareUrl);
        return void message.success(t("shareCopied"));
      }
      void message.info(t("shareUnavailable"));
    } catch {
      void message.error(tCommon("error"));
    }
  };

  const renderLessonCard = (item: RightsAcademyLesson, index: number) => {
    const isExpanded = expandedId === item.id;
    return (
      <article
        key={item.id}
        id={item.id}
        className="surface-card grain-panel"
        style={{
          gridColumn: isExpanded ? "1 / -1" : undefined,
          padding: 24,
          borderRadius: getRightCardRadius(index),
          boxShadow: isExpanded
            ? shadowOrganic
            : "0 8px 18px rgba(59, 52, 46, 0.08)",
          display: "grid",
          gap: isExpanded ? 22 : 14,
          alignContent: "start",
        }}
      >
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            gap: 16,
            alignItems: "flex-start",
          }}
        >
          <div style={{ display: "grid", gap: 8, minWidth: 0 }}>
            <h2
              style={{
                margin: 0,
                color: C.fg,
                fontFamily: fontSerif,
                fontSize: isExpanded ? 30 : 24,
                lineHeight: 1.2,
              }}
            >
              {item.title}
            </h2>
            <span style={{ color: C.primary, fontWeight: 700, fontSize: 14 }}>
              {item.primaryCitation || item.categoryName}
            </span>
            <p
              style={{
                margin: 0,
                color: C.mutedFg,
                lineHeight: 1.7,
                display: isExpanded ? "block" : "-webkit-box",
                WebkitLineClamp: isExpanded ? "unset" : 1,
                WebkitBoxOrient: "vertical",
                overflow: "hidden",
              }}
            >
              {isExpanded ? item.explanation : item.summary}
            </p>
          </div>
          <button
            type="button"
            onClick={() => handleToggle(item.id)}
            aria-expanded={isExpanded}
            aria-label={isExpanded ? "Collapse" : "Expand"}
            style={{
              width: 44,
              height: 44,
              borderRadius: 9999,
              border: `1px solid ${C.border}`,
              background: "rgba(255,255,255,0.75)",
              display: "inline-flex",
              alignItems: "center",
              justifyContent: "center",
              color: C.fg,
              cursor: "pointer",
              flexShrink: 0,
            }}
          >
            {isExpanded ? <Minus size={18} /> : <Plus size={18} />}
          </button>
        </div>

        {isExpanded ? (
          <>
            {item.sourceQuote ? (
              <blockquote
                style={{
                  margin: 0,
                  padding: "18px 20px",
                  borderLeft: `4px solid ${C.accent}`,
                  borderRadius: "18px 14px 18px 14px",
                  background: "rgba(243, 239, 229, 0.88)",
                  color: C.mutedFg,
                  fontFamily: fontSerif,
                  fontSize: 18,
                  fontStyle: "italic",
                  lineHeight: 1.7,
                }}
              >
                {item.sourceQuote}
              </blockquote>
            ) : null}

            {item.citations.length > 0 ? (
              <div style={{ display: "grid", gap: 10 }}>
                <strong style={{ color: C.fg }}>
                  {tChat("citationsTitle")}
                </strong>
                <div style={{ display: "grid", gap: 10 }}>
                  {item.citations.map((citation) => (
                    <div
                      key={citation.id}
                      style={{
                        padding: "12px 14px",
                        borderRadius: "16px 12px 16px 12px",
                        border: `1px solid ${C.border}`,
                        background: "rgba(255,255,255,0.55)",
                        display: "grid",
                        gap: 6,
                      }}
                    >
                      <strong style={{ color: C.fg, fontSize: 14 }}>
                        {[citation.actName, citation.sectionNumber]
                          .filter(Boolean)
                          .join(", ")}
                      </strong>
                      {citation.excerpt ? (
                        <span style={{ color: C.mutedFg, lineHeight: 1.6 }}>
                          {citation.excerpt}
                        </span>
                      ) : null}
                    </div>
                  ))}
                </div>
              </div>
            ) : null}

            <div style={{ display: "flex", flexWrap: "wrap", gap: 12 }}>
              <button
                type="button"
                onClick={() => askFollowUp(item)}
                style={{
                  border: "none",
                  borderRadius: 9999,
                  background: C.primary,
                  color: C.primaryFg,
                  minHeight: 44,
                  padding: "12px 18px",
                  fontWeight: 700,
                  cursor: "pointer",
                  display: "inline-flex",
                  alignItems: "center",
                  gap: 8,
                }}
              >
                {t("askFollowUp")}
                <ArrowRight size={16} />
              </button>
              <button
                type="button"
                onClick={() => listenToItem(item)}
                style={{
                  borderRadius: 9999,
                  border: `1px solid ${C.border}`,
                  background: "rgba(255,255,255,0.6)",
                  color: C.fg,
                  minHeight: 44,
                  padding: "12px 18px",
                  fontWeight: 700,
                  cursor: "pointer",
                  display: "inline-flex",
                  alignItems: "center",
                  gap: 8,
                }}
              >
                <Play size={16} />
                {tChat("listenIn", {
                  language: LOCALE_NAMES[locale] ?? locale,
                })}
              </button>
              <button
                type="button"
                onClick={() => void shareItem(item)}
                style={{
                  borderRadius: 9999,
                  border: `1px solid ${C.border}`,
                  background: "rgba(255,255,255,0.6)",
                  color: C.fg,
                  minHeight: 44,
                  padding: "12px 18px",
                  fontWeight: 700,
                  cursor: "pointer",
                  display: "inline-flex",
                  alignItems: "center",
                  gap: 8,
                }}
              >
                <Share2 size={16} />
                {t("share")}
              </button>
            </div>
          </>
        ) : null}
      </article>
    );
  };

  return (
    <main
      className="page-shell"
      style={{
        display: "flex",
        flexDirection: "column",
        gap: 28,
        fontFamily: fontSans,
      }}
    >
      <button
        type="button"
        onClick={() =>
          router.push(createLocalizedPath(locale, appRoutes.rights))
        }
        style={{
          alignSelf: "flex-start",
          border: `1px solid ${C.border}`,
          background: "rgba(255,255,255,0.7)",
          color: C.fg,
          borderRadius: 9999,
          minHeight: 44,
          padding: "10px 16px",
          fontWeight: 700,
          display: "inline-flex",
          alignItems: "center",
          gap: 8,
          cursor: "pointer",
        }}
      >
        <ArrowLeft size={16} />
        {t("academyBack")}
      </button>

      {isLoading ? (
        <section style={{ display: "grid", gap: 18 }}>
          {Array.from({ length: 3 }).map((_, index) => (
            <article
              key={index}
              className="surface-card grain-panel"
              style={{
                padding: 28,
                borderRadius: "24px 18px 28px 20px",
                boxShadow: shadowOrganic,
              }}
            >
              <Skeleton
                active
                paragraph={{ rows: 3 }}
                title={{ width: "50%" }}
              />
            </article>
          ))}
        </section>
      ) : null}

      {!isLoading && errorMessage ? (
        <RetryNotice
          title={
            isOnline
              ? "We couldn't load this law track"
              : "You're offline right now"
          }
          description={errorMessage}
          onRetry={() => void loadTrack()}
          isOffline={!isOnline}
        />
      ) : null}

      {!isLoading && !errorMessage && !activeTrack ? (
        <section
          className="surface-card grain-panel"
          style={{
            padding: 32,
            borderRadius: "24px 18px 28px 20px",
            boxShadow: shadowOrganic,
          }}
        >
          <Empty description={false}>
            <div style={{ display: "grid", gap: 10 }}>
              <strong style={{ color: C.fg, fontSize: 18 }}>
                {t("academyTrackNotFoundTitle")}
              </strong>
              <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>
                {t("academyTrackNotFoundBody")}
              </p>
            </div>
          </Empty>
        </section>
      ) : null}

      {!isLoading && !errorMessage && activeTrack ? (
        <>
          <section
            className="surface-card grain-panel"
            style={{
              padding: 28,
              borderRadius: "30px 18px 32px 22px",
              boxShadow: shadowOrganic,
              display: "grid",
              gap: 20,
            }}
          >
            <div style={{ display: "grid", gap: 8, maxWidth: 760 }}>
              <span
                style={{
                  color: C.primary,
                  fontWeight: 700,
                  textTransform: "uppercase",
                  letterSpacing: "0.08em",
                  fontSize: 12,
                }}
              >
                {t("academyCurrentTrack")}
              </span>
              <h1
                style={{
                  margin: 0,
                  color: C.fg,
                  fontFamily: fontSerif,
                  fontSize: "clamp(2.2rem, 5vw, 3.4rem)",
                  lineHeight: 1.08,
                }}
              >
                {activeTrackLabel}
              </h1>
              <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>
                {t("academyCurrentTrackHint")}
              </p>
            </div>
            <div
              style={{
                display: "grid",
                gap: 16,
                gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
              }}
            >
              <div
                style={{
                  padding: 20,
                  borderRadius: "24px 18px 28px 16px",
                  background: "rgba(255,255,255,0.62)",
                  border: `1px solid ${C.border}`,
                  display: "grid",
                  gap: 10,
                }}
              >
                <span
                  style={{ color: C.mutedFg, fontSize: 13, fontWeight: 700 }}
                >
                  {t("academyLessonsExplored")}
                </span>
                <strong
                  style={{
                    color: C.fg,
                    fontFamily: fontSerif,
                    fontSize: 34,
                    lineHeight: 1,
                  }}
                >
                  {exploredCount}/{activeTrack.lessons.length}
                </strong>
                <span style={{ color: C.mutedFg }}>
                  {t("academyLessonSummary", {
                    explored: exploredCount,
                    total: activeTrack.lessons.length,
                  })}
                </span>
              </div>
              <div
                style={{
                  padding: 20,
                  borderRadius: "18px 26px 18px 30px",
                  background:
                    "linear-gradient(135deg, rgba(93,112,82,0.15), rgba(126,107,86,0.08))",
                  border: `1px solid ${C.border}`,
                  display: "grid",
                  gap: 10,
                }}
              >
                <span
                  style={{ color: C.mutedFg, fontSize: 13, fontWeight: 700 }}
                >
                  {t("knowledgeScore", {
                    explored: exploredCount,
                    total: activeTrack.lessons.length,
                  })}
                </span>
                <strong
                  style={{
                    color: C.fg,
                    fontFamily: fontSerif,
                    fontSize: 34,
                    lineHeight: 1,
                  }}
                >
                  {progressPercent}%
                </strong>
              </div>
            </div>
          </section>

          <section className="rights-grid">
            {activeTrack.lessons.map((item, index) =>
              renderLessonCard(item, index),
            )}
          </section>
        </>
      ) : null}
    </main>
  );
}
