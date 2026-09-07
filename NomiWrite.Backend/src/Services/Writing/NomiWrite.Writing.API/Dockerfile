# ─────────────────────────────────────────────
# NomiWrite Writing Service — Multi-stage Dockerfile
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj", "src/Shared/NomiWrite.Shared.Contracts/"]
COPY ["src/Services/Writing/NomiWrite.Writing.Domain/NomiWrite.Writing.Domain.csproj", "src/Services/Writing/NomiWrite.Writing.Domain/"]
COPY ["src/Services/Writing/NomiWrite.Writing.Application/NomiWrite.Writing.Application.csproj", "src/Services/Writing/NomiWrite.Writing.Application/"]
COPY ["src/Services/Writing/NomiWrite.Writing.Infrastructure/NomiWrite.Writing.Infrastructure.csproj", "src/Services/Writing/NomiWrite.Writing.Infrastructure/"]
COPY ["src/Services/Writing/NomiWrite.Writing.API/NomiWrite.Writing.API.csproj", "src/Services/Writing/NomiWrite.Writing.API/"]
RUN dotnet restore "src/Services/Writing/NomiWrite.Writing.API/NomiWrite.Writing.API.csproj"

# Copy source and build
COPY . .
WORKDIR "/src/src/Services/Writing/NomiWrite.Writing.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NomiWrite.Writing.API.dll"]