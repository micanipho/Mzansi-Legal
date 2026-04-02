import type { ContractListItem, ContractRecord } from "./context";

export enum ContractsStateEnums {
  CONTRACTS_FETCH_ALL_PENDING = "CONTRACTS_FETCH_ALL_PENDING",
  CONTRACTS_FETCH_ALL_SUCCESS = "CONTRACTS_FETCH_ALL_SUCCESS",
  CONTRACTS_FETCH_ALL_ERROR = "CONTRACTS_FETCH_ALL_ERROR",
  CONTRACTS_FETCH_ONE_PENDING = "CONTRACTS_FETCH_ONE_PENDING",
  CONTRACTS_FETCH_ONE_SUCCESS = "CONTRACTS_FETCH_ONE_SUCCESS",
  CONTRACTS_FETCH_ONE_ERROR = "CONTRACTS_FETCH_ONE_ERROR",
  CONTRACTS_ANALYSE_PENDING = "CONTRACTS_ANALYSE_PENDING",
  CONTRACTS_ANALYSE_SUCCESS = "CONTRACTS_ANALYSE_SUCCESS",
  CONTRACTS_ANALYSE_ERROR = "CONTRACTS_ANALYSE_ERROR",
}

export type ContractsAction =
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ALL_PENDING }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ALL_SUCCESS; items: ContractListItem[]; totalCount: number }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ALL_ERROR; errorMessage: string }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ONE_PENDING }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ONE_SUCCESS; selected: ContractRecord }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ONE_ERROR; errorMessage: string }
  | { type: ContractsStateEnums.CONTRACTS_ANALYSE_PENDING }
  | { type: ContractsStateEnums.CONTRACTS_ANALYSE_SUCCESS; selected: ContractRecord; items: ContractListItem[]; totalCount: number }
  | { type: ContractsStateEnums.CONTRACTS_ANALYSE_ERROR; errorMessage: string };
