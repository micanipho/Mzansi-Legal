"use client";

import { useCallback, useState } from "react";
import {
  askRagQuestion,
  type RagAnswerMode,
  type RagCitationDto,
  type RagConfidenceBand,
} from "@/services/qa.service";

export interface ChatMessage {
  id: string;
  type: "user" | "bot";
  text: string;
  status: "sending" | "sent" | "error";
  citations?: RagCitationDto[];
  isInsufficientInformation?: boolean;
  answerMode?: RagAnswerMode;
  confidenceBand?: RagConfidenceBand;
  clarificationQuestion?: string | null;
}

export interface UseChatReturn {
  messages: ChatMessage[];
  isLoading: boolean;
  error: string | null;
  sendMessage: (questionText: string, locale?: string) => Promise<void>;
}

export function useChat(): UseChatReturn {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sendMessage = useCallback(async (questionText: string, locale?: string) => {
    const trimmed = questionText.trim();
    if (!trimmed) return;

    const userMessage: ChatMessage = {
      id: crypto.randomUUID(),
      type: "user",
      text: trimmed,
      status: "sent",
    };

    setMessages((prev) => [...prev, userMessage]);
    setIsLoading(true);
    setError(null);

    try {
      const result = await askRagQuestion({ questionText: trimmed }, locale);

      const botMessage: ChatMessage = {
        id: crypto.randomUUID(),
        type: "bot",
        text: result.answerText ?? "",
        status: "sent",
        citations: result.citations,
        isInsufficientInformation: result.isInsufficientInformation,
        answerMode: result.answerMode,
        confidenceBand: result.confidenceBand,
        clarificationQuestion: result.clarificationQuestion,
      };

      setMessages((prev) => [...prev, botMessage]);
    } catch (err) {
      const message = err instanceof Error ? err.message : "An unexpected error occurred.";
      setError(message);
      setMessages((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          type: "bot",
          text: message,
          status: "error",
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  return { messages, isLoading, error, sendMessage };
}
