FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Notification/NomiWrite.Notification.API/NomiWrite.Notification.API.csproj src/Services/Notification/NomiWrite.Notification.API/
COPY src/Services/Notification/NomiWrite.Notification.Application/NomiWrite.Notification.Application.csproj src/Services/Notification/NomiWrite.Notification.Application/
COPY src/Services/Notification/NomiWrite.Notification.Infrastructure/NomiWrite.Notification.Infrastructure.csproj src/Services/Notification/NomiWrite.Notification.Infrastructure/
COPY src/Services/Notification/NomiWrite.Notification.Domain/NomiWrite.Notification.Domain.csproj src/Services/Notification/NomiWrite.Notification.Domain/

RUN dotnet restore src/Services/Notification/NomiWrite.Notification.API/NomiWrite.Notification.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Notification/ src/Services/Notification/

RUN dotnet publish src/Services/Notification/NomiWrite.Notification.API/NomiWrite.Notification.API.csproj \
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
ENTRYPOINT ["dotnet", "NomiWrite.Notification.API.dll"]