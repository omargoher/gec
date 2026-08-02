FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GEC.API/GEC.API.csproj", "GEC.API/"]
COPY ["GEC.Domain/GEC.Domain.csproj", "GEC.Domain/"]
COPY ["GEC.ApplicationCore/GEC.ApplicationCore.csproj", "GEC.ApplicationCore/"]
COPY ["GEC.Infrastructure/GEC.Infrastructure.csproj", "GEC.Infrastructure/"]

RUN dotnet restore "GEC.API/GEC.API.csproj"

COPY . .
WORKDIR "/src/GEC.API"
RUN dotnet build "./GEC.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GEC.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GEC.API.dll"]
