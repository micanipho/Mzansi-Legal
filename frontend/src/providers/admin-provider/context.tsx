"use client";
import { createContext } from "react";

export interface IAdminStats {
  documentsIndexed: number;
  totalAnalyses: number;
  activeAlerts: number;
}

export interface IAdminStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  stats: IAdminStats | null;
}

export interface IAdminActionContext {
  fetchAll: () => void;
}

export const INITIAL_STATE: IAdminStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  stats: null,
};

export const AdminStateContext = createContext<IAdminStateContext>(INITIAL_STATE);
export const AdminActionContext = createContext<IAdminActionContext>({
  fetchAll: () => {},
});
