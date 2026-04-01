"use client";
import { useContext, useReducer } from "react";
import { demoContracts, getContractById } from "@/components/contracts/contractData";
import { ContractsReducer } from "./reducer";
import { INITIAL_STATE, ContractsStateContext, ContractsActionContext } from "./context";
import { ContractsStateEnums } from "./actions";

// TODO: replace demoContracts calls with real API once contract analysis endpoint is available:
// GET /api/app/contracts  →  fetchAll
// GET /api/app/contracts/{id}  →  fetchById

export const ContractsProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(ContractsReducer, INITIAL_STATE);

  const fetchAll = async () => {
    dispatch({ type: ContractsStateEnums.CONTRACTS_FETCH_ALL_PENDING });
    try {
      await Promise.resolve(); // placeholder for real API call
      dispatch({
        type: ContractsStateEnums.CONTRACTS_FETCH_ALL_SUCCESS,
        items: demoContracts,
        totalCount: demoContracts.length,
      });
    } catch {
      dispatch({ type: ContractsStateEnums.CONTRACTS_FETCH_ALL_ERROR });
    }
  };

  const fetchById = async (id: string) => {
    dispatch({ type: ContractsStateEnums.CONTRACTS_FETCH_ONE_PENDING });
    try {
      const selected = getContractById(id);
      if (!selected) throw new Error("Contract not found");
      dispatch({ type: ContractsStateEnums.CONTRACTS_FETCH_ONE_SUCCESS, selected });
    } catch {
      dispatch({ type: ContractsStateEnums.CONTRACTS_FETCH_ONE_ERROR });
    }
  };

  return (
    <ContractsStateContext.Provider value={state}>
      <ContractsActionContext.Provider value={{ fetchAll, fetchById }}>
        {children}
      </ContractsActionContext.Provider>
    </ContractsStateContext.Provider>
  );
};

export const useContractsState = () => {
  const context = useContext(ContractsStateContext);
  if (!context) throw new Error("useContractsState must be used within ContractsProvider");
  return context;
};

export const useContractsAction = () => {
  const context = useContext(ContractsActionContext);
  if (!context) throw new Error("useContractsAction must be used within ContractsProvider");
  return context;
};
