"use client";
import { createContext } from "react";
import type {
  RagAnswerMode,
  RagCitationDto,
  RagConfidenceBand,
} from "@/services/qa.service";

export interface IChatMessage {
  id: string;
  type: "user" | "bot";
  text: string;
  status: "sending" | "sent" | "error";
  detectedLanguageCode?: string;
  citations?: RagCitationDto[];
  isInsufficientInformation?: boolean;
  answerMode?: RagAnswerMode;
  confidenceBand?: RagConfidenceBand;
  clarificationQuestion?: string | null;
  requiresUrgentAttention?: boolean;
}

export interface IChatStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  messages: IChatMessage[];
  error: string | null;
}

export interface IChatActionContext {
  sendMessage: (text: string, locale?: string) => void;
  clearMessages: () => void;
}

export const INITIAL_STATE: IChatStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  messages: [],
  error: null,
};

export const ChatStateContext = createContext<IChatStateContext>(INITIAL_STATE);
export const ChatActionContext = createContext<IChatActionContext>({
  sendMessage: () => {},
  clearMessages: () => {},
});
