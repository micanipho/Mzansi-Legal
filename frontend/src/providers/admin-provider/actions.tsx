import type { IAdminStats } from "./context";

export enum AdminStateEnums {
  ADMIN_FETCH_ALL_PENDING = "ADMIN_FETCH_ALL_PENDING",
  ADMIN_FETCH_ALL_SUCCESS = "ADMIN_FETCH_ALL_SUCCESS",
  ADMIN_FETCH_ALL_ERROR   = "ADMIN_FETCH_ALL_ERROR",
}

export type AdminAction =
  | { type: AdminStateEnums.ADMIN_FETCH_ALL_PENDING }
  | { type: AdminStateEnums.ADMIN_FETCH_ALL_SUCCESS; stats: IAdminStats }
  | { type: AdminStateEnums.ADMIN_FETCH_ALL_ERROR };
