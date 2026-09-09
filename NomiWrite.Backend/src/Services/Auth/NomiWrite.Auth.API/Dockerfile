FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Auth/NomiWrite.Auth.Domain/NomiWrite.Auth.Domain.csproj src/Services/Auth/NomiWrite.Auth.Domain/
COPY src/Services/Auth/NomiWrite.Auth.Application/NomiWrite.Auth.Application.csproj src/Services/Auth/NomiWrite.Auth.Application/
COPY src/Services/Auth/NomiWrite.Auth.Infrastructure/NomiWrite.Auth.Infrastructure.csproj src/Services/Auth/NomiWrite.Auth.Infrastructure/
COPY src/Services/Auth/NomiWrite.Auth.API/NomiWrite.Auth.API.csproj src/Services/Auth/NomiWrite.Auth.API/

RUN dotnet restore src/Services/Auth/NomiWrite.Auth.API/NomiWrite.Auth.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Auth/ src/Services/Auth/

RUN dotnet publish src/Services/Auth/NomiWrite.Auth.API/NomiWrite.Auth.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NomiWrite.Auth.API.dll"]
