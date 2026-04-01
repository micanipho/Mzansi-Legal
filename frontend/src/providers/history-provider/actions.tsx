import type { IConversationItem } from "./context";

export enum HistoryStateEnums {
  HISTORY_FETCH_ALL_PENDING = "HISTORY_FETCH_ALL_PENDING",
  HISTORY_FETCH_ALL_SUCCESS = "HISTORY_FETCH_ALL_SUCCESS",
  HISTORY_FETCH_ALL_ERROR   = "HISTORY_FETCH_ALL_ERROR",
}

export type HistoryAction =
  | { type: HistoryStateEnums.HISTORY_FETCH_ALL_PENDING }
  | { type: HistoryStateEnums.HISTORY_FETCH_ALL_SUCCESS; items: IConversationItem[]; totalCount: number }
  | { type: HistoryStateEnums.HISTORY_FETCH_ALL_ERROR };
