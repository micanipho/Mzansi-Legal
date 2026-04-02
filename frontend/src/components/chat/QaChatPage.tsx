"use client";

import { useLocale, useTranslations } from "next-intl";
import { useSearchParams } from "next/navigation";
import { useEffect, useEffectEvent, useRef } from "react";
import { C, fontSans } from "@/styles/theme";
import { useChatState, useChatAction } from "@/providers/chat-provider";
import ChatInput from "./ChatInput";
import ChatThread from "./ChatThread";

export default function QaChatPage() {
  const locale = useLocale();
  const t = useTranslations("chat");
  const searchParams = useSearchParams();
  const initialQuestion = searchParams.get("q") ?? "";
  const requestedConversationId = searchParams.get("conversationId");
  const lastAutoSentQuestionRef = useRef<string | null>(null);
  const lastLoadedConversationRef = useRef<string | null>(null);

  const {
    messages,
    isPending: isLoading,
    error,
    conversationId,
  } = useChatState();
  const { sendMessage, loadConversation } = useChatAction();
  const autoSendQuestion = useEffectEvent((question: string) => {
    void sendMessage(question, locale);
  });
  const hydrateConversation = useEffectEvent((nextConversationId: string) => {
    void loadConversation(nextConversationId);
  });

  useEffect(() => {
    const trimmedConversationId = requestedConversationId?.trim();

    if (!trimmedConversationId) {
      return;
    }

    if (
      lastLoadedConversationRef.current === trimmedConversationId &&
      conversationId === trimmedConversationId
    ) {
      return;
    }

    lastLoadedConversationRef.current = trimmedConversationId;
    hydrateConversation(trimmedConversationId);
  }, [conversationId, requestedConversationId]);

  useEffect(() => {
    const trimmedQuestion = initialQuestion.trim();

    if (!trimmedQuestion) {
      return;
    }

    if (
      requestedConversationId?.trim() &&
      conversationId !== requestedConversationId.trim()
    ) {
      return;
    }

    if (lastAutoSentQuestionRef.current === trimmedQuestion) {
      return;
    }

    lastAutoSentQuestionRef.current = trimmedQuestion;
    autoSendQuestion(trimmedQuestion);
  }, [conversationId, initialQuestion, requestedConversationId]);

  const handleSend = (text: string) => {
    // Allow guests to chat, no redirect
    void sendMessage(text, locale);
  };

  return (
    <main
      className="page-shell page-shell--narrow"
      style={{
        paddingBottom: 176,
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
          padding: "12px 16px calc(12px + env(safe-area-inset-bottom, 0px))",
          background: `linear-gradient(to top, ${C.bg} 60%, transparent)`,
          zIndex: 40,
        }}
      >
        <div style={{ maxWidth: 896, margin: "0 auto", width: "100%" }}>
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
