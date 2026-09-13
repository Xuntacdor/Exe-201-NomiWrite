FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Admin/NomiWrite.Admin.Domain/NomiWrite.Admin.Domain.csproj src/Services/Admin/NomiWrite.Admin.Domain/
COPY src/Services/Admin/NomiWrite.Admin.Application/NomiWrite.Admin.Application.csproj src/Services/Admin/NomiWrite.Admin.Application/
COPY src/Services/Admin/NomiWrite.Admin.Infrastructure/NomiWrite.Admin.Infrastructure.csproj src/Services/Admin/NomiWrite.Admin.Infrastructure/
COPY src/Services/Admin/NomiWrite.Admin.API/NomiWrite.Admin.API.csproj src/Services/Admin/NomiWrite.Admin.API/

RUN dotnet restore src/Services/Admin/NomiWrite.Admin.API/NomiWrite.Admin.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Admin/ src/Services/Admin/

RUN dotnet publish src/Services/Admin/NomiWrite.Admin.API/NomiWrite.Admin.API.csproj \
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
ENTRYPOINT ["dotnet", "NomiWrite.Admin.API.dll"]