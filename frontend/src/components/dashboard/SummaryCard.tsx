import type { ReactNode } from "react";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

interface SummaryCardProps {
  eyebrow: string;
  value: string;
  description: string;
  icon?: ReactNode;
}

export default function SummaryCard({ eyebrow, value, description, icon }: SummaryCardProps) {
  return (
    <article
      className="surface-card grain-panel"
      style={{
        borderRadius: 24,
        padding: 24,
        display: "flex",
        flexDirection: "column",
        gap: 16,
        boxShadow: shadowOrganic,
      }}
    >
      <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "flex-start" }}>
        <span
          style={{
            fontSize: 12,
            fontWeight: 800,
            textTransform: "uppercase",
            letterSpacing: "0.08em",
            color: C.mutedFg,
            fontFamily: fontSans,
          }}
        >
          {eyebrow}
        </span>
        {icon}
      </div>
      <strong style={{ fontFamily: fontSerif, fontSize: 40, lineHeight: 1, color: C.fg }}>{value}</strong>
      <p style={{ margin: 0, color: C.mutedFg, fontSize: 14, lineHeight: 1.6 }}>{description}</p>
    </article>
  );
}
