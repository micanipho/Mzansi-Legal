"use client";

import { useEffect, useState } from "react";
import { useTranslations, useLocale } from "next-intl";
import Link from "next/link";
import { ArrowLeft, Scale } from "lucide-react";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";
import { createLocalizedPath, appRoutes } from "@/i18n/routing";
import SignInForm from "@/components/auth/SignInForm";
import RegisterForm from "@/components/auth/RegisterForm";

type ActiveTab = "sign-in" | "register";

export default function AuthPage() {
  const t = useTranslations("auth");
  const locale = useLocale();
  const [activeTab, setActiveTab] = useState<ActiveTab>("sign-in");

  useEffect(() => {
    const updateTab = () => {
      setActiveTab(window.location.hash === "#register" ? "register" : "sign-in");
    };
    updateTab();
    window.addEventListener("hashchange", updateTab);
    return () => window.removeEventListener("hashchange", updateTab);
  }, []);

  const isRegister = activeTab === "register";

  return (
    <main
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        fontFamily: fontSans,
        height: "100vh",
        overflow: "hidden",
        padding: "24px",
        background: `linear-gradient(135deg, ${C.muted} 0%, ${C.bg} 100%)`,
      }}
    >
      {/* Back to Home */}
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
        onMouseEnter={(e) => (e.currentTarget.style.color = C.primary)}
        onMouseLeave={(e) => (e.currentTarget.style.color = C.mutedFg)}
      >
        <ArrowLeft size={18} />
        Back to Home
      </Link>

      {/* Brand — hidden on register to save vertical space */}
      {!isRegister && (
        <div style={{ marginBottom: 28, textAlign: "center" }}>
          <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: 12, marginBottom: 8 }}>
            <div
              style={{
                width: 44,
                height: 44,
                background: C.primary,
                borderRadius: 9999,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                flexShrink: 0,
              }}
            >
              <Scale size={22} color="#fff" />
            </div>
            <h1
              style={{
                fontFamily: fontSerif,
                fontSize: 32,
                fontWeight: 800,
                color: C.fg,
                margin: 0,
                letterSpacing: "-0.02em",
              }}
            >
              MzansiLegal
            </h1>
          </div>
          <p style={{ fontSize: 14, color: C.mutedFg, margin: 0 }}>
            Welcome back
          </p>
        </div>
      )}

      {/* Auth Card */}
      <div
        style={{
          width: "100%",
          maxWidth: 440,
          background: C.card,
          border: `1px solid ${C.border}`,
          borderRadius: 24,
          boxShadow: `0 8px 32px rgba(0, 0, 0, 0.08), ${shadowOrganic}`,
          padding: isRegister ? "20px 40px 16px" : "36px 40px 28px",
        }}
      >
        {/* Compact logo — only shown on register */}
        {isRegister && (
          <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: 10, marginBottom: 10 }}>
            <div
              style={{
                width: 32,
                height: 32,
                background: C.primary,
                borderRadius: 9999,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                flexShrink: 0,
              }}
            >
              <Scale size={16} color="#fff" />
            </div>
            <span style={{ fontFamily: fontSerif, fontWeight: 700, fontSize: 18, color: C.fg, letterSpacing: "-0.02em" }}>
              MzansiLegal
            </span>
          </div>
        )}

        {/* Card title */}
        <h2
          style={{
            fontFamily: fontSerif,
            fontSize: isRegister ? 24 : 28,
            fontWeight: 700,
            color: C.fg,
            marginBottom: 6,
            textAlign: "center",
          }}
        >
          {isRegister ? t("registerTitle") : t("signInTitle")}
        </h2>

        {/* Divider */}
        <div
          style={{
            height: 2,
            background: `linear-gradient(90deg, transparent, ${C.primary}, transparent)`,
            borderRadius: 2,
            marginBottom: isRegister ? 14 : 28,
          }}
        />

        {/* Form */}
        {isRegister ? <RegisterForm /> : <SignInForm />}

        {/* Tab switch */}
        <div style={{ textAlign: "center", marginTop: isRegister ? 10 : 20 }}>
          {isRegister ? (
            <button
              type="button"
              onClick={() => { window.location.hash = "#sign-in"; }}
              style={{
                background: "none",
                border: "none",
                color: C.primary,
                cursor: "pointer",
                fontSize: 13,
                fontFamily: fontSans,
                textDecoration: "underline",
                padding: 0,
              }}
            >
              {t("switchToSignIn")}
            </button>
          ) : (
            <button
              type="button"
              onClick={() => { window.location.hash = "#register"; }}
              style={{
                background: "none",
                border: "none",
                color: C.primary,
                cursor: "pointer",
                fontSize: 13,
                fontFamily: fontSans,
                textDecoration: "underline",
                padding: 0,
              }}
            >
              {t("switchToRegister")}
            </button>
          )}
        </div>
      </div>

      {/* Footer — hidden on register */}
      {!isRegister && (
        <p
          style={{
            marginTop: 20,
            fontSize: 13,
            color: C.mutedFg,
            textAlign: "center",
            maxWidth: 440,
          }}
        >
          By signing in, you agree to our Terms of Service and Privacy Policy
        </p>
      )}
    </main>
  );
}
