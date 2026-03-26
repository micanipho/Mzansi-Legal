---
description: Apply to all C# backend code to ensure adherence to Clean Architecture, Single Responsibility, and coding conventions.
applyTo: "**/*.cs"
---
# C# Coding Standards

- **Class Length**: Keep classes under 350 lines to respect Single Responsibility. Keep methods short and vertically visible.
- **Comment Standards**: All classes and public methods must have comments explaining their purpose. Complex logic or non-obvious assumptions requires preceding comments.
- **Regions**: Use `#region` and `#endregion` to organize code, especially in longer classes (e.g. grouping sub-methods).
- **Architecture (Clean Architecture)**: 
  - DO NOT put domain logic in AppServices.
  - DO NOT place DTOs in the domain layer. DTOs belong in a `Dtos` folder alongside their AppService.
- **DTO Naming**: Request DTOs handling a single function should use the `{EndPointName}Request` suffix (e.g. `CreateFieldServiceProfileRequest`).
- **Guard Clauses**: Use `Ardalis.GuardClauses` early in a method block to enforce precondition checks cleanly.
- **Control Flow**: Avoid nesting deeper than 3 layers. Employ the early-return principle or refactor into extracted sub-methods.
- **Member Ordering**: Place significant properties at the top. Sub-methods exclusively used by a parent should follow immediately below the parent.
- **Performance**: Use batched/bulk updates and flatten entities via joins instead of looping through and executing individual DB queries.

Ensure clean code formatting (Ctrl+K, Ctrl+D in Visual Studio) and permanently delete dead or unused code blocks.