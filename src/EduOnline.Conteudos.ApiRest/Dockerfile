# Multi-stage build para EduOnline.Conteudos.ApiRest
# Stage 1: Restore - Restaura dependências NuGet
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

# Copiar apenas os arquivos de projeto primeiro (cache layer)
COPY ["src/EduOnline.Core/EduOnline.Core.csproj", "src/EduOnline.Core/"]
COPY ["src/EduOnline.Core.Api/EduOnline.Core.Api.csproj", "src/EduOnline.Core.Api/"]
COPY ["src/EduOnline.Conteudos.Domain/EduOnline.Conteudos.Domain.csproj", "src/EduOnline.Conteudos.Domain/"]
COPY ["src/EduOnline.Conteudos.Data/EduOnline.Conteudos.Data.csproj", "src/EduOnline.Conteudos.Data/"]
COPY ["src/EduOnline.Conteudos.ApiRest/EduOnline.Conteudos.ApiRest.csproj", "src/EduOnline.Conteudos.ApiRest/"]
COPY ["src/EventSourcing/EventSourcing.csproj", "src/EventSourcing/"]

# Restaurar dependências
RUN dotnet restore "src/EduOnline.Conteudos.ApiRest/EduOnline.Conteudos.ApiRest.csproj"

# Stage 2: Publish - Compila e publica a aplicação
FROM restore AS build
WORKDIR /src

# Copiar código fonte completo
COPY . .

# Publish da aplicação (já compila internamente, dispensando um "dotnet build" separado)
RUN dotnet publish "src/EduOnline.Conteudos.ApiRest/EduOnline.Conteudos.ApiRest.csproj" \
	-c Release \
	-o /app/publish

# Stage 3: Runtime - Imagem final leve
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Criar usuário não-root para executar a aplicação
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Copiar arquivos publicados
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

# Expor porta
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
	CMD wget -O- http://localhost:5001/health || exit 1

# Executar aplicação
ENTRYPOINT ["dotnet", "EduOnline.Conteudos.ApiRest.dll"]
