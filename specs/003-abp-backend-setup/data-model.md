# Data Model: ABP Backend Foundation Setup

**Feature**: 003-abp-backend-setup
**Phase**: 1 — Design

---

## Overview

The data model for this feature is the ABP Zero baseline schema. No new application entities are
introduced. The goal is to ensure the ABP Zero-managed entities are correctly mapped to PostgreSQL.

All entities below are owned by ABP Zero and are **not** defined in this project's domain layer.
They are registered automatically by `AbpZeroDbContext<Tenant, Role, User, TDbContext>`.

---

## ABP Zero Baseline Entities

| Entity | Table | Notes |
|---|---|---|
| `Edition` | `AbpEditions` | SaaS subscription tiers |
| `Tenant` | `AbpTenants` | Multi-tenancy isolation |
| `TenantNotificationInfo` | `AbpTenantNotifications` | |
| `UserNotificationInfo` | `AbpUserNotifications` | |
| `NotificationInfo` | `AbpNotifications` | |
| `NotificationSubscriptionInfo` | `AbpNotificationSubscriptions` | |
| `OrganizationUnit` | `AbpOrganizationUnits` | |
| `UserOrganizationUnit` | `AbpUserOrganizationUnits` | |
| `Role` | `AbpRoles` | Extended by project `Role` class |
| `User` | `AbpUsers` | Extended by project `User` class |
| `UserLogin` | `AbpUserLogins` | |
| `UserRole` | `AbpUserRoles` | |
| `UserClaim` | `AbpUserClaims` | |
| `UserToken` | `AbpUserTokens` | |
| `RoleClaim` | `AbpRoleClaims` | |
| `PermissionSetting` | `AbpPermissions` | |
| `Setting` | `AbpSettings` | Key-value application settings |
| `AuditLog` | `AbpAuditLogs` | Full audit trail |
| `BackgroundJobInfo` | `AbpBackgroundJobs` | |
| `UserAccount` | `AbpUserAccounts` | |
| `LanguageInfo` | `AbpLanguages` | |
| `ApplicationLanguageText` | `AbpLanguageTexts` | |
| `EntityChange` | `AbpEntityChanges` | |
| `EntityChangeSet` | `AbpEntityChangeSets` | |
| `EntityPropertyChange` | `AbpEntityPropertyChanges` | |
| `WebhookEvent` | `AbpWebhookEvents` | |
| `WebhookSubscriptionInfo` | `AbpWebhookSubscriptions` | |
| `WebhookSendAttempt` | `AbpWebhookSendAttempts` | |
| `DynamicProperty` | `AbpDynamicProperties` | |
| `DynamicPropertyValue` | `AbpDynamicPropertyValues` | |
| `DynamicEntityProperty` | `AbpDynamicEntityProperties` | |
| `DynamicEntityPropertyValue` | `AbpDynamicEntityPropertyValues` | |

---

## PostgreSQL Type Mapping

EF Core with Npgsql maps ABP Zero column types as follows:

| C# Type | SQL Server Type (old) | PostgreSQL Type (new) |
|---|---|---|
| `string` (bounded) | `nvarchar(N)` | `character varying(N)` |
| `string` (unbounded) | `nvarchar(max)` | `text` |
| `DateTime` | `datetime2` | `timestamp without time zone` |
| `DateTime?` | `datetime2` | `timestamp without time zone` |
| `bool` | `bit` | `boolean` |
| `int` (identity) | `int IDENTITY` | `integer` (serial / `GENERATED ALWAYS AS IDENTITY`) |
| `long` | `bigint` | `bigint` |
| `Guid` | `uniqueidentifier` | `uuid` |

These mappings are applied automatically by Npgsql's EF Core provider when the migration is
regenerated — no manual column type declarations are needed.

---

## Seed Data (unchanged from ABP Zero defaults)

| Seed Record | Value |
|---|---|
| Default Edition | `Standard` |
| Default Tenant | `Default` |
| Admin Role | `Admin` (static, not deletable) |
| Default Role | `Citizen` (assigned to new users) |
| Admin User | `admin` / `123qwe` (dev default; change in production) |
| Default Language | `en` (English) |
