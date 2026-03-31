FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia só o csproj (cache de restore)
COPY ["Medicines/Medicines.csproj", "Medicines/"]
RUN dotnet restore "Medicines/Medicines.csproj"

# Copia o resto
COPY Medicines/ Medicines/
WORKDIR /src/Medicines

RUN dotnet build "Medicines.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Medicines.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Medicines.dll"]