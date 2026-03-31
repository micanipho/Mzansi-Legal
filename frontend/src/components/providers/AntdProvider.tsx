"use client";

import { ConfigProvider, App } from "antd";
import { antdTheme } from "@/styles/theme";
import type { ReactNode } from "react";

interface AntdProviderProps {
  children: ReactNode;
}

/**
 * Wraps the application with Ant Design's ConfigProvider using MzansiLegal
 * design tokens, and the App component for static access to message/modal APIs.
 */
export default function AntdProvider({ children }: AntdProviderProps) {
  return (
    <ConfigProvider theme={antdTheme}>
      <App>{children}</App>
    </ConfigProvider>
  );
}
