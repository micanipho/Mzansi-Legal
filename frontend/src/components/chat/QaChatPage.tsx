"use client";

import { useLocale, useTranslations } from "next-intl";
import { useRouter } from "next/navigation";
import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { Alert, Button } from "antd";
import { C, fontSans } from "@/styles/theme";
import { useChat } from "@/hooks/useChat";
import { useAuth } from "@/hooks/useAuth";
import { appRoutes, createLocalizedPath } from "@/i18n/routing";
import ChatInput from "./ChatInput";
import ChatThread from "./ChatThread";

export default function QaChatPage() {
  const locale = useLocale();
  const router = useRouter();
  const t = useTranslations("chat");
  const searchParams = useSearchParams();
  const initialQuestion = searchParams.get("q") ?? "";

  const { messages, isLoading, error, sendMessage } = useChat();
  const { user } = useAuth();
  const [showSavePrompt, setShowSavePrompt] = useState(false);

  useEffect(() => {
    if (initialQuestion) {
      void sendMessage(initialQuestion, locale);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSend = (text: string) => {
    // Allow guests to chat, no redirect
    void sendMessage(text, locale);
  };

  const handleSignIn = () => {
    const returnUrl = encodeURIComponent(`/${locale}${appRoutes.ask}`);
    router.push(`${createLocalizedPath(locale, "auth")}?returnUrl=${returnUrl}`);
  };

  return (
    <main
      className="page-shell page-shell--narrow"
      style={{
        paddingBottom: 128,
        display: "flex",
        flexDirection: "column",
        gap: 24,
        fontFamily: fontSans,
      }}
      role="log"
      aria-live="polite"
    >
      {/* Guest Banner */}
      {!user && (
        <Alert
          type="info"
          message={t("guestBannerTitle")}
          description={t("guestBannerDesc")}
          action={
            <Button
              type="primary"
              size="small"
              onClick={handleSignIn}
              style={{ whiteSpace: "nowrap" }}
            >
              {t("guestBannerAction")}
            </Button>
          }
          style={{
            borderRadius: 12,
            position: "sticky",
            top: 16,
            zIndex: 10,
          }}
          showIcon
        />
      )}
      <ChatThread
        messages={messages}
        isLoading={isLoading}
        error={error}
        emptyStateText={t("emptyState")}
      />

      <div
        style={{
          position: "fixed",
          bottom: 0,
          left: 0,
          right: 0,
          padding: 16,
          background: `linear-gradient(to top, ${C.bg} 60%, transparent)`,
          zIndex: 40,
        }}
      >
        <div style={{ maxWidth: 896, margin: "0 auto" }}>
          <ChatInput
            onSend={handleSend}
            disabled={isLoading}
            placeholder={t("inputPlaceholder")}
          />
          <p
            style={{
              textAlign: "center",
              fontSize: 12,
              color: C.mutedFg,
              marginTop: 12,
              fontFamily: fontSans,
              fontWeight: 500,
            }}
          >
            {t("disclaimer")}
          </p>
        </div>
      </div>
    </main>
  );
}
