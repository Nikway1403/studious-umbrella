FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Presentation/AuthService.GrpcApi/AuthService.GrpcApi.csproj", "src/Presentation/AuthService.GrpcApi/"]
COPY ["src/Infrastructure/Persistence/Persistence.csproj", "src/Infrastructure/Persistence/"]
COPY ["src/Application/Models/Models.csproj", "src/Application/Models/"]
COPY ["src/Application/Contracts/Contracts.csproj", "src/Application/Contracts/"]
COPY ["src/Application/Abstractions/Abstractions.csproj", "src/Application/Abstractions/"]
COPY ["src/Application/Application/Application.csproj", "src/Application/Application/"]

RUN dotnet restore "src/Presentation/AuthService.GrpcApi/AuthService.GrpcApi.csproj"

COPY . .
WORKDIR "/src/src/Presentation/AuthService.GrpcApi"
RUN dotnet publish "./AuthService.GrpcApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthService.GrpcApi.dll"]