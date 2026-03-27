"use client";

import { Button, Card, Space, Typography } from "antd";
import { MessageOutlined } from "@ant-design/icons";
import { useLocale, useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import { brand } from "@/styles/theme";

const { Text, Title } = Typography;

export default function HistoryPage() {
  const t = useTranslations("history");
  const locale = useLocale();
  const router = useRouter();

  return (
    <main style={{ background: brand.cream, minHeight: "100vh" }}>
      <div style={{ background: "#fff", borderBottom: `1px solid ${brand.border}`, padding: "16px 32px" }}>
        <Text style={{ fontWeight: 700, fontSize: 15, color: brand.dark }}>{t("title")}</Text>
      </div>

      <div style={{ maxWidth: 760, margin: "0 auto", padding: "80px 32px", textAlign: "center" }}>
        <MessageOutlined style={{ fontSize: 40, color: "#D1D5DB", display: "block", marginBottom: 16 }} />
        <Title level={4} style={{ color: brand.dark, margin: "0 0 8px" }}>
          {t("empty")}
        </Title>
        <Text style={{ color: "#6B7280", fontSize: 14, display: "block", marginBottom: 24 }}>
          {t("signInPrompt")}
        </Text>
        <Space>
          <Button
            type="primary"
            onClick={() => router.push(`/${locale}/chat`)}
            style={{ background: brand.dark, borderColor: brand.dark, borderRadius: 20, fontWeight: 600 }}
          >
            {t("askQuestion")}
          </Button>
          <Button
            href={`/${locale}/auth`}
            style={{ borderRadius: 20, borderColor: brand.border, color: brand.dark, fontWeight: 600 }}
          >
            {t("signIn")}
          </Button>
        </Space>
      </div>
    </main>
  );
}
