# Quickstart: MzansiLegal Platform

## Backend Setup (.NET 9 + ABP)
1.  **Prerequisites**: .NET 8 SDK, PostgreSQL, OpenAI API Key.
2.  **Configuration**: Update `appsettings.json` with `ConnectionStrings:Default` and `OpenAI:ApiKey`.
3.  **Migrations**: Run `dotnet ef database update` in the `EntityFrameworkCore` project.
4.  **Seed Data**: Run the `DbMigrator` project to seed Categories and initial 13 legislation documents.
5.  **Run Host**: `dotnet run` in the `HttpApi.Host` project.

## Frontend Setup (Next.js 14)
1.  **Prerequisites**: Node.js 18+, npm/pnpm.
2.  **Configuration**: Copy `.env.local.example` to `.env.local` and set `NEXT_PUBLIC_BASE_URL`.
3.  **Install**: `pnpm install` or `npm install`.
4.  **Run Dev**: `npm run dev`.

## Developer Workflows
- **PDF Ingestion**: Use the Admin dashboard `/admin/document/upload` to test new legislation Act ingestion.
- **Accessibility Check**: Toggle "Dyslexia Mode" in `/settings` to verify font/spacing shifts.
- **Multilingual Q&A**: Use the `/ask` interface to test Q&A in `zu`, `st`, and `af`.

## Testing
- **Backend**: `dotnet test` from the root `backend/` directory.
- **Frontend**: `npm run test` or `npx playwright test` for E2E flows.
