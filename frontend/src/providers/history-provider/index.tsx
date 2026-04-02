"use client";
import { useContext, useEffect, useEffectEvent, useReducer } from "react";
import { getConversation, getConversations } from "@/services/qaService";
import { useAuth } from "@/hooks/useAuth";
import { getUserFacingErrorMessage } from "@/lib/userFacingErrors";
import { HistoryReducer } from "./reducer";
import {
  INITIAL_STATE,
  HistoryStateContext,
  HistoryActionContext,
} from "./context";
import { HistoryStateEnums } from "./actions";

export const HistoryProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const { user, isLoading } = useAuth();
  const [state, dispatch] = useReducer(HistoryReducer, INITIAL_STATE);

  const fetchAll = async () => {
    dispatch({ type: HistoryStateEnums.HISTORY_FETCH_ALL_PENDING });
    try {
      const result = await getConversations(user?.token);
      const detailedItems = await Promise.all(
        result.items.map(async (item) => {
          try {
            const conversation = await getConversation(
              item.conversationId,
              user?.token,
            );

            return {
              conversationId: item.conversationId,
              firstQuestion: item.firstQuestion,
              questionCount: item.questionCount,
              startedAt: item.startedAt,
              locale: item.locale,
              messages: conversation.messages,
            };
          } catch {
            return {
              conversationId: item.conversationId,
              firstQuestion: item.firstQuestion,
              questionCount: item.questionCount,
              startedAt: item.startedAt,
              locale: item.locale,
              messages: [],
            };
          }
        }),
      );

      dispatch({
        type: HistoryStateEnums.HISTORY_FETCH_ALL_SUCCESS,
        items: detailedItems,
        totalCount: result.totalCount,
      });
    } catch (error) {
      dispatch({
        type: HistoryStateEnums.HISTORY_FETCH_ALL_ERROR,
        errorMessage: getUserFacingErrorMessage(
          error,
          "We couldn't load your conversation history. Please try again.",
        ),
      });
    }
  };
  const fetchAllOnReady = useEffectEvent(() => {
    void fetchAll();
  });

  useEffect(() => {
    if (isLoading || !user) {
      return;
    }

    fetchAllOnReady();
  }, [isLoading, user]);

  return (
    <HistoryStateContext.Provider value={state}>
      <HistoryActionContext.Provider value={{ fetchAll }}>
        {children}
      </HistoryActionContext.Provider>
    </HistoryStateContext.Provider>
  );
};

export const useHistoryState = () => {
  const context = useContext(HistoryStateContext);
  if (!context)
    throw new Error("useHistoryState must be used within HistoryProvider");
  return context;
};

export const useHistoryAction = () => {
  const context = useContext(HistoryActionContext);
  if (!context)
    throw new Error("useHistoryAction must be used within HistoryProvider");
  return context;
};
