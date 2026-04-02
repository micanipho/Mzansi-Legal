"use client";

import type { ReactNode } from "react";
import { AlertTriangle, RefreshCw, WifiOff } from "lucide-react";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

interface RetryNoticeProps {
  title: string;
  description: string;
  onRetry?: () => void | Promise<void>;
  isOffline?: boolean;
  compact?: boolean;
  icon?: ReactNode;
}

export default function RetryNotice({
  title,
  description,
  onRetry,
  isOffline = false,
  compact = false,
  icon,
}: RetryNoticeProps) {
  return (
    <section
      className="surface-card grain-panel"
      style={{
        padding: compact ? 20 : 28,
        borderRadius: compact ? 22 : 28,
        boxShadow: shadowOrganic,
        display: "grid",
        gap: 14,
        background: isOffline
          ? "rgba(255, 248, 238, 0.94)"
          : "rgba(254, 248, 246, 0.96)",
        border: `1px solid ${isOffline ? "rgba(193, 140, 93, 0.28)" : "rgba(168, 84, 72, 0.22)"}`,
      }}
    >
      <div style={{ display: "flex", alignItems: "flex-start", gap: 12 }}>
        <div
          style={{
            width: 44,
            height: 44,
            borderRadius: 9999,
            display: "inline-flex",
            alignItems: "center",
            justifyContent: "center",
            background: isOffline
              ? "rgba(193, 140, 93, 0.12)"
              : "rgba(168, 84, 72, 0.12)",
            color: isOffline ? C.secondary : C.destructive,
            flexShrink: 0,
          }}
        >
          {icon ??
            (isOffline ? <WifiOff size={18} /> : <AlertTriangle size={18} />)}
        </div>
        <div style={{ display: "grid", gap: 6 }}>
          <strong
            style={{
              color: C.fg,
              fontFamily: fontSerif,
              fontSize: compact ? 22 : 26,
            }}
          >
            {title}
          </strong>
          <p
            style={{
              margin: 0,
              color: C.mutedFg,
              lineHeight: 1.7,
              fontFamily: fontSans,
            }}
          >
            {description}
          </p>
        </div>
      </div>

      {onRetry ? (
        <button
          type="button"
          onClick={() => void onRetry()}
          style={{
            minHeight: 44,
            justifySelf: "flex-start",
            display: "inline-flex",
            alignItems: "center",
            gap: 8,
            padding: "10px 18px",
            borderRadius: 9999,
            border: `1px solid ${C.border}`,
            background: "rgba(255,255,255,0.78)",
            color: C.fg,
            fontFamily: fontSans,
            fontWeight: 700,
            cursor: "pointer",
          }}
        >
          <RefreshCw size={16} />
          Retry
        </button>
      ) : null}
    </section>
  );
}
