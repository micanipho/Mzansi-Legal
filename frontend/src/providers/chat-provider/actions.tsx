import type { IChatMessage } from "./context";

export enum ChatStateEnums {
  CHAT_LOAD_PENDING = "CHAT_LOAD_PENDING",
  CHAT_LOAD_SUCCESS = "CHAT_LOAD_SUCCESS",
  CHAT_LOAD_ERROR   = "CHAT_LOAD_ERROR",
  CHAT_SEND_PENDING = "CHAT_SEND_PENDING",
  CHAT_SEND_SUCCESS = "CHAT_SEND_SUCCESS",
  CHAT_SEND_ERROR   = "CHAT_SEND_ERROR",
  CHAT_CLEAR        = "CHAT_CLEAR",
}

export type ChatAction =
  | { type: ChatStateEnums.CHAT_LOAD_PENDING }
  | { type: ChatStateEnums.CHAT_LOAD_SUCCESS; messages: IChatMessage[]; conversationId: string | null }
  | { type: ChatStateEnums.CHAT_LOAD_ERROR; error: string }
  | { type: ChatStateEnums.CHAT_SEND_PENDING; userMsg: IChatMessage }
  | { type: ChatStateEnums.CHAT_SEND_SUCCESS; botMsg: IChatMessage; conversationId: string | null }
  | { type: ChatStateEnums.CHAT_SEND_ERROR;   errorMsg: IChatMessage; error: string }
  | { type: ChatStateEnums.CHAT_CLEAR };
