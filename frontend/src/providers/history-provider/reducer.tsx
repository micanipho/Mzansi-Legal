import { INITIAL_STATE, type IHistoryStateContext } from "./context";
import { HistoryStateEnums, type HistoryAction } from "./actions";

export function HistoryReducer(state: IHistoryStateContext, action: HistoryAction): IHistoryStateContext {
  switch (action.type) {
    case HistoryStateEnums.HISTORY_FETCH_ALL_PENDING:
      return { ...state, isPending: true, isSuccess: false, isError: false };
    case HistoryStateEnums.HISTORY_FETCH_ALL_SUCCESS:
      return { ...state, isPending: false, isSuccess: true, isError: false, items: action.items, totalCount: action.totalCount };
    case HistoryStateEnums.HISTORY_FETCH_ALL_ERROR:
      return { ...state, isPending: false, isSuccess: false, isError: true };
    default:
      return state;
  }
}
