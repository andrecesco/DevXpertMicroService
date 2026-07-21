# 🚀 EduOnline Platform - Docker & Kubernetes Setup

<div align="center">

**Plataforma Educacional Distribuída com Pipeline CI/CD, Docker e Kubernetes**

[![Docker](https://img.shields.io/badge/Docker-20.10+-blue?logo=docker)](https://www.docker.com/)
[![Docker Compose](https://img.shields.io/badge/Docker%20Compose-2.0+-blue?logo=docker)](https://docs.docker.com/compose/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)

</div>

---

## 📋 Índice

1. [Status de Implementação](#-status-de-implementação)
2. [Visão Geral](#visão-geral)
3. [Arquitetura](#arquitetura)
4. [Pré-requisitos](#pré-requisitos)
5. [Quick Start](#quick-start)
6. [Configuração Detalhada](#configuração-detalhada)
7. [Comandos Úteis](#comandos-úteis)
8. [Troubleshooting](#troubleshooting)
9. [CI/CD Pipeline](#-cicd-pipeline)
10. [Próximos Passos - Kubernetes](#próximos-passos---kubernetes)

---

## 🚀 Status de Implementação

### ✅ Estado Atual

- [x] **Ambiente Docker Compose** concluído
  - ✅ 5 APIs + SQL Server + EventStoreDB + OpenTelemetry/Prometheus/Jaeger
  - ✅ Health checks e Swagger disponíveis nas APIs
  - ✅ Volumes persistentes configurados
  - ✅ Script `docker-init.ps1` para subir, derrubar e inspecionar o ambiente

- [x] **Pipeline CI/CD** concluído
  - ✅ 4 Workflows (CI, CD, Security, PR)
  - ✅ Build matrix para 5 APIs
  - ✅ Push automático para Docker Hub
  - ✅ Scanning de vulnerabilidades
  - ✅ Documentação de CI/CD disponível em `docs/`

### 📚 Documentação útil

- `README.md` - visão geral e instruções de execução
- `docs/CI-CD-PIPELINE.md` - detalhes do pipeline
- `docs/GITHUB-ACTIONS-SETUP.md` - configuração de secrets
- `CI-CD-TESTING-GUIDE.md` - validação dos workflows
- `SECURITY-COMPLIANCE-MATRIX.md` - estado atual de segurança e compliance
- `infra/kubernetes/README.md` - base dos manifests e scripts de Kubernetes

**Stack local atual:** 5 APIs + BFF + SQL Server + EventStoreDB + observabilidade.

---

## 🎯 Visão Geral

A **EduOnline Platform** é um ecossistema de microsserviços desenvolvido em C# com .NET 10, containerizado com Docker e orquestrado localmente via Docker Compose.

### Componentes

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| **Auth API** | 5000 | Autenticação e autorização com JWT |
| **Conteúdos API** | 5001 | Gestão de cursos e aulas |
| **Alunos API** | 5002 | Perfil de alunos e matrículas |
| **Pagamentos API** | 5003 | Processamento de pagamentos e faturamento |
| **BFF API** | 5004 | Backend for Frontend - orquestração |
| **EventStoreDB** | 2113 | Event sourcing |
| **SQL Server** | 1433 | Banco de dados relacional |


---

## 🏗️ Arquitetura

### Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────┐
│                      Docker Network                          │
│                  (eduonline-network)                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │  Auth API    │  │Conteudos API │  │ Alunos API   │       │
│  │   :5000      │  │   :5001      │  │   :5002      │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
│         │                 │                 │                │
│         └─────────────────┼─────────────────┘                │
│                           │                                  │
│  ┌──────────────┐  ┌──────────────┐                          │
│  │Pagamentos API│  │  BFF API     │                          │
│  │   :5003      │  │   :5004      │                          │
│  └──────────────┘  └──────────────┘                          │
│         │                 │                                  │
│         └─────────────────┼─────────────────┐                │
│                           │                 │                │
│                      ┌────▼─────────────┐   │                │
│                      │  SQL Server 2022 │   │                │
│                      │    :1433         │   │                │
│                      │  (persistent)    │   │                │
│                      └──────────────────┘   │                │
│                                             │                │
│  Volumes de Persistência:                   │                │
│  • sqlserver_data  (MDF)                    │                │
│  • sqlserver_log   (LDF)                    │                │
│  • sqlserver_backup (BAK)                   │                │
│                                             │                │
└─────────────────────────────────────────────────────────────┘
```

### Fluxo de Dados

1. **Frontend** → BFF API (porta 5004)
2. **BFF** → Auth + Conteúdos + Alunos + Pagamentos APIs
3. **APIs** → SQL Server (schema compartilhado com contextos isolados)

---

## 📦 Pré-requisitos

### Windows / macOS / Linux

- **Docker Desktop** 20.10+ ([Download](https://www.docker.com/products/docker-desktop))
  - Docker Engine
  - Docker Compose v2.0+
- **PowerShell** 7.0+ (Windows) ou Bash (Linux/macOS)
- **Git** para versionamento
- **Visual Studio Code** ou **Visual Studio 2026** (opcional, para desenvolvimento)

### Requisitos de Sistema

| Recurso | Mínimo | Recomendado |
|---------|--------|------------|
| CPU | 2 cores | 4+ cores |
| RAM | 4 GB | 8+ GB |
| Disco | 20 GB | 50+ GB |
| Internet | Sim | Sim (para pull de imagens) |

### Verificar Instalação

```bash
docker --version
# Docker version 24.0.0+

docker compose version
# Docker Compose version v2.20.0+
```

---

## 🚀 Quick Start

### 1️⃣ Clone o Repositório

```bash
git clone https://github.com/seu-usuario/DevXpertMod05.git
cd DevXpertMicroService
```

### 2️⃣ Configurar Variáveis de Ambiente

```bash
# Copiar arquivo .env padrão (já incluído no repositório)
# Ou editar conforme necessário:
cat .env

# Editar valores sensíveis (senhas, secrets)
# Abrir com seu editor favorito
code .env
```

### 3️⃣ Subir o Ecossistema Completo

```bash
# Build das imagens Docker + Start dos containers
docker compose up -d --build

# Verificar status dos containers
docker compose ps

# Expected output:
# NAME                      STATUS         PORTS
# eduonline-sqlserver       Up (healthy)   1433->1433/tcp
# eduonline-eventstore      Up (healthy)   2113->2113/tcp
# eduonline-auth-api        Up (healthy)   5000->5000/tcp
# eduonline-conteudos-api   Up (healthy)   5001->5001/tcp
# eduonline-alunos-api      Up (healthy)   5002->5002/tcp
# eduonline-pagamentos-api  Up (healthy)   5003->5003/tcp
# eduonline-bff-api         Up (healthy)   5004->5004/tcp
```

### 4️⃣ Verificar Saúde dos Serviços

```bash
# Health Check de cada API
curl -s http://localhost:5000/health | jq .
curl -s http://localhost:5001/health | jq .
curl -s http://localhost:5002/health | jq .
curl -s http://localhost:5003/health | jq .
curl -s http://localhost:5004/health | jq .

# Verificar logs em tempo real
docker compose logs -f
```

### 5️⃣ Acessar APIs

```bash
# Swagger UI de cada API
Auth API:      http://localhost:5000/swagger
Conteúdos:     http://localhost:5001/swagger
Alunos:        http://localhost:5002/swagger
Pagamentos:    http://localhost:5003/swagger
BFF:           http://localhost:5004/swagger
```

### 6️⃣ Dados de Teste

**Credenciais padrão (seed automático):**

```
User: admin@eduonline.com
Pass: [Configure na aplicação via register endpoint]
Role: Administrador

User: aluno@eduonline.com
Pass: [Configure na aplicação via register endpoint]
Role: Aluno
Matrícula: ALUNO001
```

---

## ⚙️ Configuração Detalhada

### Variáveis de Ambiente (.env)

```bash
# SQL Server
SA_PASSWORD=P@ssw0rd2024!           # Senha do SA (mude em prod!)

# JWT
JWT_SECRET=SuperSecretKey...        # Chave para assinar JWTs (mude em prod!)

# Environment
ASPNETCORE_ENVIRONMENT=Development  # Development, Staging, Production

# Database
DB_SERVER=sqlserver                 # Host do SQL Server (Docker network)
DB_PORT=1433                        # Porta padrão SQL Server
DB_NAME=EduOnlineDB                 # Nome do database
DB_USER=sa                          # User padrão

# Compose
COMPOSE_PROJECT_NAME=eduonline-dev  # Prefixo dos containers
```

### Estrutura de Volumes

```
volumes:
  sqlserver_data:       # Arquivos de dados (MDF) - ~500MB
  sqlserver_log:        # Arquivos de log (LDF) - ~100MB
  sqlserver_backup:     # Backups e arquivos temporários
```

### Network Compartilhada

- **Nome:** `eduonline-network`
- **Driver:** `bridge`
- **Subnet:** `172.25.0.0/16`
- **Resolução DNS:** Containers podem se chamar pelo nome (ex: `http://auth-api:5000`)

---

## 📝 Comandos Úteis

### Build e Execução

```bash
# Build das imagens (sem cache)
docker compose build --no-cache

# Subir em background
docker compose up -d

# Subir com logs em tempo real
docker compose up

# Parar todos os containers
docker compose down

# Parar e remover volumes (⚠️ perderá dados)
docker compose down -v

# Remover imagens também
docker compose down -v --rmi all
```

### Logs e Debugging

```bash
# Logs de um serviço específico
docker compose logs auth-api
docker compose logs sqlserver -f  # Follow mode

# Logs de todos os serviços
docker compose logs -f

# Últimas 100 linhas
docker compose logs --tail=100

# Ver histórico (sem follow)
docker compose logs auth-api | tail -50
```

### Acessar Containers

```bash
# Shell interativa no SQL Server
docker exec -it eduonline-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P P@ssw0rd2024!

# Shell no container de uma API
docker exec -it eduonline-auth-api sh

# Executar comando único
docker compose exec auth-api dotnet --version
```

### Health Checks

```bash
# Status dos containers
docker compose ps

# Inspecionar healthcheck de um container
docker inspect eduonline-auth-api | jq '.[0].State.Health'

# Forçar healthcheck manualmente
docker exec eduonline-auth-api wget -O- http://localhost:5000/health
```

### Database

```bash
# Conectar ao SQL Server (de dentro do container)
docker exec -it eduonline-sqlserver \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssw0rd2024!

# Query de exemplo
# > SELECT * FROM [Auth].[Users];
# > GO

# Backup do database
docker exec eduonline-sqlserver \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssw0rd2024! \
  -Q "BACKUP DATABASE EduOnlineDB TO DISK='/var/opt/mssql/backup/EduOnlineDB.bak'"
```

### Performance e Recursos

```bash
# Monitorar uso de recursos em tempo real
docker stats

# Ver tamanho das imagens
docker images | grep eduonline

# Limpar espaço em disco
docker compose down -v              # Remove volumes
docker system prune -a              # Remove imagens não utilizadas
docker volume prune                 # Remove volumes órfãs
```

---

## 🔍 Troubleshooting

### ❌ Containers não iniciam

```bash
# Verificar logs
docker compose logs sqlserver

# Problema comum: Porta já em uso
lsof -i :5000
# Solução: Mudar porta em docker-compose.yml ou .env
```

### ❌ Erro de conexão no SQL Server

```bash
# Verificar se SQL Server está saudável
docker compose ps | grep sqlserver

# Se não está healthy, aguardar mais tempo
docker compose logs sqlserver | tail -30

# Problema: Insuficiente RAM
# Solução: Aumentar memória do Docker Desktop
```

### ❌ API não consegue conectar em outra API

```bash
# Verificar rede
docker network ls
docker network inspect eduonline_eduonline-network

# Testar conectividade entre containers
docker exec eduonline-auth-api ping -c 3 conteudos-api
# Output esperado: bytes received do ping

# Se falhar: checar nome do container (deve estar no docker-compose.yml)
```

### ❌ Porta já está em uso

```bash
# Encontrar qual processo usa a porta (Linux/macOS)
lsof -i :5000

# Encontrar qual processo (Windows - PowerShell)
Get-NetTCPConnection -LocalPort 5000

# Solução 1: Matar o processo
kill -9 <PID>

# Solução 2: Usar porta diferente
# Editar docker-compose.yml ou .env
```

### ❌ Rebuild necessário após mudanças no código

```bash
# Rebuild e restart
docker compose down
docker compose up -d --build

# Ou apenas rebuild sem down
docker compose build --no-cache
docker compose restart
```

---

## 🐳 Dockerfile - Práticas Implementadas

### Multi-Stage Build

```dockerfile
# Stage 1: Restore - Cache otimizado
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
COPY *.csproj .
RUN dotnet restore

# Stage 2: Build
FROM restore AS build
RUN dotnet build -c Release

# Stage 3: Runtime - Imagem final leve
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
COPY --from=build /app/publish .
```

### Benefícios

- ✅ **Imagens menores** (redução de ~80%)
- ✅ **Build mais rápido** (caching de dependências)
- ✅ **Segurança melhor** (SDK não incluído na imagem final)
- ✅ **Produção otimizada** (apenas runtime necessário)

### Tamanho das Imagens

```
eduonline/auth-api:latest       ~180 MB (runtime alpine)
vs. without multi-stage          ~900 MB (com SDK)
```

---

## 🐳 SQL Server - Seed Automático

### Inicialização

```bash
# O arquivo infra/database/InitDB.sql é executado automaticamente quando
# o container SQL Server inicia (via ENTRYPOINT no Docker)

# Verificar schema criado
docker exec -it eduonline-sqlserver \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssw0rd2024! \
  -Q "SELECT name FROM sys.schemas"

# Resultado esperado:
# dbo
# Auth
# Conteudos
# Alunos
# Pagamentos
```

### Scripts de Seed

| Schema | Tabelas | Dados Iniciais |
|--------|---------|----------------|
| **Auth** | Users, Roles, UserRoles | admin@eduonline.com |
| **Conteudos** | Cursos, Aulas | 3 cursos de exemplo |
| **Alunos** | AlunosPerfil, Matrículas, ProgressoAulas, Certificados | aluno@eduonline.com |
| **Pagamentos** | Pagamentos, HistoricoTransacoes | - |

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflows (Passo 2 - Completo)

O projeto utiliza **4 workflows automáticos** para garantir qualidade e entrega contínua:

| Workflow | Trigger | Duração | Função |
|----------|---------|---------|--------|
| **CI** | Push/PR em `develop` | 10-15m | Build, Test, Lint, CodeQL |
| **CD** | Push `main` / Tag `v*.*.*` | 8-20m | Build, Push Docker Hub, Release |
| **Security** | Push/PR + schedule | 20-30m | Scanning de vulnerabilidades |
| **PR** | PR events | 2-5m | Validação, labels, reviewers |

**Documentação Completa:**
- 📖 [docs/CI-CD-PIPELINE.md](docs/CI-CD-PIPELINE.md) - Pipeline overview
- 📖 [docs/GITHUB-ACTIONS-SETUP.md](docs/GITHUB-ACTIONS-SETUP.md) - Setup secrets
- 📖 [CI-CD-TESTING-GUIDE.md](CI-CD-TESTING-GUIDE.md) - Guia de testes

### Status Badges

```markdown
[![CI](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/ci.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/ci.yml)
[![CD](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/cd.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/cd.yml)
[![Security](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/security.yml/badge.svg)](https://github.com/seu-usuario/DevXpertMod05/actions/workflows/security.yml)
```

---

## 🔄 Próximos Passos - Kubernetes

### Preparação para K8s

1. **Gerar manifests Kubernetes** a partir dos Dockerfiles:
   ```bash
   kubectl create deployment auth-api --image=eduonline/auth-api:latest --dry-run=client -o yaml
   ```

2. **Criar ConfigMaps** para variáveis de ambiente:
   ```yaml
   apiVersion: v1
   kind: ConfigMap
   metadata:
	 name: eduonline-config
   data:
	 JWT_SECRET: "your-secret-key"
	 DB_SERVER: "sqlserver.default.svc.cluster.local"
   ```

3. **Deployments** com replicas e autoscaling:
   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
	 name: auth-api
   spec:
	 replicas: 3
	 strategy:
	   type: RollingUpdate
   ```

4. **Services** para descoberta de serviços:
   ```yaml
   apiVersion: v1
   kind: Service
   metadata:
	 name: auth-api
   spec:
	 type: ClusterIP
   ```

5. **Ingress** para roteamento:
   ```yaml
   apiVersion: networking.k8s.io/v1
   kind: Ingress
   metadata:
	 name: eduonline-ingress
   ```

### Recursos de Referência

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Docker to Kubernetes Migration](https://kubernetes.io/docs/setup/release/docker-to-k8s/)
- [Helm Charts](https://helm.sh/) para templating

---

## 📚 Estrutura de Diretórios

```
.
├── src/
│   ├── EduOnline.Auth.ApiRest/
│   │   ├── Dockerfile                    # Multi-stage build
│   │   ├── Program.cs
│   │   └── ...
│   ├── EduOnline.Conteudos.ApiRest/
│   ├── EduOnline.Alunos.ApiRest/
│   ├── EduOnline.Pagamentos.ApiRest/
│   ├── EduOnline.WebApps.ApiRest/ (BFF)
│   ├── EduOnline.Core/
│   ├── EduOnline.Core.Api/
│   └── EventSourcing/
├── infra/
│   ├── database/
│   │   ├── InitDB.sql                   # Seed script
│   │   └── init-db.sh                   # Automação
│   └── kubernetes/                       # Manifests K8s (futuros)
├── docker-compose.yml                    # Orquestração principal
├── docker-compose.override.yml          # Config desenvolvimento
├── .dockerignore                        # Otimização de build
├── .env                                 # Variáveis de ambiente
└── DOCKER-README.md                     # Este arquivo
```

---

## 🤝 Contribuindo

1. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
2. Commit suas mudanças (`git commit -m 'Add MinhaFeature'`)
3. Push para a branch (`git push origin feature/MinhaFeature`)
4. Abra um Pull Request

---

## 📝 Licença

Este projeto é parte do programa MBA DevXpert - Módulo 05.

---

## 📚 Documentação Completa

### Docker e execução local
- 📖 `DOCKER-README.md` (este arquivo)
- 📖 `README.md` - visão geral atualizada
- 📖 `infra/kubernetes/README.md` - base dos manifests e scripts de Kubernetes

### CI/CD e automação
- 📖 `docs/CI-CD-PIPELINE.md` - pipeline overview
- 📖 `docs/GITHUB-ACTIONS-SETUP.md` - setup de secrets
- 📖 `CI-CD-TESTING-GUIDE.md` - guia de testes

### Segurança e compliance
- 📖 `SECURITY-COMPLIANCE-MATRIX.md` - resumo do estado atual
- 📖 `infra/security/COMPLIANCE-CHECKLIST.md` - checklist técnico
- 📖 `infra/observability/DEPLOYMENT.md` - implantação da stack de observabilidade

---

## 📞 Suporte & Documentação

Para dúvidas ou problemas:

1. Consulte a seção [Troubleshooting](#troubleshooting)
2. Verifique os [Comandos Úteis](#comandos-úteis)
3. Leia a documentação específica do cenário que deseja validar
4. Abra uma issue no GitHub
5. Consulte: [docs.docker.com](https://docs.docker.com)

---

**Última atualização:** 2026  
**Versão:** `3.0.0` (documentação alinhada ao estado atual)  
**Status:** ✅ Documentação atualizada para o estado real do projeto
