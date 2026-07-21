# Multi-stage build para EduOnline.Alunos.ApiRest
# Stage 1: Restore - Restaura dependências NuGet
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

# Copiar apenas os arquivos de projeto primeiro (cache layer)
COPY ["src/EduOnline.Core/EduOnline.Core.csproj", "src/EduOnline.Core/"]
COPY ["src/EduOnline.Core.Api/EduOnline.Core.Api.csproj", "src/EduOnline.Core.Api/"]
COPY ["src/EduOnline.Alunos.Domain/EduOnline.Alunos.Domain.csproj", "src/EduOnline.Alunos.Domain/"]
COPY ["src/EduOnline.Alunos.Data/EduOnline.Alunos.Data.csproj", "src/EduOnline.Alunos.Data/"]
COPY ["src/EduOnline.Alunos.Application/EduOnline.Alunos.Application.csproj", "src/EduOnline.Alunos.Application/"]
COPY ["src/EduOnline.Alunos.ApiRest/EduOnline.Alunos.ApiRest.csproj", "src/EduOnline.Alunos.ApiRest/"]
COPY ["src/EventSourcing/EventSourcing.csproj", "src/EventSourcing/"]

# Restaurar dependências
RUN dotnet restore "src/EduOnline.Alunos.ApiRest/EduOnline.Alunos.ApiRest.csproj"


FROM restore AS build
WORKDIR /src

# Copiar código fonte completo
COPY . .

# Stage 2: Publish - Compila e publica a aplicação
# Publish da aplicação (já compila internamente, dispensando um "dotnet build" separado)
RUN dotnet publish "src/EduOnline.Alunos.ApiRest/EduOnline.Alunos.ApiRest.csproj" \
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
EXPOSE 5002

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
	CMD wget -O- http://localhost:5002/health || exit 1

# Executar aplicação
ENTRYPOINT ["dotnet", "EduOnline.Alunos.ApiRest.dll"]
