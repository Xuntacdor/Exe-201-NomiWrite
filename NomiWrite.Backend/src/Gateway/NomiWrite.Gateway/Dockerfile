FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Gateway/NomiWrite.Gateway/NomiWrite.Gateway.csproj src/Gateway/NomiWrite.Gateway/

RUN dotnet restore src/Gateway/NomiWrite.Gateway/NomiWrite.Gateway.csproj

COPY src/Gateway/ src/Gateway/

RUN dotnet publish src/Gateway/NomiWrite.Gateway/NomiWrite.Gateway.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NomiWrite.Gateway.dll"]
