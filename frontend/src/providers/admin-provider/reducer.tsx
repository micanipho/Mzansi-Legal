import { INITIAL_STATE, type IAdminStateContext } from "./context";
import { AdminStateEnums, type AdminAction } from "./actions";

export function AdminReducer(state: IAdminStateContext, action: AdminAction): IAdminStateContext {
  switch (action.type) {
    case AdminStateEnums.ADMIN_FETCH_ALL_PENDING:
      return { ...state, isPending: true, isSuccess: false, isError: false };
    case AdminStateEnums.ADMIN_FETCH_ALL_SUCCESS:
      return { ...state, isPending: false, isSuccess: true, isError: false, stats: action.stats };
    case AdminStateEnums.ADMIN_FETCH_ALL_ERROR:
      return { ...state, isPending: false, isSuccess: false, isError: true };
    default:
      return state;
  }
}
