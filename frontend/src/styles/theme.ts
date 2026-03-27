import type { ThemeConfig } from "antd";

/** MzansiLegal design tokens — matched to reference implementation */

export const C = {
  bg:          '#FDFCF8',
  fg:          '#2C2C24',
  primary:     '#5D7052',
  primaryFg:   '#F3F4F1',
  secondary:   '#C18C5D',
  secondaryFg: '#FFFFFF',
  muted:       '#F0EBE5',
  mutedFg:     '#78786C',
  border:      '#DED8CF',
  destructive: '#A85448',
  card:        '#FEFEFA',
  accent:      '#E6DCCD',
} as const;

/** Organic border radii */
export const R = {
  o1: '32px 16px 24px 32px',
  o2: '16px 32px 32px 24px',
  o3: '24px 24px 16px 32px',
  o4: '32px 32px 16px 24px',
} as const;

export const shadowOrganic = '0 4px 20px rgba(93, 112, 82, 0.15)';
export const fontSerif = "'Fraunces', serif";
export const fontSans  = "'Nunito', sans-serif";

/** Kept for components that still import `brand` */
export const brand = {
  dark:        C.fg,
  cta:         C.primary,
  ctaHover:    '#4d6045',
  cream:       C.bg,
  cardBg:      C.card,
  border:      C.border,
  borderLight: C.accent,
} as const;

export const antdTheme: ThemeConfig = {
  token: {
    colorPrimary:          C.primary,
    colorSuccess:          '#5D7052',
    colorWarning:          C.secondary,
    colorError:            C.destructive,
    colorInfo:             '#1677ff',
    colorText:             C.fg,
    colorTextSecondary:    C.mutedFg,
    colorBgLayout:         C.bg,
    colorBgContainer:      C.card,
    colorBorder:           C.border,
    colorBorderSecondary:  C.accent,
    fontFamily:            fontSans,
    fontSize:              15,
    borderRadius:          8,
    borderRadiusLG:        12,
    borderRadiusSM:        4,
    boxShadow:             shadowOrganic,
  },
  components: {
    Button: {
      borderRadius:  24,
      paddingInline: 20,
      primaryColor:  C.primaryFg,
    },
    Card: {
      borderRadius:         16,
      colorBorderSecondary: C.border,
    },
    Input: {
      borderRadius:    24,
      colorBgContainer: C.card,
    },
    Tag: {
      borderRadius: 24,
      fontSize:     11,
    },
  },
};
