"use client";

import { useContext, useReducer } from "react";
import type { ContractListItem } from "@/components/contracts/contractData";
import { getUserFacingErrorMessage } from "@/lib/userFacingErrors";
import {
  analyseContract,
  getContractAnalysis,
  getMyContracts,
} from "@/services/contract.service";
import { ContractsReducer } from "./reducer";
import {
  INITIAL_STATE,
  ContractsStateContext,
  ContractsActionContext,
} from "./context";
import { ContractsStateEnums } from "./actions";

function toListItem(selected: {
  id: string;
  displayTitle: string;
  contractType: string;
  healthScore: number;
  summary: string;
  language: string;
  analysedAt: string;
  redFlagCount: number;
  amberFlagCount: number;
  greenFlagCount: number;
}): ContractListItem {
  return {
    id: selected.id,
    displayTitle: selected.displayTitle,
    contractType: selected.contractType,
    healthScore: selected.healthScore,
    summary: selected.summary,
    language: selected.language,
    analysedAt: selected.analysedAt,
    redFlagCount: selected.redFlagCount,
    amberFlagCount: selected.amberFlagCount,
    greenFlagCount: selected.greenFlagCount,
  };
}

export const ContractsProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ContractsReducer, INITIAL_STATE);

  const fetchAll = async (locale?: string) => {
    dispatch({ type: ContractsStateEnums.CONTRACTS_FETCH_ALL_PENDING });
    try {
      const result = await getMyContracts(locale);
      dispatch({
        type: ContractsStateEnums.CONTRACTS_FETCH_ALL_SUCCESS,
        items: result.items,
        totalCount: result.totalCount,
      });
    } catch (error) {
      dispatch({
        type: ContractsStateEnums.CONTRACTS_FETCH_ALL_ERROR,
        errorMessage: getUserFacingErrorMessage(
          error,
          "We couldn't load your contract analyses. Please try again.",
        ),
      });
    }
  };

  const fetchById = async (id: string, locale?: string) => {
    dispatch({ type: ContractsStateEnums.CONTRACTS_FETCH_ONE_PENDING });
    try {
      const selected = await getContractAnalysis(id, locale);
      dispatch({
        type: ContractsStateEnums.CONTRACTS_FETCH_ONE_SUCCESS,
        selected,
      });
    } catch (error) {
      dispatch({
        type: ContractsStateEnums.CONTRACTS_FETCH_ONE_ERROR,
        errorMessage: getUserFacingErrorMessage(
          error,
          "We couldn't load this contract analysis. Please try again.",
        ),
      });
    }
  };

  const analyse = async (file: File, locale?: string) => {
    dispatch({ type: ContractsStateEnums.CONTRACTS_ANALYSE_PENDING });
    try {
      const selected = await analyseContract(file, locale);
      const nextItems = [
        toListItem(selected),
        ...state.items.filter((item) => item.id !== selected.id),
      ];
      dispatch({
        type: ContractsStateEnums.CONTRACTS_ANALYSE_SUCCESS,
        selected,
        items: nextItems,
        totalCount: nextItems.length,
      });
      return selected;
    } catch (error) {
      const errorMessage = getUserFacingErrorMessage(
        error,
        "We couldn't analyse that contract just now. Please try again.",
      );
      dispatch({
        type: ContractsStateEnums.CONTRACTS_ANALYSE_ERROR,
        errorMessage,
      });
      throw error;
    }
  };

  return (
    <ContractsStateContext.Provider value={state}>
      <ContractsActionContext.Provider value={{ fetchAll, fetchById, analyse }}>
        {children}
      </ContractsActionContext.Provider>
    </ContractsStateContext.Provider>
  );
};

export const useContractsState = () => {
  const context = useContext(ContractsStateContext);
  if (!context)
    throw new Error("useContractsState must be used within ContractsProvider");
  return context;
};

export const useContractsAction = () => {
  const context = useContext(ContractsActionContext);
  if (!context)
    throw new Error("useContractsAction must be used within ContractsProvider");
  return context;
};
