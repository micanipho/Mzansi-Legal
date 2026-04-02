"use client";
import { useContext, useReducer } from "react";
import { getUserFacingErrorMessage } from "@/lib/userFacingErrors";
import { askRagQuestion, getConversation } from "@/services/qa.service";
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
      const result = await askRagQuestion(
        {
          questionText: trimmed,
          conversationId: state.conversationId ?? undefined,
        },
        locale,
      );

      // Validate the result structure
      if (!result || typeof result !== "object") {
        throw new Error("Invalid response from server");
      }

      const botMsg: IChatMessage = {
        id: crypto.randomUUID(),
        type: "bot",
        text: result.answerText || "No answer received.",
        status: "sent",
        detectedLanguageCode: result.detectedLanguageCode,
        citations: result.citations || [],
        isInsufficientInformation: result.isInsufficientInformation,
        answerMode: result.answerMode,
        confidenceBand: result.confidenceBand,
        clarificationQuestion: result.clarificationQuestion,
        requiresUrgentAttention: result.requiresUrgentAttention,
      };

      dispatch({
        type: ChatStateEnums.CHAT_SEND_SUCCESS,
        botMsg,
        conversationId: result.conversationId ?? state.conversationId ?? null,
      });
    } catch (err) {
      console.error("Chat error:", err);
      const error = getUserFacingErrorMessage(
        err,
        "We couldn't send that question just now. Please try again.",
      );
      const errorMsg: IChatMessage = {
        id: crypto.randomUUID(),
        type: "bot",
        text: error,
        status: "error",
      };
      dispatch({ type: ChatStateEnums.CHAT_SEND_ERROR, errorMsg, error });
    }
  };

  const loadConversation = async (conversationId: string) => {
    if (!conversationId) return;

    dispatch({ type: ChatStateEnums.CHAT_LOAD_PENDING });

    try {
      const conversation = await getConversation(conversationId);
      const messages: IChatMessage[] = conversation.messages.map((message) => ({
        id: message.messageId,
        type: message.type === "bot" ? "bot" : "user",
        text: message.text,
        status: "sent",
        detectedLanguageCode: message.detectedLanguageCode,
        citations: message.citations,
      }));

      dispatch({
        type: ChatStateEnums.CHAT_LOAD_SUCCESS,
        messages,
        conversationId: conversation.conversationId,
      });
    } catch (err) {
      const error = getUserFacingErrorMessage(
        err,
        "We couldn't load that conversation just now. Please try again.",
      );

      dispatch({ type: ChatStateEnums.CHAT_LOAD_ERROR, error });
    }
  };

  const clearMessages = () => {
    dispatch({ type: ChatStateEnums.CHAT_CLEAR });
  };

  return (
    <ChatStateContext.Provider value={state}>
      <ChatActionContext.Provider
        value={{ sendMessage, loadConversation, clearMessages }}
      >
        {children}
      </ChatActionContext.Provider>
    </ChatStateContext.Provider>
  );
};

export const useChatState = () => {
  const context = useContext(ChatStateContext);
  if (!context)
    throw new Error("useChatState must be used within ChatProvider");
  return context;
};

export const useChatAction = () => {
  const context = useContext(ChatActionContext);
  if (!context)
    throw new Error("useChatAction must be used within ChatProvider");
  return context;
};
