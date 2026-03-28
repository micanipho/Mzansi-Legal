import type { ReactNode } from "react";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

interface SectionCardProps {
  title: string;
  description?: string;
  children: ReactNode;
}

export default function SectionCard({ title, description, children }: SectionCardProps) {
  return (
    <section
      className="surface-card grain-panel"
      style={{
        borderRadius: 28,
        padding: 28,
        boxShadow: shadowOrganic,
        display: "flex",
        flexDirection: "column",
        gap: 20,
      }}
    >
      <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
        <h2 style={{ margin: 0, fontFamily: fontSerif, fontSize: 28, color: C.fg }}>{title}</h2>
        {description ? (
          <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.6, fontFamily: fontSans }}>{description}</p>
        ) : null}
      </div>
      {children}
    </section>
  );
}
