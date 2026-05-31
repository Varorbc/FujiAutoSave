FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
RUN apk update \
    && apk add build-base zlib-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["NuGet.config", "."]
COPY ["src/FujiAutoSave/FujiAutoSave.csproj", "src/FujiAutoSave/"]
COPY ["src/FujiAutoSave.Core/FujiAutoSave.Core.csproj", "src/FujiAutoSave.Core/"]
RUN dotnet restore "./src/FujiAutoSave/FujiAutoSave.csproj"
COPY src .
WORKDIR "/src/FujiAutoSave"
RUN dotnet build "./FujiAutoSave.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FujiAutoSave.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./FujiAutoSave"]