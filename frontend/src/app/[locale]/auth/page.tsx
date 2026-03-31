"use client";

import { useEffect, useState } from "react";
import { useTranslations, useLocale } from "next-intl";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";
import { createLocalizedPath, appRoutes } from "@/i18n/routing";
import SignInForm from "@/components/auth/SignInForm";
import RegisterForm from "@/components/auth/RegisterForm";

type ActiveTab = "sign-in" | "register";

export default function AuthPage() {
  const t = useTranslations("auth");
  const locale = useLocale();
  const [activeTab, setActiveTab] = useState<ActiveTab>("sign-in");

  // Determine active tab from URL hash
  useEffect(() => {
    const updateTab = () => {
      const hash = window.location.hash;
      if (hash === "#register") {
        setActiveTab("register");
      } else {
        setActiveTab("sign-in");
      }
    };

    updateTab();
    window.addEventListener("hashchange", updateTab);
    return () => window.removeEventListener("hashchange", updateTab);
  }, []);

  const switchToRegister = () => {
    window.location.hash = "#register";
  };

  const switchToSignIn = () => {
    window.location.hash = "#sign-in";
  };

  return (
    <main
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        fontFamily: fontSans,
        minHeight: "100vh",
        padding: "24px",
        background: `linear-gradient(135deg, ${C.muted} 0%, ${C.bg} 100%)`,
      }}
    >
      {/* Back to Home link */}
      <Link
        href={createLocalizedPath(locale, appRoutes.home)}
        style={{
          position: "absolute",
          top: 24,
          left: 24,
          display: "flex",
          alignItems: "center",
          gap: 8,
          color: C.mutedFg,
          textDecoration: "none",
          fontSize: 14,
          fontWeight: 500,
          transition: "color 0.2s ease",
        }}
        onMouseEnter={(e) => e.currentTarget.style.color = C.primary}
        onMouseLeave={(e) => e.currentTarget.style.color = C.mutedFg}
      >
        <ArrowLeft size={18} />
        Back to Home
      </Link>

      {/* Logo / Brand */}
      <div
        style={{
          marginBottom: 32,
          textAlign: "center",
        }}
      >
        <h1
          style={{
            fontFamily: fontSerif,
            fontSize: 36,
            fontWeight: 800,
            color: C.primary,
            margin: 0,
            letterSpacing: "-0.02em",
          }}
        >
          MzansiLegal
        </h1>
        <p
          style={{
            fontSize: 14,
            color: C.mutedFg,
            margin: "4px 0 0",
          }}
        >
          {activeTab === "sign-in" ? "Welcome back" : "Join our community"}
        </p>
      </div>

      {/* Auth Card */}
      <div
        style={{
          width: "100%",
          maxWidth: 440,
          background: C.card,
          border: `1px solid ${C.border}`,
          borderRadius: 24,
          boxShadow: `0 8px 32px rgba(0, 0, 0, 0.08), ${shadowOrganic}`,
          padding: "40px 40px 32px",
        }}
      >
        {/* Title */}
        <h2
          style={{
            fontFamily: fontSerif,
            fontSize: 28,
            fontWeight: 700,
            color: C.fg,
            marginBottom: 8,
            textAlign: "center",
          }}
        >
          {activeTab === "sign-in" ? t("signInTitle") : t("registerTitle")}
        </h2>

        {/* Divider */}
        <div
          style={{
            height: 2,
            background: `linear-gradient(90deg, transparent, ${C.primary}, transparent)`,
            borderRadius: 2,
            marginBottom: 32,
          }}
        />

        {/* Active form */}
        {activeTab === "sign-in" ? (
          <SignInForm />
        ) : (
          <RegisterForm />
        )}

        {/* Tab switch link */}
        <div style={{ textAlign: "center", marginTop: 24 }}>
          {activeTab === "sign-in" ? (
            <button
              type="button"
              onClick={switchToRegister}
              style={{
                background: "none",
                border: "none",
                color: C.primary,
                cursor: "pointer",
                fontSize: 14,
                fontFamily: fontSans,
                textDecoration: "underline",
                padding: 0,
              }}
            >
              {t("switchToRegister")}
            </button>
          ) : (
            <button
              type="button"
              onClick={switchToSignIn}
              style={{
                background: "none",
                border: "none",
                color: C.primary,
                cursor: "pointer",
                fontSize: 14,
                fontFamily: fontSans,
                textDecoration: "underline",
                padding: 0,
              }}
            >
              {t("switchToSignIn")}
            </button>
          )}
        </div>
      </div>

      {/* Footer text */}
      <p
        style={{
          marginTop: 24,
          fontSize: 13,
          color: C.mutedFg,
          textAlign: "center",
          maxWidth: 440,
        }}
      >
        By signing in, you agree to our Terms of Service and Privacy Policy
      </p>
    </main>
  );
}
