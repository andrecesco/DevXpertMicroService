# Multi-stage build para EduOnline.Bff.ApiRest (BFF)
# Stage 1: Restore - Restaura dependências NuGet
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

# Copiar apenas os arquivos de projeto primeiro (cache layer)
COPY ["src/EduOnline.Core/EduOnline.Core.csproj", "src/EduOnline.Core/"]
COPY ["src/EduOnline.Core.Api/EduOnline.Core.Api.csproj", "src/EduOnline.Core.Api/"]
COPY ["src/EduOnline.WebApps.ApiRest/EduOnline.Bff.ApiRest.csproj", "src/EduOnline.WebApps.ApiRest/"]
COPY ["src/EventSourcing/EventSourcing.csproj", "src/EventSourcing/"]

# Restaurar dependências
RUN dotnet restore "src/EduOnline.WebApps.ApiRest/EduOnline.Bff.ApiRest.csproj"

# Stage 2: Publish - Compila e publica a aplicação
FROM restore AS build
WORKDIR /src

# Copiar código fonte completo
COPY . .

# Publish da aplicação (já compila internamente, dispensando um "dotnet build" separado)
RUN dotnet publish "src/EduOnline.WebApps.ApiRest/EduOnline.Bff.ApiRest.csproj" \
	-c Release \
	-o /app/publish

# Stage 3: Runtime - Imagem final leve
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
RUN apk add --no-cache icu-libs

# Variáveis de ambiente padrão
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
	ASPNETCORE_ENVIRONMENT=Production \
	ASPNETCORE_URLS=http://+:5004 \
	ASPNETCORE_HTTP_PORTS=5004 \
	OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Expor porta
EXPOSE 5004

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
	CMD wget -O- http://localhost:5004/health || exit 1

# Executar aplicação
ENTRYPOINT ["dotnet", "EduOnline.Bff.ApiRest.dll"]
