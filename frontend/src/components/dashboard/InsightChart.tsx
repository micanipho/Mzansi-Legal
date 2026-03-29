"use client";

import { Column } from "@ant-design/charts";
import { Empty } from "antd";
import { C } from "@/styles/theme";

interface InsightDatum {
  label: string;
  value: number;
  tone: string;
}

interface InsightChartProps {
  data: InsightDatum[];
  title: string;
  emptyTitle: string;
  emptyDescription: string;
}

export default function InsightChart({
  data,
  title,
  emptyTitle,
  emptyDescription,
}: InsightChartProps) {
  if (data.length === 0) {
    return (
      <div
        style={{
          minHeight: 280,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          border: `1px dashed ${C.border}`,
          borderRadius: 24,
          background: "rgba(255,255,255,0.5)",
        }}
      >
        <Empty
          description={
            <div>
              <strong style={{ display: "block", marginBottom: 4 }}>{emptyTitle}</strong>
              <span>{emptyDescription}</span>
            </div>
          }
        />
      </div>
    );
  }

  return (
    <div style={{ minHeight: 320 }}>
      <Column
        data={data}
        xField="label"
        yField="value"
        colorField="tone"
        axis={{
          x: { title: false },
          y: { title: false },
        }}
        legend={false}
        scale={{
          color: {
            range: ["#5d7052", "#c18c5d", "#a85448"],
          },
        }}
        style={{
          maxWidth: "100%",
        }}
        title={title}
      />
    </div>
  );
}
