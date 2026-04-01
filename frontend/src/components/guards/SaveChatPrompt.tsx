"use client";

import { Modal, Button } from "antd";
import { useLocale, useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";

interface SaveChatPromptProps {
  open: boolean;
  onClose: () => void;
}

/**
 * Modal that prompts guests to sign in when they try to save chat.
 * Shows title, description, and two actions:
 * - Primary: "Sign In to Save" (redirects to auth with returnUrl)
 * - Cancel: "Continue without saving" (closes modal)
 */
export default function SaveChatPrompt({ open, onClose }: SaveChatPromptProps) {
  const t = useTranslations("chat");
  const locale = useLocale();
  const router = useRouter();

  const handleSignIn = () => {
    const returnUrl = encodeURIComponent(`/${locale}/ask`);
    router.push(`${createLocalizedPath(locale, appRoutes.auth)}?returnUrl=${returnUrl}`);
  };

  return (
    <Modal
      open={open}
      onCancel={onClose}
      footer={null}
      centered
      width={480}
      styles={{
        body: { padding: 32 },
      }}
    >
      <div style={{ display: "flex", flexDirection: "column", gap: 24 }}>
        <div>
          <h3 style={{ fontSize: 22, fontWeight: 700, margin: "0 0 12px" }}>
            {t("saveChatPromptTitle")}
          </h3>
          <p style={{ margin: 0, fontSize: 15, lineHeight: 1.6, color: "#666" }}>
            {t("saveChatPromptDesc")}
          </p>
        </div>

        <div style={{ display: "flex", gap: 12, justifyContent: "flex-end" }}>
          <Button onClick={onClose} size="large">
            {t("saveChatPromptCancel")}
          </Button>
          <Button type="primary" onClick={handleSignIn} size="large">
            {t("saveChatPromptConfirm")}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
