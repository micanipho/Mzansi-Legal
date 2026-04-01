"use client";
import { useContext, useReducer } from "react";
import { getConversations } from "@/services/qaService";
import { useAuth } from "@/hooks/useAuth";
import { HistoryReducer } from "./reducer";
import { INITIAL_STATE, HistoryStateContext, HistoryActionContext } from "./context";
import { HistoryStateEnums } from "./actions";

export const HistoryProvider = ({ children }: { children: React.ReactNode }) => {
  const { user } = useAuth();
  const [state, dispatch] = useReducer(HistoryReducer, INITIAL_STATE);

  const fetchAll = async () => {
    dispatch({ type: HistoryStateEnums.HISTORY_FETCH_ALL_PENDING });
    try {
      const result = await getConversations(user?.token);
      dispatch({
        type: HistoryStateEnums.HISTORY_FETCH_ALL_SUCCESS,
        items: result.items.map((item) => ({
          conversationId: item.conversationId,
          firstQuestion: item.firstQuestion,
          questionCount: item.questionCount,
          startedAt: item.startedAt,
          locale: item.locale,
        })),
        totalCount: result.totalCount,
      });
    } catch {
      dispatch({ type: HistoryStateEnums.HISTORY_FETCH_ALL_ERROR });
    }
  };

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
  if (!context) throw new Error("useHistoryState must be used within HistoryProvider");
  return context;
};

export const useHistoryAction = () => {
  const context = useContext(HistoryActionContext);
  if (!context) throw new Error("useHistoryAction must be used within HistoryProvider");
  return context;
};
