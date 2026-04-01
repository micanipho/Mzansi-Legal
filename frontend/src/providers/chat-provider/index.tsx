"use client";
import { useContext, useReducer } from "react";
import { askRagQuestion } from "@/services/qa.service";
import { ChatReducer } from "./reducer";
import { INITIAL_STATE, ChatStateContext, ChatActionContext } from "./context";
import { ChatStateEnums } from "./actions";
import type { IChatMessage } from "./context";

export const ChatProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(ChatReducer, INITIAL_STATE);

  const sendMessage = async (text: string, locale?: string) => {
    const trimmed = text.trim();
    if (!trimmed) return;

    const userMsg: IChatMessage = {
      id: crypto.randomUUID(),
      type: "user",
      text: trimmed,
      status: "sent",
    };

    dispatch({ type: ChatStateEnums.CHAT_SEND_PENDING, userMsg });

    try {
      const result = await askRagQuestion({ questionText: trimmed }, locale);

      // Validate the result structure
      if (!result || typeof result !== 'object') {
        throw new Error('Invalid response from server');
      }

      const botMsg: IChatMessage = {
        id: crypto.randomUUID(),
        type: "bot",
        text: result.answerText || "No answer received.",
        status: "sent",
        citations: result.citations || [],
        isInsufficientInformation: result.isInsufficientInformation,
        answerMode: result.answerMode,
        confidenceBand: result.confidenceBand,
        clarificationQuestion: result.clarificationQuestion,
      };

      dispatch({ type: ChatStateEnums.CHAT_SEND_SUCCESS, botMsg });
    } catch (err) {
      console.error("Chat error:", err);
      const error = err instanceof Error ? err.message : "An unexpected error occurred.";
      const errorMsg: IChatMessage = {
        id: crypto.randomUUID(),
        type: "bot",
        text: error,
        status: "error",
      };
      dispatch({ type: ChatStateEnums.CHAT_SEND_ERROR, errorMsg, error });
    }
  };

  const clearMessages = () => {
    dispatch({ type: ChatStateEnums.CHAT_CLEAR });
  };

  return (
    <ChatStateContext.Provider value={state}>
      <ChatActionContext.Provider value={{ sendMessage, clearMessages }}>
        {children}
      </ChatActionContext.Provider>
    </ChatStateContext.Provider>
  );
};

export const useChatState = () => {
  const context = useContext(ChatStateContext);
  if (!context) throw new Error("useChatState must be used within ChatProvider");
  return context;
};

export const useChatAction = () => {
  const context = useContext(ChatActionContext);
  if (!context) throw new Error("useChatAction must be used within ChatProvider");
  return context;
};
