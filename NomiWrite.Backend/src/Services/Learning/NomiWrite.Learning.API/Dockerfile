FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj src/Shared/NomiWrite.Shared.Contracts/
COPY src/Services/Learning/NomiWrite.Learning.Domain/NomiWrite.Learning.Domain.csproj src/Services/Learning/NomiWrite.Learning.Domain/
COPY src/Services/Learning/NomiWrite.Learning.Application/NomiWrite.Learning.Application.csproj src/Services/Learning/NomiWrite.Learning.Application/
COPY src/Services/Learning/NomiWrite.Learning.Infrastructure/NomiWrite.Learning.Infrastructure.csproj src/Services/Learning/NomiWrite.Learning.Infrastructure/
COPY src/Services/Learning/NomiWrite.Learning.API/NomiWrite.Learning.API.csproj src/Services/Learning/NomiWrite.Learning.API/

RUN dotnet restore src/Services/Learning/NomiWrite.Learning.API/NomiWrite.Learning.API.csproj

COPY src/Shared/ src/Shared/
COPY src/Services/Learning/ src/Services/Learning/

RUN dotnet publish src/Services/Learning/NomiWrite.Learning.API/NomiWrite.Learning.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NomiWrite.Learning.API.dll"]