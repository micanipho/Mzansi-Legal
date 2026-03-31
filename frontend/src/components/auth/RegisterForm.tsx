"use client";

import { useState } from "react";
import { Button, Form, Input, Select, Alert } from "antd";
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

export default function RegisterForm() {
  const t = useTranslations("auth");
  const { register } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onFinish = async (values: {
    fullName: string;
    emailAddress: string;
    password: string;
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
      if (e.status === 400) {
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
        <Alert
          type="error"
          message={error}
          style={{ marginBottom: 16, borderRadius: 12 }}
          role="alert"
          aria-live="polite"
          id="register-error"
        />
      )}

      <Form.Item
        name="fullName"
        label={<span style={{ fontFamily: fontSans }}>{t("nameLabel")}</span>}
        rules={[{ required: true, message: `${t("nameLabel")} is required` }]}
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

      <Form.Item
        name="password"
        label={<span style={{ fontFamily: fontSans }}>{t("passwordLabel")}</span>}
        rules={[
          { required: true, message: `${t("passwordLabel")} is required` },
          { min: 6, message: "Password must be at least 6 characters" },
        ]}
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
        name="preferredLanguage"
        label={<span style={{ fontFamily: fontSans }}>{t("languageLabel")}</span>}
        initialValue="en"
        rules={[{ required: true, message: `${t("languageLabel")} is required` }]}
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
