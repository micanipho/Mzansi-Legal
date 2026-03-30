"use client";

import { useLocale, useTranslations } from "next-intl";
import { useSearchParams } from "next/navigation";
import { useEffect } from "react";
import { C, fontSans } from "@/styles/theme";
import { useChat } from "@/hooks/useChat";
import ChatInput from "./ChatInput";
import ChatThread from "./ChatThread";

export default function QaChatPage() {
  const locale = useLocale();
  const t = useTranslations("chat");
  const searchParams = useSearchParams();
  const initialQuestion = searchParams.get("q") ?? "";

  const { messages, isLoading, error, sendMessage } = useChat();

  useEffect(() => {
    if (initialQuestion) {
      void sendMessage(initialQuestion, locale);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSend = (text: string) => {
    void sendMessage(text, locale);
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
      <ChatThread
        messages={messages}
        isLoading={isLoading}
        error={error}
        emptyStateText={t("emptyState")}
      />

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
          <ChatInput
            onSend={handleSend}
            disabled={isLoading}
            placeholder={t("inputPlaceholder")}
          />
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
