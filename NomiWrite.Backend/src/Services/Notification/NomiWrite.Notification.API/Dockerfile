FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5170

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/Notification/NomiWrite.Notification.API/NomiWrite.Notification.API.csproj", "Services/Notification/NomiWrite.Notification.API/"]
COPY ["src/Services/Notification/NomiWrite.Notification.Application/NomiWrite.Notification.Application.csproj", "Services/Notification/NomiWrite.Notification.Application/"]
COPY ["src/Services/Notification/NomiWrite.Notification.Infrastructure/NomiWrite.Notification.Infrastructure.csproj", "Services/Notification/NomiWrite.Notification.Infrastructure/"]
COPY ["src/Services/Notification/NomiWrite.Notification.Domain/NomiWrite.Notification.Domain.csproj", "Services/Notification/NomiWrite.Notification.Domain/"]
COPY ["src/Shared/NomiWrite.Shared.Contracts/NomiWrite.Shared.Contracts.csproj", "Shared/NomiWrite.Shared.Contracts/"]
RUN dotnet restore "Services/Notification/NomiWrite.Notification.API/NomiWrite.Notification.API.csproj"

COPY src/ .
WORKDIR "/src/Services/Notification/NomiWrite.Notification.API"
RUN dotnet build "NomiWrite.Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NomiWrite.Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NomiWrite.Notification.API.dll"]
