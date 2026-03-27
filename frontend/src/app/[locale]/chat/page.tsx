"use client";

import { useSearchParams } from "next/navigation";
import { useLocale } from "next-intl";
import { useEffect, useRef, useState } from "react";
import { Send, Mic, Play, Book, Info, ChevronDown, ChevronUp } from "lucide-react";
import { C, R, shadowOrganic, fontSerif, fontSans } from "@/styles/theme";
import { askQuestion, type QuestionWithAnswerDto } from "@/services/qaService";

interface ChatMessage {
  role: "user" | "assistant";
  text: string;
  language?: string;
  answer?: QuestionWithAnswerDto["answer"];
  disclaimer?: string;
}

const WAVEFORM_HEIGHTS = [40, 70, 40, 100, 60, 30, 80, 50, 90, 60, 40, 70, 30, 50, 80, 40, 60, 30, 90, 50];

function AssistantMessage({ msg }: { msg: ChatMessage }) {
  const [citationsOpen, setCitationsOpen] = useState(false);
  const citations = msg.answer?.citations ?? [];

  return (
    <div style={{ display: "flex", justifyContent: "flex-start" }}>
      <div
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
        {/* Voice playback bar */}
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
            style={{
              width: 40, height: 40,
              borderRadius: 9999,
              background: C.primary,
              border: "none",
              cursor: "pointer",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: C.primaryFg,
              flexShrink: 0,
            }}
            aria-label="Play audio"
          >
            <Play size={16} style={{ marginLeft: 2 }} />
          </button>
          <div style={{ flex: 1, display: "flex", alignItems: "center", gap: 2, height: 32 }}>
            {WAVEFORM_HEIGHTS.map((h, i) => (
              <div
                key={i}
                style={{
                  flex: 1,
                  background: "rgba(120,120,108,0.4)",
                  borderRadius: 9999,
                  height: `${h}%`,
                }}
              />
            ))}
          </div>
          <span style={{ fontSize: 13, fontWeight: 500, color: C.mutedFg, whiteSpace: "nowrap", fontFamily: fontSans }}>
            Listen in {msg.language ?? "English"}
          </span>
        </div>

        {/* Answer text */}
        <div style={{ padding: "24px 32px", display: "flex", flexDirection: "column", gap: 16 }}>
          <p style={{ fontSize: 17, color: C.fg, lineHeight: 1.7, margin: 0, fontFamily: fontSans }}>
            {msg.text}
          </p>

          {/* Citations */}
          {citations.length > 0 && (
            <div style={{ border: `1px solid ${C.border}`, borderRadius: 16, overflow: "hidden", marginTop: 8 }}>
              <button
                onClick={() => setCitationsOpen((o) => !o)}
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
                  Sources ({citations.length} sections cited)
                </div>
                {citationsOpen
                  ? <ChevronUp size={16} color={C.mutedFg} />
                  : <ChevronDown size={16} color={C.mutedFg} />}
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
                  {citations.map((c, i) => (
                    <div key={i} style={{ borderLeft: `3px solid ${C.primary}`, paddingLeft: 12 }}>
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
                          {c.actName}
                        </span>
                        <span style={{ fontWeight: 700, fontSize: 13, color: C.fg, fontFamily: fontSans }}>
                          {c.section}
                        </span>
                      </div>
                      <p style={{ fontSize: 13, color: C.mutedFg, fontStyle: "italic", margin: 0, fontFamily: fontSans }}>
                        &ldquo;{c.excerpt}&rdquo;
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Disclaimer */}
          {msg.disclaimer && (
            <div
              style={{
                background: "#EFF6FF",
                border: "1px solid #BFDBFE",
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

          {/* Related questions */}
          <div style={{ marginTop: 16, paddingTop: 16, borderTop: `1px solid ${C.border}` }}>
            <h4 style={{ fontSize: 11, fontWeight: 700, color: C.mutedFg, textTransform: "uppercase", letterSpacing: "0.08em", marginBottom: 12, fontFamily: fontSans }}>
              Related questions
            </h4>
            <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
              {[
                "Ngingenza njani uma umnikazi wendlu evala ugesi wami?",
                "How long does a legal eviction process take?",
                "Where is my nearest Rental Housing Tribunal?",
              ].map((q) => (
                <button
                  key={q}
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
                  {q}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function ChatPage() {
  const locale      = useLocale();
  const searchParams = useSearchParams();
  const initialQ    = searchParams.get("q") ?? "";

  const [input,          setInput]          = useState(initialQ);
  const [messages,       setMessages]       = useState<ChatMessage[]>([]);
  const [loading,        setLoading]        = useState(false);
  const [conversationId, setConversationId] = useState<string | undefined>();
  const bottomRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (initialQ) handleSend(initialQ);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, loading]);

  const handleSend = async (text?: string) => {
    const q = (text ?? input).trim();
    if (!q || loading) return;

    setInput("");
    setMessages((prev) => [...prev, { role: "user", text: q }]);
    setLoading(true);

    try {
      const result = await askQuestion({ conversationId, language: locale, text: q });
      if (!conversationId) setConversationId(result.conversationId);
      setMessages((prev) => [
        ...prev,
        {
          role: "assistant",
          text: result.answer.text,
          language: result.language,
          answer: result.answer,
          disclaimer: result.disclaimer,
        },
      ]);
    } catch {
      setMessages((prev) => [
        ...prev,
        { role: "assistant", text: "Sorry, something went wrong. Please try again." },
      ]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main
      style={{
        minHeight: "100vh",
        paddingTop: 96,
        paddingBottom: 128,
        paddingLeft: 16,
        paddingRight: 16,
        maxWidth: 896,
        margin: "0 auto",
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
          <p style={{ fontSize: 16, fontFamily: fontSans }}>Ask a legal or financial rights question in any language.</p>
        </div>
      )}

      {messages.map((msg, i) => (
        <div key={i}>
          {msg.role === "user" ? (
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
                <p style={{ fontSize: 17, margin: 0, fontFamily: fontSans }}>{msg.text}</p>
              </div>
            </div>
          ) : (
            <AssistantMessage msg={msg} />
          )}
        </div>
      ))}

      {loading && (
        <div style={{ display: "flex", justifyContent: "flex-start" }}>
          <div
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
              {[0, 1, 2].map((i) => (
                <div
                  key={i}
                  style={{
                    width: 8, height: 8,
                    borderRadius: "50%",
                    background: C.mutedFg,
                    opacity: 0.5,
                    animation: `pulse 1.2s ease-in-out ${i * 0.2}s infinite`,
                  }}
                />
              ))}
            </div>
          </div>
        </div>
      )}

      <div ref={bottomRef} />

      {/* ── Fixed input bar ──────────────────────────────────── */}
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
              placeholder="Buza umbuzo..."
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleSend()}
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
                style={{
                  width: 40, height: 40,
                  borderRadius: 9999,
                  background: C.muted,
                  border: "none",
                  cursor: "pointer",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  color: C.primary,
                }}
                aria-label="Voice input"
              >
                <Mic size={20} />
              </button>
              <button
                onClick={() => handleSend()}
                disabled={loading}
                style={{
                  width: 40, height: 40,
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
          <p style={{ textAlign: "center", fontSize: 12, color: C.mutedFg, marginTop: 12, fontFamily: fontSans, fontWeight: 500 }}>
            MzansiLegal provides legal information, not legal advice.
          </p>
        </div>
      </div>
    </main>
  );
}
