"use client";

import { useState } from "react";
import { Button, Form, Input, Select } from "antd";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/useAuth";
import type { RegisterData } from "@/services/authService";
import { C, fontSans } from "@/styles/theme";

const LANGUAGE_OPTIONS = [
  { value: "en", label: "English" },
  { value: "zu", label: "isiZulu" },
  { value: "st", label: "Sesotho" },
  { value: "af", label: "Afrikaans" },
];

const INPUT_STYLE = {
  borderRadius: 9999,
  height: 44,
};

const ITEM_STYLE = { marginBottom: 12 };

export default function RegisterForm() {
  const t = useTranslations("auth");
  const tCommon = useTranslations("common");
  const { register } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onFinish = async (values: {
    fullName: string;
    emailAddress: string;
    password: string;
    confirmPassword: string;
    preferredLanguage: string;
  }) => {
    setError(null);
    setIsLoading(true);

    // Split full name into name + surname for ABP Zero
    const parts = values.fullName.trim().split(/\s+/);
    const name = parts[0] ?? "";
    const surname = parts.slice(1).join(" ") || name;

    const data: RegisterData = {
      name,
      surname,
      userName: values.emailAddress,
      emailAddress: values.emailAddress,
      password: values.password,
      preferredLanguage: values.preferredLanguage,
    };

    try {
      await register(data);
    } catch (err) {
      const e = err as Error & { status?: number };
      if (e.message?.includes("Failed to fetch")) {
        setError(tCommon("error"));
      } else if (e.status === 400) {
        setError(t("emailTaken"));
      } else {
        setError(e.message || t("invalidCredentials"));
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Form
      layout="vertical"
      onFinish={onFinish}
      requiredMark={false}
      style={{ fontFamily: fontSans }}
    >
      {error && (
        <div
          role="alert"
          aria-live="polite"
          id="register-error"
          style={{
            marginBottom: 16,
            padding: "10px 16px",
            borderRadius: 12,
            background: "rgba(168, 84, 72, 0.08)",
            border: `1px solid rgba(168, 84, 72, 0.25)`,
            color: C.destructive,
            fontSize: 13,
            fontWeight: 500,
          }}
        >
          {error}
        </div>
      )}

      <Form.Item
        name="fullName"
        label={<span style={{ fontFamily: fontSans }}>{t("nameLabel")}</span>}
        rules={[{ required: true, message: `${t("nameLabel")} is required` }]}
        style={ITEM_STYLE}
      >
        <Input
          id="register-name"
          aria-label={t("nameLabel")}
          aria-describedby={error ? "register-error" : undefined}
          style={INPUT_STYLE}
          placeholder={t("nameLabel")}
          autoComplete="name"
        />
      </Form.Item>

      <Form.Item
        name="emailAddress"
        label={<span style={{ fontFamily: fontSans }}>{t("emailLabel")}</span>}
        rules={[
          { required: true, message: `${t("emailLabel")} is required` },
          { type: "email", message: "Please enter a valid email address" },
        ]}
        style={ITEM_STYLE}
      >
        <Input
          id="register-email"
          aria-label={t("emailLabel")}
          aria-describedby={error ? "register-error" : undefined}
          style={INPUT_STYLE}
          placeholder={t("emailLabel")}
          autoComplete="email"
          type="email"
        />
      </Form.Item>

      {/* Password + Confirm side-by-side */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
        <Form.Item
          name="password"
          label={<span style={{ fontFamily: fontSans }}>{t("passwordLabel")}</span>}
          rules={[
            { required: true, message: "Required" },
            { min: 6, message: "Min 6 chars" },
          ]}
          style={ITEM_STYLE}
        >
          <Input.Password
            id="register-password"
            aria-label={t("passwordLabel")}
            aria-describedby={error ? "register-error" : undefined}
            style={INPUT_STYLE}
            placeholder={t("passwordLabel")}
            autoComplete="new-password"
          />
        </Form.Item>

        <Form.Item
          name="confirmPassword"
          label={<span style={{ fontFamily: fontSans }}>{t("confirmPasswordLabel")}</span>}
          dependencies={["password"]}
          rules={[
            { required: true, message: "Required" },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue("password") === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t("passwordMismatch")));
              },
            }),
          ]}
          style={ITEM_STYLE}
        >
          <Input.Password
            id="register-confirm-password"
            aria-label={t("confirmPasswordLabel")}
            aria-describedby={error ? "register-error" : undefined}
            style={INPUT_STYLE}
            placeholder={t("confirmPasswordLabel")}
            autoComplete="new-password"
          />
        </Form.Item>
      </div>

      <Form.Item
        name="preferredLanguage"
        label={<span style={{ fontFamily: fontSans }}>{t("languageLabel")}</span>}
        initialValue="en"
        rules={[{ required: true, message: `${t("languageLabel")} is required` }]}
        style={ITEM_STYLE}
      >
        <Select
          id="register-language"
          aria-label={t("languageLabel")}
          aria-describedby={error ? "register-error" : undefined}
          options={LANGUAGE_OPTIONS}
          style={{ borderRadius: 9999, height: 44 }}
        />
      </Form.Item>

      <Form.Item style={{ marginBottom: 0 }}>
        <Button
          type="primary"
          htmlType="submit"
          loading={isLoading}
          block
          style={{
            height: 44,
            borderRadius: 9999,
            fontWeight: 700,
            fontSize: 16,
            background: C.primary,
            borderColor: C.primary,
          }}
        >
          {t("registerButton")}
        </Button>
      </Form.Item>
    </Form>
  );
}
