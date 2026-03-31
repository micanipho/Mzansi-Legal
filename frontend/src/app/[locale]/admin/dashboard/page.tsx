"use client";

import { useTranslations } from "next-intl";
import { Activity, FileText, Globe2, ShieldAlert } from "lucide-react";
import InsightChart from "@/components/dashboard/InsightChart";
import SectionCard from "@/components/dashboard/SectionCard";
import SummaryCard from "@/components/dashboard/SummaryCard";
import AdminGuard from "@/components/guards/AdminGuard";
import { C } from "@/styles/theme";

const insightData = [
  { label: "Housing", value: 67, tone: "primary" },
  { label: "Employment", value: 54, tone: "secondary" },
  { label: "Credit", value: 42, tone: "danger" },
];

export default function AdminDashboardPage() {
  const t = useTranslations("admin");

  return (
    <AdminGuard>
      <main className="page-shell" style={{ display: "flex", flexDirection: "column", gap: 32 }}>
      <section style={{ maxWidth: 860, display: "flex", flexDirection: "column", gap: 12 }}>
        <span
          style={{
            width: "fit-content",
            padding: "6px 14px",
            borderRadius: 9999,
            background: "rgba(74, 90, 58, 0.1)",
            color: C.primary,
            fontWeight: 800,
            fontSize: 12,
            letterSpacing: "0.08em",
            textTransform: "uppercase",
          }}
        >
          {t("shellEyebrow")}
        </span>
        <h1 style={{ margin: 0, fontSize: "clamp(2.6rem, 5vw, 4.3rem)", color: C.fg }}>
          {t("dashboardTitle")}
        </h1>
        <p style={{ margin: 0, color: C.mutedFg, fontSize: 17, lineHeight: 1.7 }}>{t("shellDescription")}</p>
      </section>

      <section className="responsive-three-grid">
        <SummaryCard
          eyebrow={t("stats.documents")}
          value="13"
          description={t("stats.documentsDescription")}
          icon={<FileText size={20} color={C.secondary} />}
        />
        <SummaryCard
          eyebrow={t("stats.analyses")}
          value="342"
          description={t("stats.analysesDescription")}
          icon={<Activity size={20} color={C.primary} />}
        />
        <SummaryCard
          eyebrow={t("stats.alerts")}
          value="27"
          description={t("stats.alertsDescription")}
          icon={<ShieldAlert size={20} color={C.destructive} />}
        />
      </section>

      <section className="responsive-dashboard-grid">
        <SectionCard title={t("insightTitle")} description={t("insightDescription")}>
          <InsightChart
            data={insightData}
            title={t("insightChartTitle")}
            emptyTitle={t("emptyState.title")}
            emptyDescription={t("emptyState.description")}
          />
        </SectionCard>

        <SectionCard title={t("coverageTitle")} description={t("coverageDescription")}>
          <div style={{ display: "grid", gap: 16 }}>
            {[
              { title: t("coverage.languages"), value: "4", detail: t("coverage.languagesDetail"), icon: <Globe2 size={18} color={C.primary} /> },
              { title: t("coverage.sources"), value: "91%", detail: t("coverage.sourcesDetail"), icon: <FileText size={18} color={C.secondary} /> },
              { title: t("coverage.risk"), value: "67%", detail: t("coverage.riskDetail"), icon: <ShieldAlert size={18} color={C.destructive} /> },
            ].map((item) => (
              <article
                key={item.title}
                style={{
                  border: `1px solid ${C.border}`,
                  borderRadius: 22,
                  padding: 18,
                  display: "flex",
                  flexDirection: "column",
                  gap: 10,
                  background: "rgba(255,255,255,0.62)",
                }}
              >
                <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center" }}>
                  <strong style={{ color: C.fg }}>{item.title}</strong>
                  {item.icon}
                </div>
                <span style={{ fontSize: 34, lineHeight: 1, fontWeight: 800, color: C.fg }}>{item.value}</span>
                <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.6 }}>{item.detail}</p>
              </article>
            ))}
          </div>
        </SectionCard>
      </section>

      <SectionCard title={t("activityTitle")} description={t("activityDescription")}>
        <div style={{ display: "grid", gap: 16 }}>
          {[0, 1, 2].map((index) => (
            <article
              key={index}
              style={{
                borderLeft: `4px solid ${index === 2 ? C.destructive : index === 1 ? C.secondary : C.primary}`,
                background: "rgba(255,255,255,0.62)",
                borderRadius: 20,
                padding: 20,
              }}
            >
              <strong style={{ display: "block", marginBottom: 8, color: C.fg }}>{t(`activity.items.${index}.title`)}</strong>
              <p style={{ margin: "0 0 6px", color: C.mutedFg, lineHeight: 1.6 }}>{t(`activity.items.${index}.description`)}</p>
              <span style={{ fontSize: 13, color: C.mutedFg, fontWeight: 700 }}>{t(`activity.items.${index}.meta`)}</span>
            </article>
          ))}
        </div>
      </SectionCard>
      </main>
    </AdminGuard>
  );
}
