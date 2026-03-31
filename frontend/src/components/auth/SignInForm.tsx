"use client";

import { useState } from "react";
import { Button, Form, Input, Alert } from "antd";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/useAuth";
import type { SignInCredentials } from "@/services/authService";
import { C, fontSans } from "@/styles/theme";

const INPUT_STYLE = {
  borderRadius: 9999,
  height: 44,
};

export default function SignInForm() {
  const t = useTranslations("auth");
  const { signIn } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onFinish = async (values: {
    userNameOrEmailAddress: string;
    password: string;
  }) => {
    setError(null);
    setIsLoading(true);

    const credentials: SignInCredentials = {
      userNameOrEmailAddress: values.userNameOrEmailAddress,
      password: values.password,
      rememberClient: false,
    };

    try {
      await signIn(credentials);
    } catch (err) {
      const e = err as Error & { status?: number };
      // Always show a generic message — never reveal which field is wrong
      if (e.status === 401 || e.status === 400) {
        setError(t("invalidCredentials"));
      } else {
        setError(t("invalidCredentials"));
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
          id="signin-error"
        />
      )}

      <Form.Item
        name="userNameOrEmailAddress"
        label={<span style={{ fontFamily: fontSans }}>{t("emailLabel")}</span>}
        rules={[{ required: true, message: `${t("emailLabel")} is required` }]}
      >
        <Input
          id="signin-email"
          aria-label={t("emailLabel")}
          aria-describedby={error ? "signin-error" : undefined}
          style={INPUT_STYLE}
          placeholder={t("emailLabel")}
          autoComplete="username"
          type="text"
        />
      </Form.Item>

      <Form.Item
        name="password"
        label={<span style={{ fontFamily: fontSans }}>{t("passwordLabel")}</span>}
        rules={[{ required: true, message: `${t("passwordLabel")} is required` }]}
      >
        <Input.Password
          id="signin-password"
          aria-label={t("passwordLabel")}
          aria-describedby={error ? "signin-error" : undefined}
          style={INPUT_STYLE}
          placeholder={t("passwordLabel")}
          autoComplete="current-password"
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
          {t("signInButton")}
        </Button>
      </Form.Item>
    </Form>
  );
}
