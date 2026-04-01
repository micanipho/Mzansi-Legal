import { INITIAL_STATE, type IChatStateContext } from "./context";
import { ChatStateEnums, type ChatAction } from "./actions";

export function ChatReducer(state: IChatStateContext, action: ChatAction): IChatStateContext {
  switch (action.type) {
    case ChatStateEnums.CHAT_SEND_PENDING:
      return {
        ...state,
        isPending: true,
        isSuccess: false,
        isError: false,
        error: null,
        messages: [...state.messages, action.userMsg],
      };
    case ChatStateEnums.CHAT_SEND_SUCCESS:
      return {
        ...state,
        isPending: false,
        isSuccess: true,
        isError: false,
        messages: [...state.messages, action.botMsg],
      };
    case ChatStateEnums.CHAT_SEND_ERROR:
      return {
        ...state,
        isPending: false,
        isSuccess: false,
        isError: true,
        error: action.error,
        messages: [...state.messages, action.errorMsg],
      };
    case ChatStateEnums.CHAT_CLEAR:
      return { ...INITIAL_STATE };
    default:
      return state;
  }
}
