import type { ThemeConfig } from "antd";

/**
 * MzansiLegal Design Tokens
 *
 * Primary brand: deep justice green
 * Secondary: warm gold (South African vibrancy)
 * Neutral: accessible grays for long-form legal text
 */

export const designTokens = {
  colorPrimary: "#006B3F", // South African green (ANC/nature association)
  colorSuccess: "#52c41a",
  colorWarning: "#faad14", // Gold — used for amber flags
  colorError: "#f5222d", // Red — used for red flags in contracts
  colorInfo: "#1677ff",

  // Text
  colorText: "#1a1a1a",
  colorTextSecondary: "#595959",
  colorTextTertiary: "#8c8c8c",

  // Backgrounds
  colorBgBase: "#ffffff",
  colorBgLayout: "#f5f5f0", // Slightly warm white

  // Border
  colorBorder: "#d9d9d9",
  colorBorderSecondary: "#f0f0f0",

  // Font
  fontFamily:
    "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
  fontSize: 16, // Larger base for accessibility
  fontSizeLG: 18,
  fontSizeSM: 14,
  fontSizeHeading1: 38,
  fontSizeHeading2: 30,
  fontSizeHeading3: 24,

  // Spacing
  borderRadius: 8,
  borderRadiusLG: 12,
  borderRadiusSM: 4,

  // Shadows
  boxShadow:
    "0 1px 3px 0 rgba(0,0,0,0.1), 0 1px 2px -1px rgba(0,0,0,0.1)",
  boxShadowSecondary:
    "0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -2px rgba(0,0,0,0.1)",

  // Motion
  motionDurationFast: "0.1s",
  motionDurationMid: "0.2s",
  motionDurationSlow: "0.3s",
} as const;

export const antdTheme: ThemeConfig = {
  token: {
    colorPrimary: designTokens.colorPrimary,
    colorSuccess: designTokens.colorSuccess,
    colorWarning: designTokens.colorWarning,
    colorError: designTokens.colorError,
    colorInfo: designTokens.colorInfo,

    fontFamily: designTokens.fontFamily,
    fontSize: designTokens.fontSize,

    borderRadius: designTokens.borderRadius,
    borderRadiusLG: designTokens.borderRadiusLG,
    borderRadiusSM: designTokens.borderRadiusSM,

    colorBgLayout: designTokens.colorBgLayout,
  },
  components: {
    Button: {
      borderRadius: designTokens.borderRadius,
      fontWeight: 600,
    },
    Card: {
      borderRadius: designTokens.borderRadiusLG,
      boxShadow: designTokens.boxShadow,
    },
    Layout: {
      headerBg: "#ffffff",
      siderBg: "#ffffff",
    },
    Menu: {
      itemBorderRadius: designTokens.borderRadius,
    },
    Tag: {
      borderRadius: designTokens.borderRadius,
    },
    Input: {
      borderRadius: designTokens.borderRadius,
      fontSize: designTokens.fontSize,
    },
    Select: {
      borderRadius: designTokens.borderRadius,
    },
    Alert: {
      borderRadius: designTokens.borderRadiusLG,
    },
    Progress: {
      defaultColor: designTokens.colorPrimary,
    },
    Typography: {
      fontFamily: designTokens.fontFamily,
    },
  },
};

/** CSS custom properties for dyslexia-friendly mode */
export const dyslexiaCssVars = {
  "--font-family": "'OpenDyslexic', sans-serif",
  "--letter-spacing": "0.1em",
  "--word-spacing": "0.2em",
  "--line-height": "1.8",
  "--font-size-base": "17px",
} as const;

/** CSS variable names for runtime theming */
export const cssVars = {
  fontFamily: "--ml-font-family",
  letterSpacing: "--ml-letter-spacing",
  lineHeight: "--ml-line-height",
} as const;
