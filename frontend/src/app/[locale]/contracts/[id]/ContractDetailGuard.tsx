"use client";

import { ReactNode } from "react";
import AuthGuard from "@/components/guards/AuthGuard";

export default function ContractDetailGuard({ children }: { children: ReactNode }) {
  return <AuthGuard>{children}</AuthGuard>;
}
