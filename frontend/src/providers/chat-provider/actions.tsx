import type { IChatStateContext, IChatMessage } from "./context";

export enum ChatStateEnums {
  CHAT_SEND_PENDING = "CHAT_SEND_PENDING",
  CHAT_SEND_SUCCESS = "CHAT_SEND_SUCCESS",
  CHAT_SEND_ERROR   = "CHAT_SEND_ERROR",
  CHAT_ADD_USER_MSG = "CHAT_ADD_USER_MSG",
  CHAT_CLEAR        = "CHAT_CLEAR",
}

export interface IChatAction {
  type: ChatStateEnums;
  payload: Partial<IChatStateContext>;
}

export const chatSendPending = (userMsg: IChatMessage): IChatAction => ({
  type: ChatStateEnums.CHAT_SEND_PENDING,
  payload: { isPending: true, isSuccess: false, isError: false, error: null, messages: undefined },
});

export const chatSendSuccess = (botMsg: IChatMessage): IChatAction => ({
  type: ChatStateEnums.CHAT_SEND_SUCCESS,
  payload: { isPending: false, isSuccess: true, isError: false, error: null },
});

export const chatSendError = (error: string): IChatAction => ({
  type: ChatStateEnums.CHAT_SEND_ERROR,
  payload: { isPending: false, isSuccess: false, isError: true, error },
});

export const chatClear = (): IChatAction => ({
  type: ChatStateEnums.CHAT_CLEAR,
  payload: { isPending: false, isSuccess: false, isError: false, messages: [], error: null },
});

// Carry message data alongside action for reducer use
export interface IChatSendPendingAction extends IChatAction {
  type: ChatStateEnums.CHAT_SEND_PENDING;
  userMsg: IChatMessage;
}

export interface IChatSendSuccessAction extends IChatAction {
  type: ChatStateEnums.CHAT_SEND_SUCCESS;
  botMsg: IChatMessage;
}

export interface IChatSendErrorAction extends IChatAction {
  type: ChatStateEnums.CHAT_SEND_ERROR;
  errorMsg: IChatMessage;
}

export type ChatAction =
  | { type: ChatStateEnums.CHAT_SEND_PENDING; userMsg: IChatMessage }
  | { type: ChatStateEnums.CHAT_SEND_SUCCESS; botMsg: IChatMessage }
  | { type: ChatStateEnums.CHAT_SEND_ERROR;   errorMsg: IChatMessage; error: string }
  | { type: ChatStateEnums.CHAT_CLEAR };
