---
description: Apply to Next.js and React frontend code to enforce functional components, state management, and optimized rendering.
applyTo: "**/*.tsx, **/*.ts, **/*.jsx, **/*.js"
---
# Next.js and React Coding Standards

- **Core Paradigm**: Favor functional and immutable styles (map/filter/reduce) over OOP or mutable loops. Adhere heavily to the 'return early' pattern. Do NOT use classes unless necessary.
- **Component Design**: 
  - Keep components purely presentational, uncomplicated, composable. Keep accessibility (a11y) in mind (e.g. keyboard navigation, aria-* properties).
- **Props Naming**: Boolean props must be prefixed logically using words like `does`, `has`, `is`, `should` (e.g., `isDisabled` rather than `disabled`).
- **Styling Architecture**: 
  - Strictly utilize `antd-style` (`createStyles`, `css`). 
  - Do NOT inject inline styles natively via `<div style={{...}}>`. 
  - Prefer using Tailwind utility classes cleanly mapped. Always consume standard design guideline color tokens instead of hardcoding raw hex/rgb.
- **Next.js Conventions**:
  - Prefer Static Site Generation (`getStaticProps`) for performant static deployments. Restrict Server-Side Rendering (`getServerSideProps`) exclusively to dynamic prerequisites.
  - Utilize Next `<Image>` API for lazy-loaded assets. 
  - Manage bundle sizes closely using dynamic imports (`next/dynamic`). Keep third-party libraries minimal.
- **State Management Context**:
  - Store domain state closely to where it is consumed (colocation). 
  - Keep Context Providers slim. Decouple `state` and `dispatch` context providers to optimize redundant application re-renders. Use Provider pattern strictly defining standard actions/reducers contexts.
- **Optimizations**: Avoid premature optimization. `useMemo`, `React.memo`, and `useCallback` require measurable validation to prove functional optimization gains relative to primitive dependency dependencies.