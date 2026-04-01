# Group Providers

Discover all provider components in the current project and generate or update the single composition file that wraps them all together.

## Arguments

`$ARGUMENTS` — optional. If provided, treat as a space-separated list of provider import paths or names to include/exclude (e.g. `--exclude ThemeProvider` or `--include AuthProvider ClientProvider`). If omitted, auto-discover all providers.

## Instructions

### Step 1 — Discover the providers directory

Search for a `providers/` directory. Common locations (check in order):
- `src/providers/`
- `app/providers/`
- `providers/`
- `lib/providers/`

If none exists, ask the user where providers live before continuing.

### Step 2 — Identify the composition file

Look for an existing composition file inside that directory. Common names:
- `index.tsx` / `index.ts`
- `AppProviders.tsx`
- `Providers.tsx`

If found, read it so you understand the current structure. If not found, you will create it.

### Step 3 — Discover individual provider files

Scan every `.tsx` / `.ts` file in the providers directory (non-recursively first, then one level deep). For each file that is **not** the composition file:
- Check whether it exports a React component whose name ends in `Provider` (e.g. `export const AuthProvider`, `export function ThemeProvider`, `export default XProvider`).
- Collect: `{ exportName, filePath, isDefaultExport }`.

Skip files that export only hooks or context objects without a wrapping Provider component.

### Step 4 — Apply argument filters

If `--exclude` names were given, remove those providers from the list.
If `--include` names were given, keep only those providers (preserve discovery order).

### Step 5 — Determine nesting order

Use this priority heuristic (outermost → innermost):
1. Auth / Session providers (names containing `Auth`, `Session`, `User`)
2. Theme / Style providers (names containing `Theme`, `Style`, `Design`)
3. Config / Settings providers
4. Data / Query providers (names containing `Query`, `Cache`, `Data`)
5. Domain feature providers (everything else, alphabetical)

If the existing composition file already defines an order, **preserve that order** and only append new providers at the innermost position before `{children}`.

### Step 6 — Generate the composition file

Produce a file with the following shape (adapt to the project's conventions — use `'use client'` only if other provider files use it, match the existing import style):

```tsx
'use client';

import React from 'react';
import { ProviderA } from './providerA';
import { ProviderB } from './providerB';
// … one import per discovered provider

interface AppProvidersProps {
  children: React.ReactNode;
}

export const AppProviders: React.FC<AppProvidersProps> = ({ children }) => {
  return (
    <ProviderA>
      <ProviderB>
        {/* … nested in determined order … */}
        {children}
        {/* … closing tags … */}
      </ProviderB>
    </ProviderA>
  );
};
```

Rules:
- Use named exports only (no default export) unless the existing file uses a default export.
- Keep the component name `AppProviders` unless the existing file uses a different name — in that case preserve the existing name.
- Do not import or wrap providers that are already composed *inside* another provider in this list (i.e. avoid double-wrapping).
- Do not add any logic, state, or side-effects to this file — it is a pure composition.

### Step 7 — Write or update the file

- If the composition file already exists, update it in place using precise edits. Preserve any comments that explain ordering decisions.
- If it does not exist, create it at `<providers-dir>/index.tsx`.

### Step 8 — Verify usage

Search for where `AppProviders` (or the existing composition component name) is consumed — typically in:
- `src/app/layout.tsx`
- `app/layout.tsx`
- `pages/_app.tsx`
- `src/main.tsx` / `main.tsx`

If the composition file was newly created, inform the user where to import and use it.
If it was updated, confirm no changes are needed at the consumption site.

### Step 9 — Report

Print a concise summary:
- List of providers included (in nesting order, outermost first)
- Path of the written file
- Any providers that were skipped and why
- Any action the user needs to take manually
