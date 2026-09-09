FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/AICoordinator/NomiWrite.AICoordinator.Domain/NomiWrite.AICoordinator.Domain.csproj src/Services/AICoordinator/NomiWrite.AICoordinator.Domain/
COPY src/Services/AICoordinator/NomiWrite.AICoordinator.Application/NomiWrite.AICoordinator.Application.csproj src/Services/AICoordinator/NomiWrite.AICoordinator.Application/
COPY src/Services/AICoordinator/NomiWrite.AICoordinator.Infrastructure/NomiWrite.AICoordinator.Infrastructure.csproj src/Services/AICoordinator/NomiWrite.AICoordinator.Infrastructure/
COPY src/Services/AICoordinator/NomiWrite.AICoordinator.API/NomiWrite.AICoordinator.API.csproj src/Services/AICoordinator/NomiWrite.AICoordinator.API/

RUN dotnet restore src/Services/AICoordinator/NomiWrite.AICoordinator.API/NomiWrite.AICoordinator.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/AICoordinator/ src/Services/AICoordinator/

RUN dotnet publish src/Services/AICoordinator/NomiWrite.AICoordinator.API/NomiWrite.AICoordinator.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NomiWrite.AICoordinator.API.dll"]
