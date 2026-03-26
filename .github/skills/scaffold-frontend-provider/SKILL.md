---
name: scaffold-frontend-provider
description: Use when generating a new frontend provider, context, reducer, and actions to ensure strict architectural compliance.
---
# Scaffold Frontend Provider

## Context
This project uses a strict Redux-style context/reducer pattern for state management on the React/Nextjs frontend. Every provider must have a specific 4-file structure and follow precise naming conventions.

## Workflow

Follow these steps exactly to create a new provider:

### 1. Requirements Gathering
Ask the user for the name/type of the feature or entity that needs a provider (e.g., "Product", "Auth", "User"). Gather or infer endpoints for operations (GET, CREATE, UPDATE, DELETE).

### 2. Scaffold the Directory
Create the folder `frontend/providers/<feature>Provider/`.
Inside, generate exactly these 4 files without deviation:
- `actions.tsx`
- `context.tsx`
- `index.tsx`
- `reducer.tsx`

### 3. Implement `actions.tsx`
- Define `[Feature]ActionEnums` with `PENDING`, `SUCCESS`, and `ERROR` variants for each async operation.
- Use `createAction` (from `redux-actions`) to export specific payload setters for each enum action.

### 4. Implement `context.tsx`
- Export the primary data interface (e.g. `I[Feature]`).
- Export `I[Feature]StateContext` (booleans: `isPending`, `isSuccess`, `isError`, plus data definitions).
- Export `I[Feature]ActionContext` (invokable action methods).
- Define and export the `INITIAL_STATE` object.
- Create both `[Feature]StateContext` and `[Feature]ActionContext` using `React.createContext`.

### 5. Implement `reducer.tsx`
- Import `handleActions` from `redux-actions`.
- Map each enum case from `actions.tsx` to handle state overlays `(state, action) => ({ ...state, ...action.payload })`.
- Make sure it's initialized with `INITIAL_STATE`.

### 6. Implement `index.tsx`
- Create the generic `[Feature]Provider` component.
- Set up `const [state, dispatch] = useReducer([Feature]Reducer, INITIAL_STATE)`.
- Implement API interaction functions explicitly using the internal `axiosInstance` imported from its separate file. Do NOT import the raw `axios` package directly. Fire the correct pending/success/error dispatches sequentially.
- Return wrapped contexts: `<StateContext.Provider>` wrapping `<ActionContext.Provider>`.
- Export typed `use[Feature]State()` and `use[Feature]Actions()` convenience hooks with strict context checks (throw an error if utilized outside the provider runtime).

### 7. Guidelines for Page Construction
When requested to build a frontend page using the new provider context:
- No inline styling is allowed.
- Consume styling exclusively via the `antd-style` `createStyles` approach inside a `style.ts` file decoupled from the component.
