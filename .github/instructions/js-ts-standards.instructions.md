---
description: Apply to JavaScript and TypeScript files to enforce formatting, naming conventions, and syntax.
applyTo: "**/*.ts, **/*.js, **/*.js[x], **/*.ts[x]"
---
# JavaScript / TypeScript Coding Standards

- **Strict Type Enforcement**: Adhere strictly to types. Use of `any` is expressly prohibited unless well documented. 
- **Strict Evaluators**: Use triple-equals strict comparison operators (`===` and `!==`). Do not use `==` / `!=`. The keyword `eval` is entirely forbidden!
- **Code Block Formatting**: 
  - Target 4-space indentation; never use tabs. Maximum line length should not exceed 200 characters. 
  - Always enforce brackets surrounding control structures (`if`, `for`, `while`), even single line statements. 
  - Leave no trailing spaces after closing curly brackets `}` or semi-colons `;`.
- **Naming Conventions**: 
  - Do not use any abbreviation shortcuts. 
  - **Classes & Interfaces**: PascalCase (`export class MyClass`, `export interface IMyInterface`).
  - **Globals / Constants**: UPPER_CASE exclusively.
  - **Function & File Variables**: camelCase. Files should take PascalCase globally for React components and strictly kebab-case for traditional internal logic. File names MUST match class/module core exports.
- **Documentation Standards**: Utilize generous `/** ... */` block headers mapping `@param` parameters.
- **Class Organization Formats**: Classes strictly mandate ordered sequence block arrangements mapped inside logic regions (`// #region Name ... // #endregion Name`):
  1. Constants
  2. Private variables (Always start with underscore prefix e.g. `_variable`)
  3. Public variables
  4. Constructor
  5. Public methods
  6. Private methods
- **Switch Flows**: Explicitly end statements falling out of cases with `break, return, or throw`. No fall-through scenarios.