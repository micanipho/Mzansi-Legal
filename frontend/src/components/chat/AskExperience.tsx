"use client";

import { useSearchParams } from "next/navigation";
import { useLocale, useTranslations } from "next-intl";
import { useEffect, useRef, useState } from "react";
import { Book, ChevronDown, ChevronUp, Info, Mic, Play, Send } from "lucide-react";
import { C, R, shadowOrganic, fontSans } from "@/styles/theme";
import { askQuestion, transcribeAudio, textToSpeech, type QuestionWithAnswerDto } from "@/services/qaService";

interface ChatMessage {
  role: "user" | "assistant";
  text: string;
  language?: string;
  answer?: QuestionWithAnswerDto["answer"];
  disclaimer?: string;
}

const WAVEFORM_HEIGHTS = [40, 70, 40, 100, 60, 30, 80, 50, 90, 60, 40, 70, 30, 50, 80, 40, 60, 30, 90, 50];

function getLanguageLabel(language?: string) {
  switch (language) {
    case "zu":
      return "isiZulu";
    case "st":
      return "Sesotho";
    case "af":
      return "Afrikaans";
    case "en":
      return "English";
    default:
      return language ?? "English";
  }
}

function AssistantMessage({ msg, onAskFollowUp }: { msg: ChatMessage; onAskFollowUp: (question: string) => void }) {
  const t = useTranslations("chat");
  const [citationsOpen, setCitationsOpen] = useState(false);
  const [playing, setPlaying] = useState(false);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const citations = msg.answer?.citations ?? [];
  const languageLabel = getLanguageLabel(msg.language);

  const handlePlay = async () => {
    if (playing) {
      audioRef.current?.pause();
      setPlaying(false);
      return;
    }

    try {
      const blob = await textToSpeech(msg.text, msg.language ?? "en");
      const url = URL.createObjectURL(blob);
      const audio = new Audio(url);
      audioRef.current = audio;
      audio.onended = () => {
        setPlaying(false);
        URL.revokeObjectURL(url);
      };
      await audio.play();
      setPlaying(true);
    } catch {
      setPlaying(false);
    }
  };

  return (
    <div style={{ display: "flex", justifyContent: "flex-start" }}>
      <div
        className="grain-panel"
        style={{
          background: C.card,
          border: `1px solid ${C.border}`,
          borderRadius: R.o1,
          borderBottomLeftRadius: 4,
          maxWidth: "85%",
          boxShadow: shadowOrganic,
          overflow: "hidden",
        }}
      >
        <div
          style={{
            background: C.muted,
            padding: "12px 16px",
            borderBottom: `1px solid ${C.border}`,
            display: "flex",
            alignItems: "center",
            gap: 12,
          }}
        >
          <button
            onClick={handlePlay}
            style={{
              width: 40,
              height: 40,
              borderRadius: 9999,
              background: playing ? C.fg : C.primary,
              border: "none",
              cursor: "pointer",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: C.primaryFg,
              flexShrink: 0,
            }}
            aria-label={playing ? "Stop audio" : "Play audio"}
          >
            <Play size={16} style={{ marginLeft: 2 }} />
          </button>
          <div style={{ flex: 1, display: "flex", alignItems: "center", gap: 2, height: 32 }}>
            {WAVEFORM_HEIGHTS.map((height, index) => (
              <div
                key={index}
                style={{
                  flex: 1,
                  background: "rgba(120,120,108,0.4)",
                  borderRadius: 9999,
                  height: `${height}%`,
                }}
              />
            ))}
          </div>
          <span
            style={{
              fontSize: 13,
              fontWeight: 500,
              color: C.mutedFg,
              whiteSpace: "nowrap",
              fontFamily: fontSans,
            }}
          >
            {t("listenIn", { language: languageLabel })}
          </span>
        </div>

        <div style={{ padding: "24px 32px", display: "flex", flexDirection: "column", gap: 16 }}>
          <p
            style={{
              fontSize: 17,
              color: C.fg,
              lineHeight: 1.7,
              margin: 0,
              fontFamily: fontSans,
              whiteSpace: "pre-wrap",
            }}
          >
            {msg.text}
          </p>

          {citations.length > 0 && (
            <div style={{ border: `1px solid ${C.border}`, borderRadius: 16, overflow: "hidden", marginTop: 8 }}>
              <button
                onClick={() => setCitationsOpen((open) => !open)}
                style={{
                  width: "100%",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  padding: "12px 16px",
                  background: "rgba(240,235,229,0.3)",
                  border: "none",
                  cursor: "pointer",
                  fontFamily: fontSans,
                }}
              >
                <div style={{ display: "flex", alignItems: "center", gap: 8, color: C.fg, fontWeight: 500, fontSize: 14 }}>
                  <Book size={16} color={C.primary} />
                  {t("citationsTitle")} ({citations.length} sections cited)
                </div>
                {citationsOpen ? <ChevronUp size={16} color={C.mutedFg} /> : <ChevronDown size={16} color={C.mutedFg} />}
              </button>

              {citationsOpen && (
                <div
                  style={{
                    padding: 16,
                    display: "flex",
                    flexDirection: "column",
                    gap: 16,
                    background: C.card,
                    borderTop: `1px solid ${C.border}`,
                  }}
                >
                  {citations.map((citation, index) => (
                    <div key={index} style={{ borderLeft: `3px solid ${C.primary}`, paddingLeft: 12 }}>
                      <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 4 }}>
                        <span
                          style={{
                            background: "#DBEAFE",
                            color: "#1E40AF",
                            fontSize: 11,
                            fontWeight: 700,
                            padding: "2px 8px",
                            borderRadius: 9999,
                            fontFamily: fontSans,
                          }}
                        >
                          {citation.actName}
                        </span>
                        <span style={{ fontWeight: 700, fontSize: 13, color: C.fg, fontFamily: fontSans }}>
                          {citation.section}
                        </span>
                      </div>
                      <p style={{ fontSize: 13, color: C.mutedFg, fontStyle: "italic", margin: 0, fontFamily: fontSans }}>
                        &ldquo;{citation.excerpt}&rdquo;
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {msg.disclaimer && (
            <div
              style={{
                background: "var(--ml-info-soft)",
                border: "1px solid var(--ml-info-border)",
                borderRadius: 16,
                padding: 16,
                display: "flex",
                gap: 12,
                marginTop: 8,
              }}
            >
              <Info size={20} color="#3B82F6" style={{ flexShrink: 0, marginTop: 2 }} />
              <p style={{ fontSize: 13, color: "#1E3A5F", margin: 0, fontFamily: fontSans }}>{msg.disclaimer}</p>
            </div>
          )}

          <div style={{ marginTop: 16, paddingTop: 16, borderTop: `1px solid ${C.border}` }}>
            <h4
              style={{
                fontSize: 11,
                fontWeight: 700,
                color: C.mutedFg,
                textTransform: "uppercase",
                letterSpacing: "0.08em",
                marginBottom: 12,
                fontFamily: fontSans,
              }}
            >
              {t("relatedQuestions")}
            </h4>
            <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
              {[
                "Ngingenza njani uma umnikazi wendlu evala ugesi wami?",
                "How long does a legal eviction process take?",
                "Where is my nearest Rental Housing Tribunal?",
              ].map((question) => (
                <button
                  key={question}
                  onClick={() => onAskFollowUp(question)}
                  style={{
                    textAlign: "left",
                    color: C.primary,
                    fontWeight: 500,
                    background: "transparent",
                    border: "none",
                    cursor: "pointer",
                    padding: 0,
                    fontSize: 14,
                    fontFamily: fontSans,
                  }}
                >
                  {question}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function AskExperience() {
  const locale = useLocale();
  const t = useTranslations("chat");
  const tc = useTranslations("common");
  const searchParams = useSearchParams();
  const initialQuestion = searchParams.get("q") ?? "";

  const [input, setInput] = useState(initialQuestion);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(false);
  const [conversationId, setConversationId] = useState<string | undefined>();
  const [recording, setRecording] = useState(false);
  const bottomRef = useRef<HTMLDivElement | null>(null);
  const mediaRecorder = useRef<MediaRecorder | null>(null);
  const audioChunks = useRef<Blob[]>([]);

  const handleVoice = async () => {
    if (recording) {
      mediaRecorder.current?.stop();
      setRecording(false);
      return;
    }

    if (!navigator.mediaDevices?.getUserMedia) {
      return;
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const recorder = new MediaRecorder(stream);
      audioChunks.current = [];
      recorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          audioChunks.current.push(event.data);
        }
      };
      recorder.onstop = async () => {
        stream.getTracks().forEach((track) => track.stop());
        const blob = new Blob(audioChunks.current, { type: "audio/webm" });
        try {
          const result = await transcribeAudio(blob);
          void handleSend(result.text);
        } catch {
          // ignore transcription errors and keep the user in the ask flow
        }
      };
      recorder.start();
      mediaRecorder.current = recorder;
      setRecording(true);
    } catch {
      // microphone denied or unavailable
    }
  };

  useEffect(() => {
    if (initialQuestion) {
      void handleSend(initialQuestion);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, loading]);

  const handleSend = async (text?: string) => {
    const question = (text ?? input).trim();
    if (!question || loading) {
      return;
    }

    setInput("");
    setMessages((previous) => [...previous, { role: "user", text: question }]);
    setLoading(true);

    try {
      const result = await askQuestion({ conversationId, language: locale, text: question });
      if (!conversationId) {
        setConversationId(result.conversationId);
      }
      setMessages((previous) => [
        ...previous,
        {
          role: "assistant",
          text: result.answer.text,
          language: result.language,
          answer: result.answer,
          disclaimer: result.disclaimer,
        },
      ]);
    } catch {
      setMessages((previous) => [...previous, { role: "assistant", text: tc("error") }]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main
      className="page-shell page-shell--narrow"
      style={{
        paddingBottom: 128,
        display: "flex",
        flexDirection: "column",
        gap: 24,
        fontFamily: fontSans,
      }}
      role="log"
      aria-live="polite"
    >
      {messages.length === 0 && !loading && (
        <div style={{ textAlign: "center", color: C.mutedFg, marginTop: 48 }}>
          <p style={{ fontSize: 16, fontFamily: fontSans }}>{t("emptyState")}</p>
        </div>
      )}

      {messages.map((message, index) => (
        <div key={index}>
          {message.role === "user" ? (
            <div style={{ display: "flex", justifyContent: "flex-end" }}>
              <div
                style={{
                  background: C.primary,
                  color: C.primaryFg,
                  padding: "16px 24px",
                  borderRadius: R.o2,
                  borderBottomRightRadius: 4,
                  maxWidth: "85%",
                  boxShadow: "0 1px 4px rgba(0,0,0,0.08)",
                }}
              >
                <p style={{ fontSize: 17, margin: 0, fontFamily: fontSans }}>{message.text}</p>
              </div>
            </div>
          ) : (
            <AssistantMessage msg={message} onAskFollowUp={(question) => void handleSend(question)} />
          )}
        </div>
      ))}

      {loading && (
        <div style={{ display: "flex", justifyContent: "flex-start" }}>
          <div
            className="grain-panel"
            style={{
              background: C.card,
              border: `1px solid ${C.border}`,
              borderRadius: R.o1,
              borderBottomLeftRadius: 4,
              padding: "16px 24px",
              boxShadow: shadowOrganic,
            }}
          >
            <div style={{ display: "flex", gap: 6, alignItems: "center" }}>
              {[0, 1, 2].map((index) => (
                <div
                  key={index}
                  style={{
                    width: 8,
                    height: 8,
                    borderRadius: "50%",
                    background: C.mutedFg,
                    opacity: 0.5,
                    animation: `pulse 1.2s ease-in-out ${index * 0.2}s infinite`,
                  }}
                />
              ))}
            </div>
          </div>
        </div>
      )}

      <div ref={bottomRef} />

      <div
        style={{
          position: "fixed",
          bottom: 0,
          left: 0,
          right: 0,
          padding: 16,
          background: `linear-gradient(to top, ${C.bg} 60%, transparent)`,
          zIndex: 40,
        }}
      >
        <div style={{ maxWidth: 896, margin: "0 auto" }}>
          <div
            className="grain-panel"
            style={{
              position: "relative",
              display: "flex",
              alignItems: "center",
              background: C.card,
              border: `2px solid ${C.border}`,
              borderRadius: 9999,
              padding: 8,
              boxShadow: shadowOrganic,
            }}
          >
            <input
              type="text"
              placeholder={t("inputPlaceholder")}
              value={input}
              onChange={(event) => setInput(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") {
                  void handleSend();
                }
              }}
              disabled={loading}
              style={{
                flex: 1,
                background: "transparent",
                border: "none",
                outline: "none",
                padding: "12px 24px",
                fontSize: 16,
                fontFamily: fontSans,
                color: C.fg,
                paddingRight: 96,
              }}
            />
            <div style={{ position: "absolute", right: 8, display: "flex", alignItems: "center", gap: 8 }}>
              <button
                onClick={handleVoice}
                disabled={loading}
                style={{
                  width: 40,
                  height: 40,
                  borderRadius: 9999,
                  background: recording ? "#FEE2E2" : C.muted,
                  border: "none",
                  cursor: loading ? "not-allowed" : "pointer",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  color: recording ? "#DC2626" : C.primary,
                }}
                aria-label={recording ? "Stop recording" : "Voice input"}
                aria-pressed={recording}
              >
                <Mic size={20} />
              </button>
              <button
                onClick={() => void handleSend()}
                disabled={loading}
                style={{
                  width: 40,
                  height: 40,
                  borderRadius: 9999,
                  background: C.primary,
                  border: "none",
                  cursor: loading ? "not-allowed" : "pointer",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  color: C.primaryFg,
                  opacity: loading ? 0.6 : 1,
                }}
                aria-label="Send message"
              >
                <Send size={16} style={{ marginLeft: 1 }} />
              </button>
            </div>
          </div>
          <p
            style={{
              textAlign: "center",
              fontSize: 12,
              color: C.mutedFg,
              marginTop: 12,
              fontFamily: fontSans,
              fontWeight: 500,
            }}
          >
            {t("disclaimer")}
          </p>
        </div>
      </div>
    </main>
  );
}
