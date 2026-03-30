# ─────────────────────────────────────────────────────────────────────────────
# Stage 1: build
# Build context must be the backend/ directory so all src/ projects are
# accessible. Run: docker build -t mzansi-legal-backend .  (from backend/)
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /src

# Copy only .csproj files first to leverage Docker layer caching for restore.
COPY ["src/backend.Web.Host/backend.Web.Host.csproj",         "src/backend.Web.Host/"]
COPY ["src/backend.Web.Core/backend.Web.Core.csproj",         "src/backend.Web.Core/"]
COPY ["src/backend.Application/backend.Application.csproj",   "src/backend.Application/"]
COPY ["src/backend.Core/backend.Core.csproj",                 "src/backend.Core/"]
COPY ["src/backend.EntityFrameworkCore/backend.EntityFrameworkCore.csproj", "src/backend.EntityFrameworkCore/"]

# Restore dependencies for the entry-point project (transitive deps resolved automatically).
WORKDIR /src/src/backend.Web.Host
RUN dotnet restore

# Copy all source code and publish a Release build.
WORKDIR /src
COPY ["src/backend.Web.Host/",         "src/backend.Web.Host/"]
COPY ["src/backend.Web.Core/",         "src/backend.Web.Core/"]
COPY ["src/backend.Application/",      "src/backend.Application/"]
COPY ["src/backend.Core/",             "src/backend.Core/"]
COPY ["src/backend.EntityFrameworkCore/", "src/backend.EntityFrameworkCore/"]

WORKDIR /src/src/backend.Web.Host
RUN dotnet publish -c Release -o /publish --no-restore

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2: runtime
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

WORKDIR /app

COPY --from=build /publish .

# Railway injects a dynamic PORT environment variable.
# ASPNETCORE_URLS overrides any Kestrel endpoint config in appsettings.json.
ENV PORT=80
ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "backend.Web.Host.dll"]
