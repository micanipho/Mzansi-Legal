# Quickstart: ABP Backend Foundation Setup

**Feature**: 003-abp-backend-setup

---

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 9.0.x | `dotnet --version` to verify — tested with 9.0 |
| PostgreSQL | 15+ | Must be running and accessible — tested locally with PostgreSQL 15 |
| EF Core CLI | 9.0.5 | `dotnet tool install --global dotnet-ef` |
| Npgsql.EFCore | 9.0.4 | Already in `backend.EntityFrameworkCore.csproj` |

---

## 1. Clone and Navigate

```bash
git clone <repo-url>
cd Mzansi-legal/backend
```

---

## 2. Configure the Connection String

Edit `src/backend.Web.Host/appsettings.json` and `src/backend.Migrator/appsettings.json`.
Replace the `Default` connection string with your PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=MzansiLegalDb;Username=postgres;Password=your_password"
  }
}
```

> **Do not commit real credentials.** Both files ship with the placeholder `YOUR_LOCAL_PASSWORD`.
> Override via a gitignored `appsettings.Development.json` (already in `.gitignore`) or environment variables.
>
> **Environment variables expected by the host** (set before running `dotnet run`):
> - `ConnectionStrings__Default` — full Npgsql connection string
> - `Authentication__JwtBearer__SecurityKey` — JWT signing key (min 16 chars)

**Example `appsettings.Development.json`** (gitignored, never commit):
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=MzansiLegalDb;Username=postgres;Password=realpassword"
  },
  "Authentication": {
    "JwtBearer": {
      "SecurityKey": "a-long-random-secret-key-at-least-16-chars"
    }
  }
}
```

---

## 3. Run Migrations

From the `backend/` folder, run the migration runner:

```bash
dotnet run --project src/backend.Migrator
```

Expected output:
```
Host database migration started...
Host database migration completed, seeding...
Host database seeds completed.
```

Connect to the database with a PostgreSQL client and confirm the `AbpUsers`, `AbpRoles`, and
`AbpSettings` tables exist and contain seed rows.

---

## 4. Start the API Host

```bash
dotnet run --project src/backend.Web.Host
```

Expected output:
```
Now listening on: https://localhost:44311
Application started.
```

---

## 5. Verify in the Browser

1. Open `https://localhost:44311/swagger` — the Swagger UI MUST load.
2. Call `GET /api/HealthCheck/Get` via Swagger — response MUST be `{ "success": true }`.
3. Call `POST /api/TokenAuth/Authenticate` with `admin` / `123qwe` — response MUST include
   an `accessToken`.

---

## Implementation Notes

- **Npgsql version used**: `9.0.4` — exact match tested against EF Core `9.0.5`
- **PostgreSQL version tested**: 15+
- **Dev certificates**: Run `dotnet dev-certs https --trust` once if the browser shows an SSL error on `https://localhost:44311`
- **Stale imports removed**: Pre-existing `using Microsoft.EntityFrameworkCore;` in `backend.Core` and `backend.Application` were unused stale template imports — removed during compliance verification (T017–T018)
- **Migration regenerated**: All SQL Server migrations deleted; `InitialCreate` regenerated for PostgreSQL — confirms `NpgsqlModelBuilderExtensions` and `MaxIdentifierLength: 63`

---

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `28P01: password authentication failed` | Wrong credentials in connection string | Re-check `Username` and `Password` |
| `3D000: database does not exist` | DB not yet created | Create the DB or let the migrator create it automatically |
| SSL certificate error in browser | Self-signed dev certificate | Run `dotnet dev-certs https --trust` |
| `Npgsql.NpgsqlException: Connection refused` | PostgreSQL not running | Start the PostgreSQL service |
