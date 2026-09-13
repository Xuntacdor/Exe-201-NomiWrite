FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Payment/NomiWrite.Payment.Domain/NomiWrite.Payment.Domain.csproj src/Services/Payment/NomiWrite.Payment.Domain/
COPY src/Services/Payment/NomiWrite.Payment.Application/NomiWrite.Payment.Application.csproj src/Services/Payment/NomiWrite.Payment.Application/
COPY src/Services/Payment/NomiWrite.Payment.Infrastructure/NomiWrite.Payment.Infrastructure.csproj src/Services/Payment/NomiWrite.Payment.Infrastructure/
COPY src/Services/Payment/NomiWrite.Payment.API/NomiWrite.Payment.API.csproj src/Services/Payment/NomiWrite.Payment.API/

RUN dotnet restore src/Services/Payment/NomiWrite.Payment.API/NomiWrite.Payment.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Payment/ src/Services/Payment/

RUN dotnet publish src/Services/Payment/NomiWrite.Payment.API/NomiWrite.Payment.API.csproj \
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
ENTRYPOINT ["dotnet", "NomiWrite.Payment.API.dll"]
