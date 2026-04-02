import { type IContractsStateContext } from "./context";
import { ContractsStateEnums, type ContractsAction } from "./actions";

export function ContractsReducer(
  state: IContractsStateContext,
  action: ContractsAction
): IContractsStateContext {
  switch (action.type) {
    case ContractsStateEnums.CONTRACTS_FETCH_ALL_PENDING:
    case ContractsStateEnums.CONTRACTS_FETCH_ONE_PENDING:
    case ContractsStateEnums.CONTRACTS_ANALYSE_PENDING:
      return { ...state, isPending: true, isSuccess: false, isError: false, errorMessage: undefined };
    case ContractsStateEnums.CONTRACTS_FETCH_ALL_SUCCESS:
      return {
        ...state,
        isPending: false,
        isSuccess: true,
        isError: false,
        errorMessage: undefined,
        items: action.items,
        totalCount: action.totalCount,
      };
    case ContractsStateEnums.CONTRACTS_FETCH_ONE_SUCCESS:
      return {
        ...state,
        isPending: false,
        isSuccess: true,
        isError: false,
        errorMessage: undefined,
        selected: action.selected,
      };
    case ContractsStateEnums.CONTRACTS_ANALYSE_SUCCESS:
      return {
        ...state,
        isPending: false,
        isSuccess: true,
        isError: false,
        errorMessage: undefined,
        selected: action.selected,
        items: action.items,
        totalCount: action.totalCount,
      };
    case ContractsStateEnums.CONTRACTS_FETCH_ALL_ERROR:
    case ContractsStateEnums.CONTRACTS_FETCH_ONE_ERROR:
    case ContractsStateEnums.CONTRACTS_ANALYSE_ERROR:
      return {
        ...state,
        isPending: false,
        isSuccess: false,
        isError: true,
        errorMessage: action.errorMessage,
      };
    default:
      return state;
  }
}
