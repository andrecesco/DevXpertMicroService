# **EduOnline - Plataforma de Educação online**

## **1. Apresentação**

Bem-vindo ao repositório do projeto **EduOnline**. A solução atual representa uma plataforma educacional distribuída desenvolvida em **.NET 10**, com múltiplos bounded contexts, **DDD**, **CQRS**, mensageria e **event sourcing**.

O ecossistema está organizado em APIs independentes para **Autenticação**, **Conteúdos**, **Alunos**, **Pagamentos** e **BFF**, além dos projetos de suporte, testes automatizados e infraestrutura local para execução com Docker e Kubernetes.

## **2. Tecnologias Utilizadas**

- **Linguagem de Programação:** C#
- **Framework:** ASP.NET Core Web API
- **Target Framework:** .NET 10
- **Banco de Dados:** SQL Server
- **Event Sourcing:** EventStoreDB na porta `2113`
- **Autenticação e Autorização:** ASP.NET Core Identity + JWT
- **Observabilidade:** OpenTelemetry, Prometheus e Jaeger
- **Containerização:** Docker e Docker Compose
- **Orquestração local:** Kubernetes manifests em `infra/kubernetes`
- **Documentação da API:** Swagger/OpenAPI

## **3. Funcionalidades Implementadas**

- Autenticação com JWT e controle por roles (`Administrador` e `Aluno`)
- Cadastro e gerenciamento de cursos e aulas
- Cadastro de alunos e matrículas
- Processamento de pagamentos
- Acompanhamento de progresso e emissão de certificados
- Persistência de eventos do contexto de Alunos no Event Store
- Health checks e endpoints de Swagger para todas as APIs
- Stack local com SQL Server, EventStoreDB e observabilidade básica

## **4. Como Executar o Projeto**

### **Pré-requisitos**

- .NET SDK 10.0 ou superior, para desenvolvimento e execução fora do Docker
- Docker Desktop com Docker Compose v2
- Git
- Visual Studio 2026 ou superior, ou outra IDE compatível
- Para execução em Kubernetes:
  - `kubectl`
  - `kind` **ou** `minikube`

### **Opção A - Execução local com Docker Compose (rápida)**

1. **Clone o repositório**
2. **Configure as variáveis do arquivo `.env`**, se necessário
3. **Suba a stack principal**:
   - `docker compose up -d --build`
   - ou `.\docker-init.ps1 -Action up`
4. **Valide os containers**:
   - `docker compose ps`
5. **Acesse os serviços** conforme a tabela da seção de documentação da API

### **Opção B - Execução local com Kubernetes (fluxo para avaliação)**

> Esta opção atende ao cenário de avaliação de Kubernetes descrito nos requisitos do projeto.

#### **B.1 Build das imagens locais**

Antes de aplicar os manifests, gere as imagens usadas pelos Deployments:

- `docker compose build auth-api conteudos-api alunos-api pagamentos-api bff-api`

As imagens geradas serão:
- `eduonline/auth-api:latest`
- `eduonline/conteudos-api:latest`
- `eduonline/alunos-api:latest`
- `eduonline/pagamentos-api:latest`
- `eduonline/bff-api:latest`

#### **B.2 Subir cluster e aplicar manifests**

Escolha uma das opções abaixo.

**Kind (recomendado para validação rápida):**
1. `./infra/kubernetes/scripts/setup-kind.ps1`

**Minikube:**
1. `./infra/kubernetes/scripts/setup-minikube.ps1`

**Cluster já existente:**
1. `./infra/kubernetes/scripts/apply.ps1`

Os manifests são aplicados com Kustomize a partir de `infra/kubernetes/kustomization.yaml` e incluem:
- `Deployment`, `Service`, `ConfigMap`, `Secret`
- `livenessProbe` e `readinessProbe`
- `Ingress`, `HPA`, `NetworkPolicy`, `RBAC`

#### **B.3 Conferência pós-deploy (checklist do avaliador)**

Execute os comandos abaixo:

1. Namespace e recursos:
   - `kubectl get ns`
   - `kubectl get all -n eduonline`
2. Configuração e segredos:
   - `kubectl get configmap -n eduonline`
   - `kubectl get secret -n eduonline`
3. Probes e réplicas dos microsserviços:
   - `kubectl get deploy -n eduonline`
   - `kubectl describe deploy auth-api -n eduonline`
4. Saúde dos pods:
   - `kubectl get pods -n eduonline -w`

#### **B.4 Acesso às APIs no Kubernetes**

Como os `Service` das APIs estão como `ClusterIP`, use port-forward para testes locais:

- Auth API: `kubectl port-forward svc/auth-api 5000:5000 -n eduonline`
- Conteúdos API: `kubectl port-forward svc/conteudos-api 5001:5001 -n eduonline`
- Alunos API: `kubectl port-forward svc/alunos-api 5002:5002 -n eduonline`
- Pagamentos API: `kubectl port-forward svc/pagamentos-api 5003:5003 -n eduonline`
- BFF API: `kubectl port-forward svc/bff-api 5004:5004 -n eduonline`

Após o port-forward, valide:
- `http://localhost:5000/health`
- `http://localhost:5001/health`
- `http://localhost:5002/health`
- `http://localhost:5003/health`
- `http://localhost:5004/health`

### **Execução apenas de uma API no modo desenvolvimento**

- Abra a solução `EduOnline.slnx` no Visual Studio
- Inicie o projeto desejado em modo de depuração
- O cenário principal do repositório, porém, é a execução via Docker Compose ou Kubernetes local

## **5. Instruções de Configuração**

- As configurações principais ficam em `.env`, `docker-compose.yml` e nos `appsettings.json` de cada serviço
- O seed inicial prepara dados de demonstração para os perfis de administrador e aluno
- As migrações e seeds são aplicadas automaticamente pelos serviços conforme o ambiente configurado
- As roles usadas pela solução são apenas `Administrador` e `Aluno`
- O usuário do ASP.NET Identity para alunos é um GUID e corresponde ao `Memo` do aluno

## **6. Documentação da API**

Cada API expõe Swagger/OpenAPI no ambiente local:

- **Auth API**: `http://localhost:5000/swagger`
- **Conteúdos API**: `http://localhost:5001/swagger`
- **Alunos API**: `http://localhost:5002/swagger`
- **Pagamentos API**: `http://localhost:5003/swagger`
- **BFF API**: `http://localhost:5004/swagger`

Endpoints de saúde:

- `http://localhost:5000/health`
- `http://localhost:5001/health`
- `http://localhost:5002/health`
- `http://localhost:5003/health`
- `http://localhost:5004/health`

## **7. Docker do EventStoreDB para event sourcing**

O ambiente padrão do repositório já sobe o EventStoreDB via `docker-compose.yml`, na porta `2113`.

Se for necessário executar manualmente o serviço, utilize a imagem configurada no projeto ou reutilize o container padrão do Compose. Após o container estar ativo, o endpoint fica disponível em:

`http://localhost:2113`

### **7.1. Docker do KurrentDb para event sourcing para teste locais**
Basta rodar os comandos do docker abaixo, isso é importante para o correto funcionamento da aplicação.

`docker pull docker.kurrent.io/kurrent-latest/kurrentdb:latest`

`docker run --name kurrentdb-node -it -p 2113:2113 \
    docker.kurrent.io/kurrent-latest/kurrentdb:latest --insecure --run-projections=All \
    --enable-atom-pub-over-http`

Após o container estiver rodando é possível acessar através do link http://localhost:2113


## **8. Troubleshooting e limpeza (Kubernetes)**

- **Pods em `ImagePullBackOff`**:
  - Refaça o build local: `docker compose build auth-api conteudos-api alunos-api pagamentos-api bff-api`
  - Confirme o contexto ativo com `kubectl config current-context` (o resultado esperado pode aparecer como `eduonline`, `kind-eduonline` ou `minikube`, conforme o ambiente).
  - Se necessário, liste contextos com `kubectl config get-contexts` e selecione com `kubectl config use-context <nome-do-contexto>`.
- **Recursos não ficaram `Ready`**:
  - `kubectl get pods -n eduonline`
  - `kubectl describe pod <nome-do-pod> -n eduonline`
  - `kubectl logs <nome-do-pod> -n eduonline`
- **Reaplicar todos os manifests**:
  - `./infra/kubernetes/scripts/apply.ps1`
- **Limpar ambiente Kubernetes (namespace da aplicação)**:
  - `kubectl delete namespace eduonline`

## **9. Avaliação**

- Este projeto é parte de um curso acadêmico e não aceita contribuições externas.
- Para feedbacks ou dúvidas utilize o recurso de Issues.
- O arquivo `FEEDBACK.md` é um resumo das avaliações do instrutor e deverá ser modificado apenas por ele.

## **10. Estrutura da solução e documentação complementar**

- `src/` - APIs, domínios, dados, aplicação, core e BFF
- `test/` - testes unitários e de integração
- `docs/` - documentação de CI/CD e configuração dos workflows
- `infra/kubernetes/` - manifests e guias da infraestrutura local em Kubernetes
- `infra/kubernetes/README.md` - guia complementar com foco nos manifests Kubernetes
- `DOCKER-README.md` - guia detalhado do ambiente Docker
- `CI-CD-TESTING-GUIDE.md` - validação dos workflows de CI/CD
- `SECURITY-COMPLIANCE-MATRIX.md` - resumo do estado de segurança e compliance
