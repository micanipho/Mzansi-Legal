"use client";
import { createContext } from "react";

export interface IConversationItem {
  conversationId: string;
  firstQuestion: string;
  questionCount: number;
  startedAt: string;
  locale: string;
}

export interface IHistoryStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage: string | null;
  items: IConversationItem[];
  totalCount: number;
}

export interface IHistoryActionContext {
  fetchAll: () => Promise<void>;
}

export const INITIAL_STATE: IHistoryStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  errorMessage: null,
  items: [],
  totalCount: 0,
};

export const HistoryStateContext =
  createContext<IHistoryStateContext>(INITIAL_STATE);
export const HistoryActionContext = createContext<IHistoryActionContext>({
  fetchAll: async () => {},
});
