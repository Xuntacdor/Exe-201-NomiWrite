FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Logging/NomiWrite.Logging.Domain/NomiWrite.Logging.Domain.csproj src/Services/Logging/NomiWrite.Logging.Domain/
COPY src/Services/Logging/NomiWrite.Logging.Application/NomiWrite.Logging.Application.csproj src/Services/Logging/NomiWrite.Logging.Application/
COPY src/Services/Logging/NomiWrite.Logging.Infrastructure/NomiWrite.Logging.Infrastructure.csproj src/Services/Logging/NomiWrite.Logging.Infrastructure/
COPY src/Services/Logging/NomiWrite.Logging.API/NomiWrite.Logging.API.csproj src/Services/Logging/NomiWrite.Logging.API/

RUN dotnet restore src/Services/Logging/NomiWrite.Logging.API/NomiWrite.Logging.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Logging/ src/Services/Logging/

RUN dotnet publish src/Services/Logging/NomiWrite.Logging.API/NomiWrite.Logging.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
# curl is required by the Compose /health healthchecks (the base image ships
# neither curl nor wget).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NomiWrite.Logging.API.dll"]