import type { ThemeConfig } from "antd";

/** Shared shell tokens sourced from CSS variables. */
export const C = {
  bg: "var(--ml-bg)",
  fg: "var(--ml-fg)",
  primary: "var(--ml-primary)",
  primaryFg: "var(--ml-primary-fg)",
  secondary: "var(--ml-secondary)",
  secondaryFg: "var(--ml-secondary-fg)",
  muted: "var(--ml-muted)",
  mutedFg: "var(--ml-muted-fg)",
  border: "var(--ml-border)",
  destructive: "var(--ml-destructive)",
  card: "var(--ml-card)",
  accent: "var(--ml-accent)",
  paper: "var(--ml-paper)",
} as const;

export const RGB = {
  primary: "var(--ml-primary-rgb)",
  secondary: "var(--ml-secondary-rgb)",
  accent: "var(--ml-accent-rgb)",
  muted: "var(--ml-muted-rgb)",
  border: "var(--ml-border-rgb)",
  destructive: "var(--ml-destructive-rgb)",
  fg: "var(--ml-fg-rgb)",
  bg: "var(--ml-bg-rgb)",
} as const;

export function alpha(rgbValue: string, opacity: number): string {
  return `rgba(${rgbValue}, ${opacity})`;
}

/** Organic border radii */
export const R = {
  o1: "32px 16px 24px 32px",
  o2: "16px 32px 32px 24px",
  o3: "24px 24px 16px 32px",
  o4: "32px 32px 16px 24px",
} as const;

export const shadowOrganic = "var(--ml-shadow-organic)";
export const fontSerif = "var(--ml-font-serif)";
export const fontSans = "var(--ml-font-sans)";

/** Kept for components that still import `brand` */
export const brand = {
  dark: C.fg,
  cta: C.primary,
  ctaHover: "var(--ml-primary-strong)",
  cream: C.bg,
  cardBg: C.card,
  border: C.border,
  borderLight: C.accent,
} as const;

export const antdTheme: ThemeConfig = {
  cssVar: {
    key: "mzansilegal",
    prefix: "ml",
  },
  token: {
    colorPrimary: "#0d7377",
    colorSuccess: "#0d7377",
    colorWarning: C.secondary,
    colorError: C.destructive,
    colorInfo: "#1677ff",
    colorText: C.fg,
    colorTextSecondary: C.mutedFg,
    colorBgLayout: C.bg,
    colorBgContainer: C.card,
    colorBorder: C.border,
    colorBorderSecondary: C.accent,
    fontFamily: fontSans,
    fontSize: 15,
    borderRadius: 8,
    borderRadiusLG: 12,
    borderRadiusSM: 4,
    boxShadow: shadowOrganic,
  },
  components: {
    Button: {
      borderRadius: 24,
      paddingInline: 20,
      primaryColor: C.primaryFg,
    },
    Card: {
      borderRadius: 16,
      colorBorderSecondary: C.border,
    },
    Input: {
      borderRadius: 24,
      colorBgContainer: C.card,
    },
    Tag: {
      borderRadius: 24,
      fontSize: 11,
    },
  },
};
