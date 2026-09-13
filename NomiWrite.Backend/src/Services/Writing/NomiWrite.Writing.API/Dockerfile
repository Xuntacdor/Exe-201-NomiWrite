FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Writing/NomiWrite.Writing.Domain/NomiWrite.Writing.Domain.csproj src/Services/Writing/NomiWrite.Writing.Domain/
COPY src/Services/Writing/NomiWrite.Writing.Application/NomiWrite.Writing.Application.csproj src/Services/Writing/NomiWrite.Writing.Application/
COPY src/Services/Writing/NomiWrite.Writing.Infrastructure/NomiWrite.Writing.Infrastructure.csproj src/Services/Writing/NomiWrite.Writing.Infrastructure/
COPY src/Services/Writing/NomiWrite.Writing.API/NomiWrite.Writing.API.csproj src/Services/Writing/NomiWrite.Writing.API/

RUN dotnet restore src/Services/Writing/NomiWrite.Writing.API/NomiWrite.Writing.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Writing/ src/Services/Writing/

RUN dotnet publish src/Services/Writing/NomiWrite.Writing.API/NomiWrite.Writing.API.csproj \
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
ENTRYPOINT ["dotnet", "NomiWrite.Writing.API.dll"]
