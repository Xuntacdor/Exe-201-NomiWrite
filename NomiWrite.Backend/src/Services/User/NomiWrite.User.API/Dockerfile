FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/User/NomiWrite.User.Domain/NomiWrite.User.Domain.csproj src/Services/User/NomiWrite.User.Domain/
COPY src/Services/User/NomiWrite.User.Application/NomiWrite.User.Application.csproj src/Services/User/NomiWrite.User.Application/
COPY src/Services/User/NomiWrite.User.Infrastructure/NomiWrite.User.Infrastructure.csproj src/Services/User/NomiWrite.User.Infrastructure/
COPY src/Services/User/NomiWrite.User.API/NomiWrite.User.API.csproj src/Services/User/NomiWrite.User.API/

RUN dotnet restore src/Services/User/NomiWrite.User.API/NomiWrite.User.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/User/ src/Services/User/

RUN dotnet publish src/Services/User/NomiWrite.User.API/NomiWrite.User.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NomiWrite.User.API.dll"]
