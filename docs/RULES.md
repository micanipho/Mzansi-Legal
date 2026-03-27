# C# Coding Rules

Rules to follow when writing or reviewing C# code for this project.

---

## Code Readability

- Code must read like a novel — clear intent without excessive head-scratching.
- All classes must have a comment describing their purpose, unless self-evident to a junior developer new to the project.
- All public methods must have a comment describing their purpose.
- Any non-obvious logic or assumptions must be preceded by a comment explaining the logic or assumption.

---

## Class and Method Length

- Classes must not exceed **350 lines**. Longer classes likely violate the Single Responsibility Principle and must be refactored.
- Methods must not require vertical scrolling to read in full. Extract sections into named sub-methods to improve readability.

---

## Class Member Organization

- Arrange class members logically to minimize jumping around:
  - Group related properties together; place the most significant ones at the top.
  - If methods are meant to be called in a sequence, arrange them in that order.
  - Sub-methods called exclusively by a parent method must be placed immediately after the parent method.
- Use `#region` to organize longer classes, especially to group sub-methods under their parent.

---

## Guard Clauses

- Use guard clauses to validate all preconditions at the top of complex methods.
- Use `Ardalis.GuardClauses` as the standard guard clause library.
- Do not perform precondition checks deep inside method bodies.

---

## Nesting

- Do not nest conditional (`if`, `switch`) or looping (`while`, `foreach`) statements more than **two levels deep**.
- At three or more levels of nesting, refactor by extracting into a separate method or applying early return.

---

## Naming

- Use clear, descriptive variable and method names with **no spelling mistakes**.
- Be consistent with naming conventions throughout the codebase.
- Take time to find the right name — clarity and intent matter more than speed.

---

## Variable Declarations

- Use `var` when the type is clear from the right-hand side of the assignment.
- Use explicit types when the type is not immediately obvious or when it improves clarity.
- Examples:
  - ✓ `var customer = new Customer();` — clear
  - ✓ `var products = new List<Product>();` — clear
  - ✗ `var result = ProcessOrder(order);` — unclear, use explicit type
  - ✗ `var price = 19.99;` — ambiguous (double? decimal?), use explicit type

---

## Magic Numbers

- Do not use magic numbers (unexplained numeric literals) in code.
- Replace them with named `const` values or `enum` types.

---

## DRY – Don't Repeat Yourself

- Do not copy and paste code. If logic appears more than once, extract it into a reusable method.
- Duplicated code creates bloat and is difficult to maintain.

---

## Simplicity (KISS)

- Write code as simply and directly as possible.
- Fewer lines is generally better, but do not sacrifice readability for brevity.
- If a single conditional or LINQ statement becomes convoluted with too many conditions, expand it for clarity.

---

## Formatting

- Always format code before committing:
  - `Ctrl+E, Ctrl+D` — format entire document.
  - `Ctrl+E, Ctrl+F` — format selection.
- Poorly formatted code is not acceptable.

---

## Dead Code

- Delete all unused code — methods, classes, variables, and commented-out blocks.
- Unused code adds clutter and hinders maintainability. Source control preserves history; deletion is safe.

---

## Performance

- Do not use loops that cause multiple database calls. Consolidate into a single query where possible.
- Avoid looping to perform multiple updates — use bulk update statements instead.
- Add database indexes for queries expected to run frequently.

---

## Solution Structure

- Follow the Clean Architecture solution template at all times.
- Place every class in the correct layer — do not place domain logic in the AppService layer.
- Do not place DTO classes in the domain layer. Place DTOs in a `Dtos` folder next to the AppService that uses them.
  - Exception: a DTO used as a parameter to a domain service may live in the same folder as that domain service.
- Single-purpose DTO naming convention: `{EndpointName}Request` — e.g. `CreateFieldServiceProfileRequest`.


#