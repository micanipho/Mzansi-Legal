"use client";

import { C, fontSans, R, shadowOrganic } from "@/styles/theme";
import type { ChatMessage as ChatMessageType } from "@/hooks/useChat";
import CitationList from "./CitationList";

interface ChatMessageProps {
  message: ChatMessageType;
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
        <p
          style={{
            fontSize: 17,
            color: isError ? "#DC2626" : C.fg,
            lineHeight: 1.7,
            margin: 0,
            fontFamily: fontSans,
            whiteSpace: "pre-wrap",
          }}
        >
          {message.text}
        </p>

        {message.citations && message.citations.length > 0 && (
          <CitationList citations={message.citations} />
        )}
      </div>
    </div>
  );
}
