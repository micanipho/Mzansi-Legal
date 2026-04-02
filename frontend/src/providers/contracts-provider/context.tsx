"use client";

import { createContext } from "react";
import type { ContractListItem, ContractRecord } from "@/components/contracts/contractData";

export type { ContractListItem, ContractRecord };

export interface IContractsStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  items: ContractListItem[];
  selected?: ContractRecord;
  totalCount: number;
}

export interface IContractsActionContext {
  fetchAll: (locale?: string) => Promise<void>;
  fetchById: (id: string, locale?: string) => Promise<void>;
  analyse: (file: File, locale?: string) => Promise<ContractRecord>;
}

export const INITIAL_STATE: IContractsStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  errorMessage: undefined,
  items: [],
  totalCount: 0,
};

export const ContractsStateContext = createContext<IContractsStateContext>(INITIAL_STATE);
export const ContractsActionContext = createContext<IContractsActionContext>({
  fetchAll: async () => {},
  fetchById: async () => {},
  analyse: async () => {
    throw new Error("Contracts provider is not mounted");
  },
});
