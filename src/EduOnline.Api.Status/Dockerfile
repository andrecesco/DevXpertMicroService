# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Stage 1: Restore - Restaura dependências NuGet
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

# Copiar apenas os arquivos de projeto primeiro (cache layer)
COPY ["src/EduOnline.Api.Status/EduOnline.Api.Status.csproj", "src/EduOnline.Api.Status/"]

# Restaurar dependências
RUN dotnet restore "src/EduOnline.Api.Status/EduOnline.Api.Status.csproj"

# Stage 2: Publish - Compila e publica a aplicação
FROM restore AS build
WORKDIR /src

# Copiar código fonte completo
COPY . .

# Publish da aplicação (já compila internamente, dispensando um "dotnet build" separado)
RUN dotnet publish "src/EduOnline.Api.Status/EduOnline.Api.Status.csproj" -c Release -o /app/publish

# Stage 3: Runtime - Imagem final leve
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
RUN apk add --no-cache icu-libs

# Criar usuário não-root para executar a aplicação
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Copiar arquivos publicados
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

# Expor porta
EXPOSE 5005

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
	CMD wget -O- http://localhost:5005/health || exit 1

ENTRYPOINT ["dotnet", "EduOnline.Api.Status.dll"]