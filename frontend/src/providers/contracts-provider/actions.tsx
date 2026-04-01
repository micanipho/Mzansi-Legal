import type { ContractRecord } from "./context";

export enum ContractsStateEnums {
  CONTRACTS_FETCH_ALL_PENDING = "CONTRACTS_FETCH_ALL_PENDING",
  CONTRACTS_FETCH_ALL_SUCCESS = "CONTRACTS_FETCH_ALL_SUCCESS",
  CONTRACTS_FETCH_ALL_ERROR   = "CONTRACTS_FETCH_ALL_ERROR",
  CONTRACTS_FETCH_ONE_PENDING = "CONTRACTS_FETCH_ONE_PENDING",
  CONTRACTS_FETCH_ONE_SUCCESS = "CONTRACTS_FETCH_ONE_SUCCESS",
  CONTRACTS_FETCH_ONE_ERROR   = "CONTRACTS_FETCH_ONE_ERROR",
}

export type ContractsAction =
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ALL_PENDING }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ALL_SUCCESS; items: ContractRecord[]; totalCount: number }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ALL_ERROR }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ONE_PENDING }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ONE_SUCCESS; selected: ContractRecord }
  | { type: ContractsStateEnums.CONTRACTS_FETCH_ONE_ERROR };
