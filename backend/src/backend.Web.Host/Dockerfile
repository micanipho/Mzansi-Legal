FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/backend.Web.Host/backend.Web.Host.csproj", "src/backend.Web.Host/"]
COPY ["src/backend.Web.Core/backend.Web.Core.csproj", "src/backend.Web.Core/"]
COPY ["src/backend.Application/backend.Application.csproj", "src/backend.Application/"]
COPY ["src/backend.Core/backend.Core.csproj", "src/backend.Core/"]
COPY ["src/backend.EntityFrameworkCore/backend.EntityFrameworkCore.csproj", "src/backend.EntityFrameworkCore/"]
WORKDIR "/src/src/backend.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/backend.Web.Host", "src/backend.Web.Host"]
COPY ["src/backend.Web.Core", "src/backend.Web.Core"]
COPY ["src/backend.Application", "src/backend.Application"]
COPY ["src/backend.Core", "src/backend.Core"]
COPY ["src/backend.EntityFrameworkCore", "src/backend.EntityFrameworkCore"]
WORKDIR "/src/src/backend.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "backend.Web.Host.dll"]


