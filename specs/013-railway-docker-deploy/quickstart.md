# Quickstart: Railway Docker Deployment

**Branch**: `013-railway-docker-deploy` | **Date**: 2026-03-30

---

## Prerequisites

- [Railway CLI](https://docs.railway.app/develop/cli) installed (`npm install -g @railway/cli` or via installer)
- Docker Desktop running locally
- `.NET 9 SDK` installed
- A Railway account (free tier is sufficient for initial deployment)
- OpenAI API key

---

## Step 1: Verify the Docker Build Locally

Before pushing to Railway, confirm the updated Dockerfile builds correctly from the `backend/` directory.

```bash
# From the repo root
cd backend
docker build -t mzansi-legal-backend:local .
docker run -p 8080:80 \
  -e ASPNETCORE_URLS="http://+:80" \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__Default="Host=host.docker.internal;Database=MzansiLegalDb;Username=postgres;Password=postgres" \
  -e Authentication__JwtBearer__SecurityKey="your-local-jwt-key-min-32-chars!!" \
  -e Authentication__JwtBearer__Issuer="backend" \
  -e Authentication__JwtBearer__Audience="backend" \
  -e OpenAI__ApiKey="sk-..." \
  -e OpenAI__BaseUrl="https://api.openai.com/" \
  -e App__ServerRootAddress="http://localhost:8080/" \
  -e App__CorsOrigins="http://localhost:3000" \
  mzansi-legal-backend:local
```

Visit `http://localhost:8080/api/health` — expect `{"status":"healthy"}`.

---

## Step 2: Create a Railway Project

```bash
# Log in
railway login

# Create a new project (from the backend/ directory)
cd backend
railway init
# Choose "Empty Project", name it "mzansi-legal"
```

---

## Step 3: Provision Railway PostgreSQL

In the Railway dashboard:
1. Open your project → **New** → **Database** → **Add PostgreSQL**
2. Railway provisions the database and sets `DATABASE_URL` automatically in your project's shared variables.
3. Copy the `DATABASE_URL` value — it will be in the format:
   `postgresql://postgres:<password>@<host>:<port>/<db>`
   Convert to Npgsql format:
   `Host=<host>;Port=<port>;Database=<db>;Username=postgres;Password=<password>;SSL Mode=Require;Trust Server Certificate=true`

---

## Step 4: Configure Environment Variables in Railway

In Railway dashboard → your service → **Variables** tab, add:

| Variable | Value |
|----------|-------|
| `ConnectionStrings__Default` | Npgsql connection string from Step 3 |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:${{PORT}}` |
| `App__ServerRootAddress` | `https://<your-service>.railway.app/` |
| `App__ClientRootAddress` | `https://<your-frontend-url>/` |
| `App__CorsOrigins` | `https://<your-frontend-url>` |
| `Authentication__JwtBearer__SecurityKey` | A random 32+ character secret |
| `Authentication__JwtBearer__Issuer` | `backend` |
| `Authentication__JwtBearer__Audience` | `backend` |
| `OpenAI__ApiKey` | Your OpenAI API key |
| `OpenAI__EmbeddingModel` | `text-embedding-ada-002` |
| `OpenAI__EnrichmentModel` | `gpt-4o-mini` |
| `OpenAI__BaseUrl` | `https://api.openai.com/` |

---

## Step 5: Deploy

```bash
# From backend/ directory
railway up
```

Railway detects `railway.toml` and `Dockerfile`, builds the image, and deploys. Watch the build logs in the dashboard.

After deploy, Railway runs the health check at `/api/health`. If it returns 200, the deployment is marked **Active**.

---

## Step 6: Verify Deployment

```bash
# Replace with your Railway public URL
curl https://<your-service>.railway.app/api/health
# Expected: {"status":"healthy"}

curl https://<your-service>.railway.app/swagger
# Expected: Swagger UI redirect
```

---

## Step 7: Set Up Automatic Deployments (CI/CD)

In Railway dashboard → your service → **Settings** → **Source**:
1. Connect your GitHub repository
2. Set **Root Directory** to `backend/`
3. Set **Branch** to `main`
4. Railway will redeploy automatically on every push to `main`

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Build fails: `net9.0 not found` | Old .NET 8 base image in Dockerfile | Ensure Dockerfile uses `sdk:9.0` and `aspnet:9.0` |
| Health check timeout | Kestrel not listening on `$PORT` | Verify `ASPNETCORE_URLS=http://+:${{PORT}}` is set |
| DB connection error | Wrong connection string format | Use Npgsql format, not `postgresql://` URL format |
| Migration fails on startup | DB not reachable during startup | Check `ConnectionStrings__Default` env var and DB plugin status |
| CORS errors from frontend | `App__CorsOrigins` missing frontend URL | Add frontend Railway URL to `App__CorsOrigins` |
| JWT errors | `SecurityKey` too short | Key must be at least 32 characters |
