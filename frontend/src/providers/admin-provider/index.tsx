"use client";
import { useContext, useReducer } from "react";
import { AdminReducer } from "./reducer";
import { INITIAL_STATE, AdminStateContext, AdminActionContext } from "./context";
import { AdminStateEnums } from "./actions";

// TODO: replace mock with real API once admin stats endpoint is available:
// GET /api/app/admin/stats  →  fetchAll
const MOCK_STATS = { documentsIndexed: 13, totalAnalyses: 342, activeAlerts: 27 };

export const AdminProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(AdminReducer, INITIAL_STATE);

  const fetchAll = async () => {
    dispatch({ type: AdminStateEnums.ADMIN_FETCH_ALL_PENDING });
    try {
      await Promise.resolve(); // placeholder for real API call
      dispatch({ type: AdminStateEnums.ADMIN_FETCH_ALL_SUCCESS, stats: MOCK_STATS });
    } catch {
      dispatch({ type: AdminStateEnums.ADMIN_FETCH_ALL_ERROR });
    }
  };

  return (
    <AdminStateContext.Provider value={state}>
      <AdminActionContext.Provider value={{ fetchAll }}>
        {children}
      </AdminActionContext.Provider>
    </AdminStateContext.Provider>
  );
};

export const useAdminState = () => {
  const context = useContext(AdminStateContext);
  if (!context) throw new Error("useAdminState must be used within AdminProvider");
  return context;
};

export const useAdminAction = () => {
  const context = useContext(AdminActionContext);
  if (!context) throw new Error("useAdminAction must be used within AdminProvider");
  return context;
};
