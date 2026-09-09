FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Subscription/NomiWrite.Subscription.Domain/NomiWrite.Subscription.Domain.csproj src/Services/Subscription/NomiWrite.Subscription.Domain/
COPY src/Services/Subscription/NomiWrite.Subscription.Application/NomiWrite.Subscription.Application.csproj src/Services/Subscription/NomiWrite.Subscription.Application/
COPY src/Services/Subscription/NomiWrite.Subscription.Infrastructure/NomiWrite.Subscription.Infrastructure.csproj src/Services/Subscription/NomiWrite.Subscription.Infrastructure/
COPY src/Services/Subscription/NomiWrite.Subscription.API/NomiWrite.Subscription.API.csproj src/Services/Subscription/NomiWrite.Subscription.API/

RUN dotnet restore src/Services/Subscription/NomiWrite.Subscription.API/NomiWrite.Subscription.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Subscription/ src/Services/Subscription/

RUN dotnet publish src/Services/Subscription/NomiWrite.Subscription.API/NomiWrite.Subscription.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NomiWrite.Subscription.API.dll"]
