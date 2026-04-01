---
name: add-styling
description: Use this skill when styling any page, component, or layout in the GovLeave frontend. Covers the mandatory antd-style createStyles pattern, design token usage, and the styles folder structure that every route must follow.
---

# GovLeave — Styling with antd-style

## Core Rule

Every page route MUST have a `styles/style.ts` file. Styles are never written inline (beyond trivial one-liners) and never in plain CSS modules.

```
app/(dashboard)/employees/
├── page.tsx           ← imports useStyles from ./styles/style
└── styles/
    └── style.ts       ← all styles live here using createStyles
```

---

## createStyles Pattern (CANONICAL)

```typescript
// styles/style.ts
import { createStyles } from 'antd-style';

export const useStyles = createStyles(({ token, css }) => ({
  // Use token.* for ALL values — never hardcode px or hex
  pageContainer: css`
    padding: ${token.paddingLG}px;
    background: ${token.colorBgLayout};
    min-height: 100vh;
  `,

  headerRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: ${token.marginLG}px;
  `,

  tableCard: css`
    border-radius: ${token.borderRadiusLG}px;
    box-shadow: ${token.boxShadow};
  `,

  statusBadge: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
  `,
}));
```

```typescript
// page.tsx
"use client";
import { useStyles } from './styles/style';

export default function EmployeesPage() {
  const { styles } = useStyles();

  return (
    <div className={styles.pageContainer}>
      <div className={styles.headerRow}>
        {/* content */}
      </div>
    </div>
  );
}
```

---

## Design Tokens Reference

Always use these — never hardcode values:

```typescript
// Spacing
token.paddingSM        // 12px
token.padding          // 16px
token.paddingMD        // 20px
token.paddingLG        // 24px
token.paddingXL        // 32px

token.marginSM         // 12px
token.margin           // 16px
token.marginMD         // 20px
token.marginLG         // 24px
token.marginXL         // 32px

// Colors
token.colorPrimary           // brand blue
token.colorBgContainer       // white in light, dark in dark
token.colorBgLayout          // page background
token.colorBgElevated        // card/modal background
token.colorText              // primary text
token.colorTextSecondary     // muted text
token.colorBorder            // border color
token.colorSuccess           // green
token.colorWarning           // orange
token.colorError             // red

// Typography
token.fontSize               // 14px base
token.fontSizeSM             // 12px
token.fontSizeLG             // 16px
token.fontSizeXL             // 20px
token.fontWeightStrong       // 600

// Shape
token.borderRadius           // 6px
token.borderRadiusLG         // 8px
token.borderRadiusSM         // 4px

// Elevation
token.boxShadow              // card shadow
token.boxShadowSecondary     // subtle shadow
```

---

## Rules

| Rule | Wrong | Correct |
|---|---|---|
| Colors | `color: '#1677ff'` | `color: ${token.colorPrimary}` |
| Spacing | `padding: '24px'` | `padding: ${token.paddingLG}px` |
| Radius | `borderRadius: '8px'` | `borderRadius: ${token.borderRadiusLG}px` |
| Inline styles | `style={{ padding: 24 }}` | `className={styles.myClass}` |
| CSS files | `import './page.css'` | Never — use createStyles |
| Tailwind | `className="p-6 flex"` | Never — use createStyles |

---

## Component Styling (no page needed)

For components that aren't pages, create a `styles.ts` next to the component:

```
components/employees/
├── EmployeeTable.tsx
└── styles.ts          ← component-level styles
```

---

## Common Page Layout Pattern

```typescript
// styles/style.ts for a typical list page
export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    padding: ${token.paddingLG}px;
    background: ${token.colorBgLayout};
    min-height: 100vh;
  `,
  toolbar: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: ${token.marginLG}px;
  `,
  title: css`
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0;
  `,
  card: css`
    border-radius: ${token.borderRadiusLG}px;
    box-shadow: ${token.boxShadow};
  `,
  emptyState: css`
    padding: ${token.paddingXL}px;
    text-align: center;
    color: ${token.colorTextSecondary};
  `,
}));
```

---

## antd Components to Use (preferred)

| Need | Component |
|---|---|
| Data table | `<Table>` |
| Form | `<Form>` + `<Form.Item>` |
| Modal / drawer | `<Modal>` or `<Drawer>` |
| Page layout | `<Layout>`, `<Sider>`, `<Content>` |
| Cards | `<Card>` |
| Status labels | `<Tag color="green">` |
| Notifications | `message.success()`, `message.error()` |
| Confirm delete | `<Popconfirm>` |
| Loading state | `<Spin>` or `<Skeleton>` |
| Empty state | `<Empty>` |
| Breadcrumb | `<Breadcrumb>` |
| Stats | `<Statistic>` |

