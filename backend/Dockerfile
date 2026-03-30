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
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

COPY --from=build /publish .

# Railway injects PORT at runtime. ASPNETCORE_HTTP_PORTS is the .NET 8+ way
# to tell Kestrel which port to listen on without a full URL. The default of
# 8080 is used when running the image locally without Railway's PORT variable.
ENV ASPNETCORE_HTTP_PORTS=8080

# Use a shell entrypoint so $PORT is resolved at container start time,
# allowing Railway's dynamic PORT value to override the default above.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet backend.Web.Host.dll"]
