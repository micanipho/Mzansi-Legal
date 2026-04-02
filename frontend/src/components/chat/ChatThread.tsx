"use client";

import { useEffect, useRef } from "react";
import { C, fontSans, R, shadowOrganic } from "@/styles/theme";
import type { IChatMessage } from "@/providers/chat-provider/context";
import ChatMessage from "./ChatMessage";

interface ChatThreadProps {
  messages: IChatMessage[];
  isLoading: boolean;
  error: string | null;
  emptyStateText?: string;
}

export default function ChatThread({
  messages,
  isLoading,
  error,
  emptyStateText,
}: ChatThreadProps) {
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 20 }}>
      {messages.length === 0 && !isLoading && (
        <div
          className="surface-card grain-panel"
          style={{
            textAlign: "center",
            color: C.mutedFg,
            marginTop: 24,
            padding: "28px 20px",
            borderRadius: R.o2,
            boxShadow: shadowOrganic,
          }}
        >
          <p style={{ fontSize: 16, fontFamily: fontSans }}>
            {emptyStateText ?? "Ask a legal question to get started."}
          </p>
        </div>
      )}

      {messages.map((message) => (
        <ChatMessage key={message.id} message={message} />
      ))}

      {isLoading && (
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
              {[0, 1, 2].map((i) => (
                <div
                  key={i}
                  style={{
                    width: 8,
                    height: 8,
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

      {error && !isLoading && (
        <div
          style={{
            background: "rgba(254,226,226,0.4)",
            border: "1px solid #FCA5A5",
            borderRadius: 18,
            padding: "14px 16px",
            fontSize: 14,
            color: "#DC2626",
            fontFamily: fontSans,
          }}
          role="alert"
        >
          {error}
        </div>
      )}

      <div ref={bottomRef} />
    </div>
  );
}
