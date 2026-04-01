"use client";
import { createContext } from "react";
import type { ContractRecord } from "@/components/contracts/contractData";

export type { ContractRecord };

export interface IContractsStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  items: ContractRecord[];
  selected?: ContractRecord;
  totalCount: number;
}

export interface IContractsActionContext {
  fetchAll: () => void;
  fetchById: (id: string) => void;
}

export const INITIAL_STATE: IContractsStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  items: [],
  totalCount: 0,
};

export const ContractsStateContext = createContext<IContractsStateContext>(INITIAL_STATE);
export const ContractsActionContext = createContext<IContractsActionContext>({
  fetchAll: () => {},
  fetchById: () => {},
});
