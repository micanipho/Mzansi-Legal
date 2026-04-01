import { INITIAL_STATE, type IContractsStateContext } from "./context";
import { ContractsStateEnums, type ContractsAction } from "./actions";

export function ContractsReducer(state: IContractsStateContext, action: ContractsAction): IContractsStateContext {
  switch (action.type) {
    case ContractsStateEnums.CONTRACTS_FETCH_ALL_PENDING:
      return { ...state, isPending: true, isSuccess: false, isError: false };
    case ContractsStateEnums.CONTRACTS_FETCH_ALL_SUCCESS:
      return { ...state, isPending: false, isSuccess: true, isError: false, items: action.items, totalCount: action.totalCount };
    case ContractsStateEnums.CONTRACTS_FETCH_ALL_ERROR:
      return { ...state, isPending: false, isSuccess: false, isError: true };
    case ContractsStateEnums.CONTRACTS_FETCH_ONE_PENDING:
      return { ...state, isPending: true, isSuccess: false, isError: false };
    case ContractsStateEnums.CONTRACTS_FETCH_ONE_SUCCESS:
      return { ...state, isPending: false, isSuccess: true, isError: false, selected: action.selected };
    case ContractsStateEnums.CONTRACTS_FETCH_ONE_ERROR:
      return { ...state, isPending: false, isSuccess: false, isError: true };
    default:
      return state;
  }
}
