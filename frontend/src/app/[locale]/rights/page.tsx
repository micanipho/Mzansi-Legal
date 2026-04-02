"use client";

import { Empty, Spin, message } from "antd";
import { useLocale, useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { ArrowRight, BookOpen, GraduationCap, Minus, Play, Plus, Share2, Trophy } from "lucide-react";
import {
  getRightCardRadius,
  getTopicLabelKey,
  mergeRightsProgressIds,
  readRightsProgress,
  sortTopicKeys,
  type PublicFaqItem,
  type RightsAcademyLesson,
  type RightsAcademyTrack,
  writeRightsProgress,
} from "@/components/rights/rightsData";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import { getPublicFaqs, getRightsAcademy, getRightsAcademyProgress, saveRightsAcademyProgress } from "@/services/faq.service";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

const LOCALE_NAMES: Record<string, string> = { en: "English", zu: "isiZulu", st: "Sesotho", af: "Afrikaans" };
const SPEECH_LANGUAGES: Record<string, string> = { en: "en-ZA", zu: "zu-ZA", st: "st-ZA", af: "af-ZA" };
type RightsExplorerCard = PublicFaqItem | RightsAcademyLesson;

function topicLabel(topicKey: string, categoryName: string, tc: (key: string) => string): string {
  const key = getTopicLabelKey(topicKey);
  return key ? tc(key) : categoryName || topicKey;
}

export default function MyRightsPage() {
  const locale = useLocale();
  const router = useRouter();
  const t = useTranslations("rights");
  const tc = useTranslations("categories");
  const tChat = useTranslations("chat");
  const tCommon = useTranslations("common");

  const [faqItems, setFaqItems] = useState<PublicFaqItem[]>([]);
  const [academyTracks, setAcademyTracks] = useState<RightsAcademyTrack[]>([]);
  const [activeView, setActiveView] = useState<"academy" | "faqs">("academy");
  const [activeFilter, setActiveFilter] = useState("all");
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

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setIsLoading(true);
      setErrorMessage(null);
      try {
        const localProgress = readRightsProgress();
        const [faqResult, academyResult, progressResult] = await Promise.all([
          getPublicFaqs(locale),
          getRightsAcademy(locale),
          getRightsAcademyProgress(),
        ]);
        if (cancelled) return;
        setFaqItems(faqResult.items);
        setAcademyTracks(academyResult.tracks);
        const mergedProgress = mergeRightsProgressIds(localProgress, progressResult.exploredLessonIds);
        writeRightsProgress(mergedProgress);
        setExploredIds(mergedProgress);
      } catch {
        if (cancelled) return;
        setErrorMessage(tCommon("error"));
        setFaqItems([]);
        setAcademyTracks([]);
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [locale, tCommon]);

  const availableIds = useMemo(() => new Set([
    ...faqItems.map((item) => item.id),
    ...academyTracks.flatMap((track) => track.lessons.map((lesson) => lesson.id)),
  ]), [academyTracks, faqItems]);

  useEffect(() => {
    const nextExplored = exploredIds.filter((id) => availableIds.has(id));
    if (nextExplored.length !== exploredIds.length) {
      setExploredIds(nextExplored);
      writeRightsProgress(nextExplored);
    }
    if (expandedId && !availableIds.has(expandedId)) setExpandedId(null);
    if (typeof window === "undefined" || !window.location.hash) return;
    const hashedId = window.location.hash.slice(1);
    if (!hashedId || !availableIds.has(hashedId) || nextExplored.includes(hashedId)) return;
    setExpandedId(hashedId);
    const updated = [...nextExplored, hashedId];
    setExploredIds(updated);
    writeRightsProgress(updated);
  }, [availableIds, expandedId, exploredIds]);

  useEffect(() => () => {
    if (typeof window !== "undefined" && "speechSynthesis" in window) window.speechSynthesis.cancel();
  }, []);

  const faqTopicNames = new Map<string, string>();
  faqItems.forEach((item) => {
    if (!faqTopicNames.has(item.topicKey)) faqTopicNames.set(item.topicKey, item.categoryName);
  });

  const faqTopicKeys = sortTopicKeys(Array.from(faqTopicNames.keys()));
  const filteredFaqItems = activeFilter === "all" ? faqItems : faqItems.filter((item) => item.topicKey === activeFilter);
  const academyTrackCards = academyTracks.map((track) => {
    const exploredLessons = track.lessons.filter((lesson) => exploredIds.includes(lesson.id)).length;
    return {
      ...track,
      topicLabel: topicLabel(track.topicKey, track.categoryName, tc),
      exploredLessons,
      completionPercent: track.lessons.length ? Math.round((exploredLessons / track.lessons.length) * 100) : 0,
    };
  });
  const academyLessonIds = academyTrackCards.flatMap((track) => track.lessons.map((lesson) => lesson.id));
  const exploredAcademyLessons = academyLessonIds.filter((id) => exploredIds.includes(id)).length;
  const exploredAcademyTopics = academyTrackCards.filter((track) => track.lessons.some((lesson) => exploredIds.includes(lesson.id))).length;
  const completedTracks = academyTrackCards.filter((track) => track.lessons.length > 0 && track.lessons.every((lesson) => exploredIds.includes(lesson.id))).length;
  const progressPercent = academyTrackCards.length ? Math.round((exploredAcademyTopics / academyTrackCards.length) * 100) : 0;

  const handleToggle = (itemId: string) => {
    const opening = expandedId !== itemId;
    setExpandedId(opening ? itemId : null);
    if (!opening || exploredIds.includes(itemId)) return;
    const updated = [...exploredIds, itemId];
    void syncProgress(updated);
  };

  const askFollowUp = (item: RightsExplorerCard) => {
    const query = "askQuery" in item && item.askQuery ? item.askQuery : item.title;
    router.push(createLocalizedPath(locale, appRoutes.ask, `q=${encodeURIComponent(query)}`));
  };

  const listenToItem = (item: RightsExplorerCard) => {
    if (typeof window === "undefined" || !("speechSynthesis" in window)) return void message.warning(t("listenUnsupported"));
    const utterance = new SpeechSynthesisUtterance(`${item.title}. ${item.explanation}`);
    utterance.lang = SPEECH_LANGUAGES[locale] ?? locale;
    window.speechSynthesis.cancel();
    window.speechSynthesis.speak(utterance);
  };

  const shareItem = async (item: RightsExplorerCard) => {
    if (typeof window === "undefined") return;
    const shareUrl = `${window.location.origin}${createLocalizedPath(locale, `${appRoutes.rights}#${item.id}`)}`;
    try {
      if (navigator.share) {
        await navigator.share({ title: item.title, text: item.summary, url: shareUrl });
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

  const renderLessonCard = (item: RightsExplorerCard, index: number) => {
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
          boxShadow: isExpanded ? shadowOrganic : "0 8px 18px rgba(59, 52, 46, 0.08)",
          display: "grid",
          gap: isExpanded ? 22 : 14,
          alignContent: "start",
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", gap: 16, alignItems: "flex-start" }}>
          <div style={{ display: "grid", gap: 8, minWidth: 0 }}>
            <h2 style={{ margin: 0, color: C.fg, fontFamily: fontSerif, fontSize: isExpanded ? 30 : 24, lineHeight: 1.2 }}>
              {item.title}
            </h2>
            <span style={{ color: C.primary, fontWeight: 700, fontSize: 14 }}>{item.primaryCitation || item.categoryName}</span>
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
              width: 42,
              height: 42,
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
                <strong style={{ color: C.fg }}>{tChat("citationsTitle")}</strong>
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
                        {[citation.actName, citation.sectionNumber].filter(Boolean).join(", ")}
                      </strong>
                      {citation.excerpt ? <span style={{ color: C.mutedFg, lineHeight: 1.6 }}>{citation.excerpt}</span> : null}
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
                  padding: "12px 18px",
                  fontWeight: 700,
                  cursor: "pointer",
                  display: "inline-flex",
                  alignItems: "center",
                  gap: 8,
                }}
              >
                <Play size={16} />
                {tChat("listenIn", { language: LOCALE_NAMES[locale] ?? locale })}
              </button>
              <button
                type="button"
                onClick={() => void shareItem(item)}
                style={{
                  borderRadius: 9999,
                  border: `1px solid ${C.border}`,
                  background: "rgba(255,255,255,0.6)",
                  color: C.fg,
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
    <main className="page-shell" style={{ display: "flex", flexDirection: "column", gap: 32, fontFamily: fontSans }}>
      <section style={{ display: "grid", gap: 14, maxWidth: 760 }}>
        <h1 style={{ margin: 0, color: C.fg, fontFamily: fontSerif, fontSize: "clamp(2.3rem, 5vw, 3.5rem)" }}>{t("title")}</h1>
        <p style={{ margin: 0, color: C.mutedFg, fontSize: 17, lineHeight: 1.7 }}>{t("headerDesc")}</p>
      </section>

      <section className="surface-card grain-panel" style={{ display: "grid", gap: 18, padding: 28, borderRadius: "26px 18px 30px 22px", boxShadow: shadowOrganic }}>
        <div style={{ display: "flex", justifyContent: "space-between", gap: 24, alignItems: "flex-end", flexWrap: "wrap" }}>
          <div style={{ display: "grid", gap: 10, minWidth: 240 }}>
            <strong style={{ color: C.fg, fontSize: 16 }}>{t("knowledgeScore", { explored: exploredAcademyTopics, total: academyTrackCards.length })}</strong>
            <div style={{ height: 16, borderRadius: 9999, background: "rgba(126, 107, 86, 0.15)", overflow: "hidden" }}>
              <div style={{ width: `${progressPercent}%`, height: "100%", borderRadius: 9999, background: "linear-gradient(90deg, rgba(93,112,82,0.92), rgba(126,107,86,0.88))", transition: "width 180ms ease" }} />
            </div>
          </div>
          <div style={{ color: C.primary, fontFamily: fontSerif, fontSize: "clamp(2.3rem, 5vw, 4rem)", lineHeight: 1 }}>{progressPercent}%</div>
        </div>
      </section>

      <section className="hide-scrollbar" style={{ display: "flex", gap: 12, overflowX: "auto", paddingBottom: 4 }}>
        <button type="button" onClick={() => setActiveView("academy")} aria-pressed={activeView === "academy"} style={{ whiteSpace: "nowrap", padding: "10px 18px", borderRadius: 9999, border: `1px solid ${activeView === "academy" ? C.primary : C.border}`, background: activeView === "academy" ? C.primary : "rgba(255,255,255,0.5)", color: activeView === "academy" ? C.primaryFg : C.fg, fontWeight: 700, cursor: "pointer", display: "inline-flex", alignItems: "center", gap: 8 }}>
          <GraduationCap size={16} />
          {t("academyTab")}
        </button>
        <button type="button" onClick={() => setActiveView("faqs")} aria-pressed={activeView === "faqs"} style={{ whiteSpace: "nowrap", padding: "10px 18px", borderRadius: 9999, border: `1px solid ${activeView === "faqs" ? C.primary : C.border}`, background: activeView === "faqs" ? C.primary : "rgba(255,255,255,0.5)", color: activeView === "faqs" ? C.primaryFg : C.fg, fontWeight: 700, cursor: "pointer", display: "inline-flex", alignItems: "center", gap: 8 }}>
          <BookOpen size={16} />
          {t("faqTab")}
        </button>
      </section>

      {isLoading ? (
        <section className="surface-card grain-panel" style={{ padding: 32, borderRadius: "24px 18px 28px 20px", boxShadow: shadowOrganic, display: "flex", justifyContent: "center" }}>
          <Spin tip={t("loadingFaqs")} />
        </section>
      ) : null}

      {!isLoading && errorMessage ? (
        <section className="surface-card grain-panel" style={{ padding: 28, borderRadius: "24px 18px 28px 20px", boxShadow: shadowOrganic, color: C.destructive, fontWeight: 700 }}>
          {errorMessage}
        </section>
      ) : null}

      {!isLoading && !errorMessage && activeView === "academy" && academyTrackCards.length === 0 ? (
        <section className="surface-card grain-panel" style={{ padding: 32, borderRadius: "24px 18px 28px 20px", boxShadow: shadowOrganic }}>
          <Empty description={false}>
            <div style={{ display: "grid", gap: 10 }}>
              <strong style={{ color: C.fg, fontSize: 18 }}>{t("emptyAcademyTitle")}</strong>
              <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>{t("emptyAcademyBody")}</p>
            </div>
          </Empty>
        </section>
      ) : null}

      {!isLoading && !errorMessage && activeView === "faqs" && faqItems.length === 0 ? (
        <section className="surface-card grain-panel" style={{ padding: 32, borderRadius: "24px 18px 28px 20px", boxShadow: shadowOrganic }}>
          <Empty description={false}>
            <div style={{ display: "grid", gap: 10 }}>
              <strong style={{ color: C.fg, fontSize: 18 }}>{t("emptyFaqTitle")}</strong>
              <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>{t("emptyFaqBody")}</p>
            </div>
          </Empty>
        </section>
      ) : null}

      {!isLoading && !errorMessage && academyTrackCards.length > 0 && activeView === "academy" ? (
        <section style={{ display: "grid", gap: 24 }}>
          <section className="surface-card grain-panel" style={{ padding: 28, borderRadius: "30px 18px 32px 22px", boxShadow: shadowOrganic, display: "grid", gap: 22 }}>
            <div style={{ display: "grid", gap: 8, maxWidth: 760 }}>
              <span style={{ color: C.primary, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.08em", fontSize: 12 }}>{t("academyEyebrow")}</span>
              <h2 style={{ margin: 0, color: C.fg, fontFamily: fontSerif, fontSize: "clamp(2rem, 4vw, 3rem)", lineHeight: 1.08 }}>{t("academyTitle")}</h2>
              <p style={{ margin: 0, color: C.mutedFg, fontSize: 16, lineHeight: 1.7 }}>{t("academyIntro")}</p>
            </div>

            <div style={{ display: "grid", gap: 16, gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))" }}>
              <div style={{ padding: 20, borderRadius: "24px 18px 28px 16px", background: "rgba(255,255,255,0.62)", border: `1px solid ${C.border}`, display: "grid", gap: 10 }}>
                <span style={{ color: C.mutedFg, fontSize: 13, fontWeight: 700 }}>{t("academyLessonsExplored")}</span>
                <strong style={{ color: C.fg, fontFamily: fontSerif, fontSize: 34, lineHeight: 1 }}>{exploredAcademyLessons}/{academyLessonIds.length}</strong>
                <span style={{ color: C.mutedFg }}>{t("academyLessonSummary", { explored: exploredAcademyLessons, total: academyLessonIds.length })}</span>
              </div>
              <div style={{ padding: 20, borderRadius: "18px 26px 18px 30px", background: "rgba(255,255,255,0.62)", border: `1px solid ${C.border}`, display: "grid", gap: 10 }}>
                <span style={{ color: C.mutedFg, fontSize: 13, fontWeight: 700 }}>{t("academyTracksExplored")}</span>
                <strong style={{ color: C.fg, fontFamily: fontSerif, fontSize: 34, lineHeight: 1 }}>{completedTracks}/{academyTrackCards.length}</strong>
                <span style={{ color: C.mutedFg }}>{t("academyTrackSummary", { completed: completedTracks, total: academyTrackCards.length })}</span>
              </div>
              <div style={{ padding: 20, borderRadius: "26px 18px 18px 30px", background: "linear-gradient(135deg, rgba(93,112,82,0.15), rgba(126,107,86,0.08))", border: `1px solid ${C.border}`, display: "grid", gap: 10 }}>
                <span style={{ color: C.mutedFg, fontSize: 13, fontWeight: 700 }}>{t("knowledgeScore", { explored: exploredAcademyTopics, total: academyTrackCards.length })}</span>
                <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                  <Trophy size={22} color={C.primary} />
                  <strong style={{ color: C.fg, fontFamily: fontSerif, fontSize: 34, lineHeight: 1 }}>{progressPercent}%</strong>
                </div>
              </div>
            </div>
          </section>

          <section style={{ display: "grid", gap: 16 }}>
            <div style={{ display: "grid", gap: 6, maxWidth: 720 }}>
              <strong style={{ color: C.fg, fontSize: 18 }}>{t("academyTrackLabel")}</strong>
              <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>{t("academyTrackHint")}</p>
            </div>
            <div style={{ display: "grid", gap: 16, gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))" }}>
              {academyTrackCards.map((track, index) => {
                const isComplete = track.lessons.length > 0 && track.completionPercent === 100;
                return (
                  <article key={track.topicKey} className="surface-card grain-panel" style={{ padding: 22, borderRadius: getRightCardRadius(index), boxShadow: "0 8px 18px rgba(59, 52, 46, 0.08)", display: "grid", gap: 16, border: `1px solid ${C.border}` }}>
                    <div style={{ display: "grid", gap: 8 }}>
                      <span style={{ color: C.primary, fontWeight: 700, fontSize: 13 }}>{track.lessons[0]?.lawShortName || track.topicLabel}</span>
                      <h3 style={{ margin: 0, color: C.fg, fontFamily: fontSerif, fontSize: 28, lineHeight: 1.1 }}>{track.topicLabel}</h3>
                      <span style={{ color: C.mutedFg }}>{t("academyLessonProgress", { explored: track.exploredLessons, total: track.lessons.length })}</span>
                    </div>
                    <div style={{ display: "grid", gap: 10 }}>
                      <div style={{ height: 12, borderRadius: 9999, background: "rgba(126, 107, 86, 0.14)", overflow: "hidden" }}>
                        <div style={{ width: `${track.completionPercent}%`, height: "100%", borderRadius: 9999, background: "linear-gradient(90deg, rgba(93,112,82,0.92), rgba(190,120,81,0.76))", transition: "width 180ms ease" }} />
                      </div>
                      <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center", flexWrap: "wrap" }}>
                        <span style={{ color: C.mutedFg, fontSize: 14 }}>{t("academyLessonCount", { count: track.lessons.length })}</span>
                        <span style={{ color: isComplete ? C.primary : C.mutedFg, fontWeight: 700, fontSize: 14 }}>{isComplete ? t("academyTrackComplete") : `${track.completionPercent}%`}</span>
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => router.push(createLocalizedPath(locale, `${appRoutes.rights}/${track.topicKey}`))}
                      style={{ border: "none", borderRadius: 9999, background: C.primary, color: C.primaryFg, padding: "12px 18px", fontWeight: 700, cursor: "pointer", display: "inline-flex", alignItems: "center", justifyContent: "center", gap: 8 }}
                    >
                      {track.exploredLessons > 0 ? t("academyContinueTrack") : t("academyOpenTrack")}
                      <ArrowRight size={16} />
                    </button>
                  </article>
                );
              })}
            </div>
          </section>
        </section>
      ) : null}

      {!isLoading && !errorMessage && filteredFaqItems.length > 0 && activeView === "faqs" ? (
        <section style={{ display: "grid", gap: 18 }}>
          <section className="hide-scrollbar" style={{ display: "flex", gap: 12, overflowX: "auto", paddingBottom: 4 }}>
            <button type="button" onClick={() => setActiveFilter("all")} aria-pressed={activeFilter === "all"} style={{ whiteSpace: "nowrap", padding: "10px 18px", borderRadius: 9999, border: `1px solid ${activeFilter === "all" ? C.primary : C.border}`, background: activeFilter === "all" ? C.primary : "rgba(255,255,255,0.5)", color: activeFilter === "all" ? C.primaryFg : C.fg, fontWeight: 700, cursor: "pointer" }}>
              {t("allCategories")}
            </button>
            {faqTopicKeys.map((topicKey) => {
              const key = getTopicLabelKey(topicKey);
              const label = key ? tc(key as Parameters<typeof tc>[0]) : faqTopicNames.get(topicKey) ?? topicKey;
              return (
                <button key={topicKey} type="button" onClick={() => setActiveFilter(topicKey)} aria-pressed={activeFilter === topicKey} style={{ whiteSpace: "nowrap", padding: "10px 18px", borderRadius: 9999, border: `1px solid ${activeFilter === topicKey ? C.primary : C.border}`, background: activeFilter === topicKey ? C.primary : "rgba(255,255,255,0.5)", color: activeFilter === topicKey ? C.primaryFg : C.fg, fontWeight: 700, cursor: "pointer" }}>
                  {label}
                </button>
              );
            })}
          </section>
          <section className="rights-grid">{filteredFaqItems.map((item, index) => renderLessonCard(item, index))}</section>
        </section>
      ) : null}
    </main>
  );
}
