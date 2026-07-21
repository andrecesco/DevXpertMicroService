# Multi-stage build para EduOnline.Pagamentos.ApiRest
# Stage 1: Restore - Restaura dependências NuGet
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

# Copiar apenas os arquivos de projeto primeiro (cache layer)
COPY ["src/EduOnline.Core/EduOnline.Core.csproj", "src/EduOnline.Core/"]
COPY ["src/EduOnline.Core.Api/EduOnline.Core.Api.csproj", "src/EduOnline.Core.Api/"]
COPY ["src/EduOnline.Pagamentos.Domain/EduOnline.Pagamentos.Domain.csproj", "src/EduOnline.Pagamentos.Domain/"]
COPY ["src/EduOnline.Pagamentos.Data/EduOnline.Pagamentos.Data.csproj", "src/EduOnline.Pagamentos.Data/"]
COPY ["src/EduOnline.Pagamentos.AntiCorruption/EduOnline.Pagamentos.AntiCorruption.csproj", "src/EduOnline.Pagamentos.AntiCorruption/"]
COPY ["src/EduOnline.Pagamentos.ApiRest/EduOnline.Pagamentos.ApiRest.csproj", "src/EduOnline.Pagamentos.ApiRest/"]
COPY ["src/EventSourcing/EventSourcing.csproj", "src/EventSourcing/"]

# Restaurar dependências
RUN dotnet restore "src/EduOnline.Pagamentos.ApiRest/EduOnline.Pagamentos.ApiRest.csproj"

# Stage 2: Publish - Compila e publica a aplicação
FROM restore AS build
WORKDIR /src

# Copiar código fonte completo
COPY . .

# Publish da aplicação (já compila internamente, dispensando um "dotnet build" separado)
RUN dotnet publish "src/EduOnline.Pagamentos.ApiRest/EduOnline.Pagamentos.ApiRest.csproj" \
	-c Release \
	-o /app/publish

# Stage 3: Runtime - Imagem final leve
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Expor porta
EXPOSE 5003

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
	CMD wget -O- http://localhost:5003/health || exit 1

# Executar aplicação
ENTRYPOINT ["dotnet", "EduOnline.Pagamentos.ApiRest.dll"]
