"use client";

import { C, fontSans, R, shadowOrganic } from "@/styles/theme";
import type { ChatMessage as ChatMessageType } from "@/hooks/useChat";
import CitationList from "./CitationList";
import ReactMarkdown from "react-markdown";

interface ChatMessageProps {
  message: ChatMessageType;
}

const DISCLAIMER_PREFIX = "⚠️ *No matching legislation";

function splitDisclaimer(text: string): { disclaimer: string | null; body: string } {
  if (!text.startsWith(DISCLAIMER_PREFIX)) return { disclaimer: null, body: text };
  const separatorIndex = text.indexOf("*\n\n");
  if (separatorIndex === -1) return { disclaimer: null, body: text };
  const disclaimer = text.slice(3, separatorIndex + 1); // strip ⚠️ and surrounding *
  const body = text.slice(separatorIndex + 3);
  return { disclaimer, body };
}

export default function ChatMessage({ message }: ChatMessageProps) {
  if (message.type === "user") {
    return (
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
    );
  }

  const isError = message.status === "error";
  const { disclaimer, body } = splitDisclaimer(message.text ?? "");

  return (
    <div style={{ display: "flex", justifyContent: "flex-start" }}>
      <div
        className="grain-panel"
        style={{
          background: isError ? "rgba(254,226,226,0.4)" : C.card,
          border: `1px solid ${isError ? "#FCA5A5" : C.border}`,
          borderRadius: R.o1,
          borderBottomLeftRadius: 4,
          maxWidth: "85%",
          boxShadow: shadowOrganic,
          overflow: "hidden",
          padding: "24px 32px",
          display: "flex",
          flexDirection: "column",
          gap: 12,
        }}
      >
        {disclaimer && (
          <div
            style={{
              background: "rgba(251,191,36,0.12)",
              border: "1px solid rgba(251,191,36,0.4)",
              borderRadius: 8,
              padding: "10px 14px",
              fontSize: 13,
              color: "#92400e",
              lineHeight: 1.5,
              fontFamily: fontSans,
            }}
          >
            ⚠️ {disclaimer}
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
            {body}
          </ReactMarkdown>
        </div>

        {message.citations && message.citations.length > 0 && (
          <CitationList citations={message.citations} />
        )}
      </div>
    </div>
  );
}
