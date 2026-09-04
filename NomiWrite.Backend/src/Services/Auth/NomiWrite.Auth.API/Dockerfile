# ─────────────────────────────────────────────
# NomiWrite Auth Service — Multi-stage Dockerfile
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj", "src/Shared/NomiWrite.Shared.Contracts/"]
COPY ["src/Services/Auth/NomiWrite.Auth.Domain/NomiWrite.Auth.Domain.csproj", "src/Services/Auth/NomiWrite.Auth.Domain/"]
COPY ["src/Services/Auth/NomiWrite.Auth.Application/NomiWrite.Auth.Application.csproj", "src/Services/Auth/NomiWrite.Auth.Application/"]
COPY ["src/Services/Auth/NomiWrite.Auth.Infrastructure/NomiWrite.Auth.Infrastructure.csproj", "src/Services/Auth/NomiWrite.Auth.Infrastructure/"]
COPY ["src/Services/Auth/NomiWrite.Auth.API/NomiWrite.Auth.API.csproj", "src/Services/Auth/NomiWrite.Auth.API/"]
RUN dotnet restore "src/Services/Auth/NomiWrite.Auth.API/NomiWrite.Auth.API.csproj"

# Copy source and build
COPY . .
WORKDIR "/src/src/Services/Auth/NomiWrite.Auth.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NomiWrite.Auth.API.dll"]
