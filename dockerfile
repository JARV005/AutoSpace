# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos del proyecto y restaurar dependencias
COPY ["AutoSpace.csproj", "."]
RUN dotnet restore "AutoSpace.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src"
RUN dotnet build "AutoSpace.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "AutoSpace.csproj" -c Release -o /app/publish

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y curl

COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "AutoSpace.dll"]