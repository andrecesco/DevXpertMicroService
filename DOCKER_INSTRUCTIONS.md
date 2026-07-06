# Instruções Docker - EduOnline Microservices

## Estrutura

Este projeto contém 5 microserviços .NET 10.0:
- **Alunos API** - Gerenciamento de alunos (porta 8001)
- **Auth API** - Autenticação e autorização (porta 8002)
- **Conteudos API** - Gerenciamento de conteúdos (porta 8003)
- **Pagamentos API** - Processamento de pagamentos (porta 8004)
- **WebApps API** - Aplicações web (porta 8005)

## Dependências

- **SQL Server 2022** - Banco de dados principal
- **RabbitMQ 3.13** - Message broker para comunicação entre serviços

## Pré-requisitos

- Docker instalado
- Docker Compose instalado
- 4GB de RAM mínimo
- Conexão à internet para download das imagens

## Como Usar

### 1. Iniciar todos os serviços

```bash
docker-compose up -d
```

Esse comando irá:
- Baixar as imagens necessárias
- Construir as imagens Docker de cada microserviço
- Iniciar os containers em modo desacoplado (background)
- Configurar a rede e volumes

### 2. Verificar status dos containers

```bash
docker-compose ps
```

Você deve ver todos os 7 containers rodando (SQL Server, RabbitMQ e 5 APIs).

### 3. Ver logs de um serviço específico

```bash
# Alunos API
docker-compose logs -f alunos-api

# Auth API
docker-compose logs -f auth-api

# RabbitMQ
docker-compose logs -f rabbitmq

# SQL Server
docker-compose logs -f sqlserver
```

### 4. Acessar os serviços

- **Alunos API**: http://localhost:8001/swagger
- **Auth API**: http://localhost:8002/swagger
- **Conteudos API**: http://localhost:8003/swagger
- **Pagamentos API**: http://localhost:8004/swagger
- **WebApps API**: http://localhost:8005/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **SQL Server**: localhost:1433

### 5. Conectar ao SQL Server

Use qualquer cliente SQL (SSMS, Azure Data Studio, etc.):
- **Server**: localhost,1433
- **User**: sa
- **Password**: Password@123
- **Databases**: EduOnlineAlunos, EduOnlineAuth, EduOnlineConteudos, EduOnlinePagamentos, EduOnlineWebApps

### 6. Parar os serviços

```bash
docker-compose down
```

Para remover também os volumes (dados persistentes):
```bash
docker-compose down -v
```

## Troubleshooting

### Serviço não inicia

1. Verifique se as portas estão disponíveis
2. Aumente a memória do Docker
3. Veja os logs: `docker-compose logs nome_do_servico`

### Erro de conexão com banco de dados

- Aguarde 30-40 segundos para o SQL Server iniciar completamente
- Verifique a senha no compos

### Erro ao buildar as imagens

```bash
# Limpe as imagens antigos
docker-compose down -v
docker image prune -a

# Recrie tudo
docker-compose up --build -d
```

## Variáveis de Ambiente

Você pode customizar o arquivo `docker-compose.yml`:

- `SA_PASSWORD`: Senha do SQL Server
- `RABBITMQ_DEFAULT_USER/PASS`: Credenciais do RabbitMQ
- Portas dos serviços
- Banco de dados e connection strings

## Health Checks

Cada API possui um health check configurado que verifica a saúde da aplicação:
```bash
curl http://localhost:8001/health
curl http://localhost:8002/health
curl http://localhost:8003/health
curl http://localhost:8004/health
curl http://localhost:8005/health
```

## Desenvolvimento Local

Para continuar desenvolvendo localmente sem Docker:
1. Instale SQL Server 2022 localmente
2. Instale RabbitMQ
3. Configure as connection strings em `appsettings.json`
4. Execute: `dotnet run` em cada projeto ApiRest

## Production Deployment

Para ambiente de produção:

1. Use registries privados (Docker Hub, Azure Container Registry, etc.)
2. Altere as senhas padrão
3. Configure ingress com Nginx ou Kong
4. Use Kubernetes ou Docker Swarm
5. Configure backups do SQL Server
6. Implemente CI/CD com GitHub Actions ou Azure DevOps
7. Use variáveis de ambiente seguras (secrets)

## Documentação Adicional

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Microsoft SQL Server Container](https://hub.docker.com/_/microsoft-mssql-server)
- [RabbitMQ Container](https://hub.docker.com/_/rabbitmq)
