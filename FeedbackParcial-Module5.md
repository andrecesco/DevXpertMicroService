# Feedback – Avaliação Geral

> **Tipo desta avaliação: PARCIAL.** Este relatório contém apenas as seções qualitativas. A Matriz de Avaliação e a Nota Final não são calculadas nem publicadas nesta etapa.

## Organização e Estrutura do Projeto

- **Pontos positivos:**
  - Separação clara entre `src/` (17 projetos), `test/` (9 projetos de teste), `infra/`, `docs/` e `scripts/`.
  - Arquivo de solução na raiz: `EduOnline.slnx`. É o formato XML novo (VS 2026 / .NET 10) e o `dotnet build EduOnline.slnx` funcionou nesta máquina — atende ao requisito, com a ressalva de que exige tooling recente.
  - Dockerfile individual por serviço, colocado junto ao respectivo projeto: `src/EduOnline.Auth.ApiRest/Dockerfile`, `src/EduOnline.Conteudos.ApiRest/Dockerfile`, `src/EduOnline.Alunos.ApiRest/Dockerfile`, `src/EduOnline.Pagamentos.ApiRest/Dockerfile`, `src/EduOnline.WebApps.ApiRest/Dockerfile` e `src/EduOnline.Api.Status/Dockerfile`.
  - Manifestos Kubernetes organizados por tipo de recurso em `infra/kubernetes/` (`deployments/`, `services/`, `hpa/`, `ingress/`, `networkpolicies/`, `statefulsets/`), com `kustomization.yaml` agregador.
  - `.gitignore` cobre corretamente `[Bb]in/` e `[Oo]bj/` (linhas 34-35) e também `[Tt]est[Rr]esult*/` (linha 48). Verificado com `git ls-files`: **nenhum** binário ou artefato de build está versionado.
  - Os arquivos acessórios apontados no Módulo 4 (`fact_aaa.snippet`, `test-output.txt`, `test-output-full.txt`) foram **removidos** do controle de versão.
  - Os 5 bounded contexts do escopo estão fisicamente isolados em projetos próprios.

- **Pontos negativos:**
  - **Duplicação massiva da árvore de infraestrutura.** `infra/security/` e `infra/kubernetes/security/` são cópias quase idênticas (mesmos `rbac/`, `tls/`, `vault/`, `audit/`, `network-policies/`). O mesmo ocorre entre `infra/observability/` e `infra/kubernetes/observability/` (`alertmanager/`, `elasticsearch/`, `fluentd/`, `jaeger/`, `otel-collector/`). Apenas a cópia sob `infra/kubernetes/` é referenciada pelo `infra/kubernetes/kustomization.yaml:39-60`; a árvore `infra/observability/` e `infra/security/` está **órfã** (exceto dois arquivos usados pelo compose). São ~40 YAMLs duplicados que divergirão silenciosamente.
  - **Vault aparece em três lugares distintos**: `infra/kubernetes/vault/`, `infra/kubernetes/security/vault/` e `infra/security/vault/`.
  - **Arquitetura superdimensionada frente ao escopo.** O escopo pede "restart policies, logs e métricas" e "health checks". O repositório entrega Vault + External Secrets Operator, Elasticsearch + Kibana, Fluentd + Promtail, Jaeger, Prometheus + Alertmanager, OTEL Collector, cert-manager/ClusterIssuer, PodSecurityPolicies, políticas de auditoria de API server e RBAC por serviço. Isso é muito além do pedido, não é validado por nenhum teste ou pipeline, e — como detalhado nas seções seguintes — convive com um fluxo básico de Kubernetes que **não sobe**. Amplitude foi priorizada sobre funcionamento.
  - `infra/kubernetes/security/rbac/pod-security-policies.yaml` usa PodSecurityPolicy, recurso **removido do Kubernetes desde a 1.25**. É manifesto morto: não aplicará em nenhum cluster atual.
  - Documentação de infraestrutura dispersa e redundante na raiz: `DOCKER-README.md`, `CI-CD-TESTING-GUIDE.md`, `SECURITY-COMPLIANCE-MATRIX.md`, `ProjetoMod05.md`, além de `infra/ARCHITECTURE.md` e `infra/kubernetes/README.md`.
  - O `FEEDBACK.md` anterior existia mas estava **vazio** (0 bytes) — ver seção "Resolução de Feedbacks".

## Pipeline CI/CD

- **Pontos positivos:**
  - Existe workflow em `.github/workflows/standard.yml`, disparado em `push` e `pull_request` para `main` (linhas 3-7) — gatilhos corretos.
  - Build da solução em Release: `.github/workflows/standard.yml:22`.
  - Verificado via `gh run list`: as 3 execuções registradas concluíram com **success**, inclusive uma disparada por `pull_request`.
  - **Uso exemplar de Pull Requests.** `gh pr list -s closed` mostra 13 PRs com branches temáticas (`add-workflow-standard`, `add-script-sql`, `add-healthchecks-apis-infra`, `Modulo05`, `adicionando-bff`, `pagamentos-api`...), todas mergeadas em `main`. O branching model atende ao escopo.

- **Pontos negativos:**
  - **O pipeline não roda testes.** Não há nenhum `dotnet test` em `.github/workflows/standard.yml`. O escopo exige explicitamente "Build e testes automatizados". Este é o ponto mais grave do CI: como demonstrado na seção Qualidade do Código, existem **testes falhando no repositório** e o pipeline passa em verde mesmo assim.
  - **Não há deploy para o Docker Hub.** O escopo exige "Deploy automático das imagens no Docker Hub". Não existe `docker/login-action`, `docker push`, nem `docker/build-push-action` em lugar algum do workflow. As imagens `andrecesco/eduonline-*` **existem** no Docker Hub (verificado via API do registry: `andrecesco/eduonline-auth-api`, `-conteudos-api`, `-alunos-api`, `-pagamentos-api`, `eduonline-bff`, todas com tag `latest`), mas foram publicadas **manualmente** — nenhum passo do pipeline as produz. O requisito não está atendido por automação.
  - **Não há lint nem análise estática.** O escopo exige "Lint e análise estática de código". Existe um `sonar-project.properties` completo na raiz, porém **nenhum workflow o referencia** (`grep sonar .github/workflows/` não retorna nada). Também não há `dotnet format --verify-no-changes`. O arquivo de configuração é uma promessa não executada — não conta como implementação.
  - **O job `docker` não entrega nada.** `.github/workflows/standard.yml:30-33` executa `docker compose build` seguido de `docker compose up -d` no runner efêmero do GitHub Actions. O runner é destruído em seguida; subir containers ali não valida nada (não há health check, smoke test ou `docker compose ps` verificando estado) e não publica imagem. É um passo sem propósito que consome ~3 minutos por execução.
  - **Os jobs `build` e `docker` são independentes** (não há `needs:`), rodam em paralelo e o `docker` refaz o checkout e o build do zero. Se o build da solução falhar, o job docker ainda executa.
  - O `docker compose up -d` no CI depende de SQL Server e EventStore e não aguarda healthy; combinado com os problemas de provisionamento de banco descritos adiante, o "verde" do pipeline não representa um ecossistema funcional.

## Containerização

- **Pontos positivos:**
  - **Multi-stage build consistente nos 6 Dockerfiles**, com 3 estágios bem nomeados (`restore` → `build` → `runtime`). Ex.: `src/EduOnline.Auth.ApiRest/Dockerfile:3,16,33`.
  - **Otimização de cache de camadas correta**: os `.csproj` são copiados isoladamente antes do `dotnet restore` (`src/EduOnline.Auth.ApiRest/Dockerfile:7-13`), antes do `COPY . .` — prática recomendada e bem aplicada.
  - **Imagens base oficiais e enxutas**: `mcr.microsoft.com/dotnet/sdk:10.0-alpine` e `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` — oficiais, atualizadas e Alpine para reduzir tamanho.
  - Tratamento correto de globalização no Alpine: `RUN apk add --no-cache icu-libs` + `ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` (`src/EduOnline.Auth.ApiRest/Dockerfile:36-38`).
  - `HEALTHCHECK` declarado em todos os Dockerfiles (ex.: `src/EduOnline.Auth.ApiRest/Dockerfile:47-48`).
  - `.dockerignore` presente na raiz e bem construído: exclui `**/bin`, `**/obj`, `**/.git`, `**/.env`, `**/*.pfx`, `**/appsettings.Development.json`.
  - `docker-compose.yml` maduro: healthchecks com `condition: service_healthy` nos `depends_on`, `restart: always/on-failure`, limites de recursos, rotação de logs (`max-size: 10m`, `max-file: 3`) e rede dedicada.

- **Pontos negativos:**
  - **`dotnet build` seguido de `dotnet publish` é redundante** em todos os Dockerfiles (ex.: `src/EduOnline.Auth.ApiRest/Dockerfile:23-30`). O `dotnet publish` já compila. O `build` para `/app/build` gera uma camada inteira que é descartada (o runtime só copia `/app/publish`, linha 41). Isso praticamente **dobra o tempo de build** e infla o cache sem benefício. Remover o passo `dotnet build`.
  - **Nenhum Dockerfile define `USER`** — as imagens rodam como **root** por padrão. Os Deployments do Kubernetes compensam com `runAsNonRoot: true` / `runAsUser: 64198` (`infra/kubernetes/deployments/auth-api-deployment.yaml:21-22`), mas o `docker-compose.yml` **não** define `user:`, então na execução local via compose todos os 6 containers rodam como root. A imagem deveria ser segura por padrão.
  - `docker-compose.yml:11` define explicitamente `user: root` no serviço `sqlserver`.
  - As imagens declaradas no compose (`andrecesco/eduonline-auth-api:latest`, linha 105) **não coincidem** com as usadas nos Deployments (`eduonline/auth-api:latest`) — ver a seção seguinte, onde isso quebra o deploy.
  - Uso exclusivo da tag `:latest` em todas as imagens, sem versionamento por SHA/tag semântica. Combinado com a ausência de push automatizado, não há rastreabilidade entre commit e imagem.

## Orquestração Kubernetes

- **Pontos positivos:**
  - Cobertura completa de recursos para os 5 microsserviços: `Deployment` + `Service` + `HPA` para cada um (`infra/kubernetes/deployments/`, `services/`, `hpa/`), mais `ConfigMap` (`configmap.yaml`), `Secret` (`secrets.yaml`), `Namespace`, `Ingress`, `NetworkPolicy`, `PV/PVC` e `ServiceAccount`.
  - Os Deployments são de boa qualidade técnica. Em `infra/kubernetes/deployments/auth-api-deployment.yaml`:
    - `readinessProbe` e `livenessProbe` HTTP em `/health` com delays distintos e coerentes (linhas 69-80).
    - `resources.requests` e `resources.limits` definidos (linhas 62-68) — pré-requisito real para o HPA funcionar.
    - `securityContext` robusto: `runAsNonRoot`, `runAsUser: 64198`, `seccompProfile: RuntimeDefault`, `allowPrivilegeEscalation: false`, `capabilities.drop: [ALL]` e `automountServiceAccountToken: false` (linhas 19-35).
    - `replicas: 3` e labels padronizadas (`app.kubernetes.io/name`, `part-of`).
  - Dependências stateful modeladas como `StatefulSet` (sqlserver, rabbitmq, eventstore) — escolha correta.
  - `kustomization.yaml` agrega os recursos numa ordem coerente, e há scripts de conveniência (`infra/kubernetes/scripts/setup-kind.ps1`, `setup-minikube.ps1`, `apply.ps1`).

- **Pontos negativos:**
  - **BLOQUEADOR: os 5 Deployments referenciam imagens que não existem.** Todos apontam para `eduonline/<serviço>:latest` (`infra/kubernetes/deployments/auth-api-deployment.yaml:29` e equivalentes nos outros 4). Verificações realizadas:
    1. `eduonline/auth-api` **não existe no Docker Hub** (API do registry retorna **404**).
    2. O `docker-compose.yml:105` tagueia a imagem como `andrecesco/eduonline-auth-api:latest` — **nome diferente**. Logo, `docker compose build` **nunca** produz `eduonline/auth-api:latest`.
    3. `infra/kubernetes/scripts/setup-kind.ps1` (7 linhas) apenas cria o cluster e roda `kubectl apply -k`; **não há `kind load docker-image`** nem `minikube image load`. Mesmo que os nomes coincidissem, o cluster não enxergaria imagens locais do Docker daemon.
    4. `imagePullPolicy: IfNotPresent` (linha 30) faz o kubelet tentar o pull no Docker Hub, onde a imagem não existe.
    - **Consequência:** os 5 Deployments entram em `ErrImagePull`/`ImagePullBackOff`. O `apply.ps1:9` (`kubectl wait --for=condition=available deployment --all --timeout=600s`) ficará bloqueado 10 minutos e falhará. **O ecossistema não sobe no cluster** — o critério de sucesso "cada serviço roda sob Kubernetes" não é atendido.
  - **Secrets em texto puro versionados.** `infra/kubernetes/secrets.yaml` usa `stringData` com valores reais commitados: `SA_PASSWORD: P@ssw0rd2024!` (linha 8), 4 connection strings com a senha embutida (linhas 9-12), o segredo JWT `AppTokenSettings__Segredo` / `AppSettings__Segredo` (linhas 13-14) e `RabbitMq__Password` (linha 15). Um `Secret` do Kubernetes **não criptografa nada** — apenas codifica. Commitar o manifesto equivale a commitar as credenciais. Ironicamente, o repositório contém um stack inteiro de Vault + External Secrets (`infra/kubernetes/vault/externalsecret.yaml`) que **não é usado** por nenhum Deployment: todos consomem `secretRef: eduonline-secrets` (linha 45).
  - **Inconsistência de banco entre compose e cluster.** O `secrets.yaml` aponta as 4 connection strings para **um único banco compartilhado** `EduOnlineDB`, enquanto o `docker-compose.yml` usa **um banco por serviço** (`EduOnlineAuthDb`, `EduOnlineConteudosDb`, `EduOnlineAlunosDb`, `EduOnlinePagamentosDb`). Um banco único compartilhado entre bounded contexts contradiz o isolamento de dados que o Módulo 4 elogiou.
  - O `EduOnline.Api.Status` (Status/HealthChecksUI) existe no compose (`docker-compose.yml:329`) e tem Dockerfile, mas **não possui manifesto Kubernetes algum**. A observabilidade agregada some no cluster.
  - `infra/kubernetes/kustomization.yaml:60` inclui `security`, que puxa `pod-security-policies.yaml` (PSP removido na 1.25+). Em cluster moderno o `kubectl apply -k` pode falhar por tipo desconhecido.
  - **A execução real em cluster não foi validada** nesta revisão (kind/minikube não foram executados). A análise acima é estática, mas a inexistência da imagem no registry foi **confirmada empiricamente** via API do Docker Hub.

## Resiliência e Observabilidade

- **Pontos positivos:**
  - **Polly implementado corretamente no BFF**: `src/EduOnline.Core.Api/Extensions/PollyExtensions.cs` define retry com backoff (`WaitAndRetryAsync`, linha 12), aplicação via `AddPolicyHandler` (linha 37) e **circuit breaker** com `AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))` (linha 38).
  - Health checks centralizados em `src/EduOnline.Core.Api/Extensions/ObservabilityExtensions.cs:92-126`, expondo `/health` e reaproveitados por todas as APIs.
  - Instrumentação OpenTelemetry presente, com `OTEL_EXPORTER_OTLP_ENDPOINT` configurado em todos os serviços no compose e no cluster (`infra/kubernetes/deployments/auth-api-deployment.yaml:53-54`).
  - Restart policies definidas em todos os serviços do compose (`restart: always` / `on-failure`) e rotação de logs configurada.
  - HPA para os 5 microsserviços (`infra/kubernetes/hpa/`), com `resources.requests` corretamente definidos para dar base às métricas.

- **Pontos negativos:**
  - **`/health/ready` e `/health/live` não existem.** `ObservabilityExtensions.cs:126` mapeia **apenas** `app.MapHealthChecks("/health", ...)`. Não há `MapHealthChecks("/health/ready")` nem `/health/live` em nenhum lugar do código. Isso é comprovado por **teste falhando** no próprio repositório:
    ```
    Failed GET /health/ready deve retornar 200 na Auth API
    Expected response.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
    test/EduOnline.Auth.ApiRest.IntegrationTest/HealthChecksIntegrationTest.cs:line 30
    ```
    Existe teste equivalente em `test/EduOnline.Pagamentos.IntegrationTest/HealthChecksIntegrationTest.cs:26`. O projeto **escreveu os testes de readiness/liveness mas nunca implementou os endpoints** — e como o pipeline não roda testes, a falha nunca foi detectada.
  - **Sem distinção entre liveness e readiness.** Os probes do Kubernetes apontam ambos para `/health` (`auth-api-deployment.yaml:71` e `:77`). Como `/health` agrega as dependências (SQL Server, RabbitMQ), uma indisponibilidade momentânea do banco faz o **liveness** falhar e o kubelet **reiniciar o pod** — quando o correto seria apenas removê-lo do balanceamento via readiness. É um antipadrão clássico que provoca crash-loop em cascata sob falha de dependência, contrariando diretamente o critério "falhas em um serviço não derrubam o ecossistema".
  - **Retry/circuit breaker seguem restritos ao BFF** — apontamento do Módulo 4 **não resolvido**. `PollyExtensions.cs` é o único ponto com políticas de resiliência; as integrações internas continuam sem proteção equivalente.
  - Todo o stack de observabilidade (Prometheus, Grafana com 4 dashboards, Jaeger, Elasticsearch/Kibana, Fluentd, Alertmanager) está declarado, mas **nenhum dashboard, regra de alerta ou trace foi validado**, e metade dos manifestos (`infra/observability/`) sequer é referenciada pelo kustomization. Não há evidência executável de que a observabilidade funcione.

## Qualidade do Código

- **Pontos positivos:**
  - **O build da solução passa sem erros**: `dotnet build EduOnline.slnx` compila os 17 projetos de `src/` e os 9 de `test/`.
  - Separação em camadas coerente por contexto (`ApiRest`, `Application`, `Domain`, `Data`), com nomes descritivos e em português consistente com o domínio.
  - 9 projetos de teste cobrindo unidade e integração dos 4 contextos principais.
  - **26 dos 27 testes** da Auth API passam; a suíte de `EduOnline.Alunos.Application` atinge **70%** de cobertura de linha, com vários Commands em 100%.
  - Migrações automáticas no startup confirmadas nos 4 serviços: `app.UseDbMigrationHelper()` em `src/EduOnline.Auth.ApiRest/Program.cs:49`, `src/EduOnline.Conteudos.ApiRest/Program.cs:50`, `src/EduOnline.Alunos.ApiRest/Program.cs:51` e `src/EduOnline.Pagamentos.ApiRest/Program.cs:51`. Seed configurável via `SeedSettings.EnableSeedData` (`appsettings.json`, default `true`).
  - O apontamento do Módulo 4 sobre código comentado foi parcialmente endereçado (commit `2ca7d93` "removendo códigos comentados e rodando clean up").

- **Pontos negativos:**
  - **TESTES FALHANDO.** `dotnet test --collect:"XPlat Code Coverage"` na raiz da solução resulta em **falha** (exit code 1). Saída real observada:

    ```
    Failed!  - Failed: 1, Passed: 26, Skipped: 0, Total: 27 - EduOnline.Auth.ApiRest.IntegrationTest.dll
      Failed GET /health/ready deve retornar 200 na Auth API
        Expected HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.

    Failed!  - Failed: 4, Passed: 3, Skipped: 0, Total: 7 - EduOnline.Alunos.IntegrationTest.dll
      Failed 004 - Obter Aluno por Id
        HttpRequestException : Response status code does not indicate success: 404 (Not Found).
        AlunoIntegrationTest.cs:line 69
      Failed 005 - Matricular Aluno
        HttpRequestException : Response status code does not indicate success: 400 (Bad Request).
        AlunoIntegrationTest.cs:line 102
      Failed 006 - Avançar progresso da matrícula
        HttpRequestException : Response status code does not indicate success: 400 (Bad Request).
        AlunoIntegrationTest.cs:line 130
      Failed 007 - Obter o Certificado
        HttpRequestException : Response status code does not indicate success: 400 (Bad Request).
        AlunoIntegrationTest.cs:line 161
    ```

    As 4 falhas de `EduOnline.Alunos.IntegrationTest` são **encadeadas e cobrem exatamente os fluxos de negócio centrais do escopo**: matrícula → progresso → certificado. O escopo determina que "todos os testes devem rodar e passar" e que "os fluxos de negócio funcionam integralmente". **Não atendido.**

  - **Cobertura de branches: 19,2% (325 de 1692)** — muito abaixo do critério de **≥ 80%**. Medido com `coverlet` + `reportgenerator` (`TestResults/Summary.txt`):
    ```
    Line coverage:   30.7% (1766 de 5747)
    Branch coverage: 19.2% (325 de 1692)
    Method coverage: 49.3% (340 de 689)
    ```
    Destaque negativo: `EduOnline.Alunos.ApiRest` com 13% e `AlunoController` com 28,3%.
  - **Provisionamento de banco quebrado no fluxo Docker.** O `docker-compose.yml` desativa migração e seed em todos os serviços (`SeedSettings__EnableMigrations: "false"` e `SeedSettings__EnableSeedData: "false"`, linhas 118-119, 159-160, 202-203, 247-248), delegando a criação do schema ao `infra/database/InitDB.sql` montado em `/docker-entrypoint-initdb.d` (linha 23). Dois defeitos:
    1. **A imagem oficial do SQL Server não executa scripts de `/docker-entrypoint-initdb.d`.** Esse mecanismo é do MySQL/PostgreSQL; `mcr.microsoft.com/mssql/server` o ignora silenciosamente. O script **nunca roda**.
    2. Mesmo se rodasse, ele cria apenas o banco **`EduOnlineDB`** (`InitDB.sql:12`), enquanto os serviços apontam para `EduOnlineAuthDb`, `EduOnlineConteudosDb`, `EduOnlineAlunosDb` e `EduOnlinePagamentosDb`. **Nenhum serviço usa o banco criado.**
    - Resultado: com migrações desligadas e o script inerte, os schemas não são criados — o ecossistema via compose não tem como funcionar de ponta a ponta. O `init-db.sh` presente em `infra/database/` não é referenciado por nada.
  - `src/EduOnline.WebApps.ApiRest/Dockerfile:~23` compila `EduOnline.Bff.ApiRest.csproj` — o nome do assembly diverge do nome da pasta, o que dificulta a rastreabilidade.
  - _(Tolerância — mencionado sem peso crítico)_: há resquícios de código comentado no `docker-compose.yml:212-215` (bloco `depends_on` de `alunos-api` comentado) e comentários de seção decorativos redundantes.

## Segurança

- **Pontos positivos:**
  - Autenticação JWT centralizada na Auth API, com as demais APIs validando o token via `AddJwtConfiguration()`/`UseAuthConfiguration()`.
  - **O apontamento do Módulo 4 sobre segredos JWT com fallback hardcoded foi resolvido**: a busca por `ChaveSuperSecreta` e por padrões de fallback (`?? "..."`) em `src/**/*.cs` **não retorna nenhuma ocorrência**. Os segredos agora vêm de configuração.
  - Imagens base oficiais Microsoft e atualizadas (.NET 10 Alpine).
  - `securityContext` exemplar nos Deployments (`runAsNonRoot`, `drop: [ALL]`, `seccompProfile`, `allowPrivilegeEscalation: false`, `automountServiceAccountToken: false`).
  - `NetworkPolicy` com abordagem `deny-all` + liberações explícitas (`infra/kubernetes/networkpolicies/`).
  - `.dockerignore` exclui corretamente `**/.env`, `**/*.pfx` e `appsettings.Development.json`, evitando vazamento para dentro das imagens.

- **Pontos negativos:**
  - `SECURITY-COMPLIANCE-MATRIX.md` e todo o aparato de Vault/PSP/TLS/audit descrevem uma postura de segurança que o repositório **não pratica**: O Vault não é consumido por nenhum Deployment. A documentação de segurança contradiz o código.

## Documentação

- **Pontos positivos:**
  - `README.md` (8,2 KB) bem estruturado em 10 seções, com apresentação, stack, funcionalidades, execução, configuração, documentação de API, troubleshooting e estrutura da solução.
  - **Endereça o apontamento do Módulo 4** sobre subida coordenada do ecossistema: agora há "Opção A — Docker Compose" e "Opção B — Kubernetes", com pré-requisitos explícitos (`kubectl`, `kind`/`minikube`) e tabela de portas.
  - Seção "B.3 Conferência pós-deploy (checklist do avaliador)" com comandos `kubectl` prontos — boa iniciativa.
  - Swagger/OpenAPI configurado nas APIs e documentado no README.
  - Documentação complementar em `infra/ARCHITECTURE.md`, `infra/kubernetes/README.md` e `DOCKER-README.md`.

- **Pontos negativos:**
  - **O README documenta um passo que não funciona.** `README.md:64-71` afirma que `docker compose build auth-api conteudos-api alunos-api pagamentos-api bff-api` gera as imagens `eduonline/auth-api:latest`, `eduonline/conteudos-api:latest`, etc. **Isso é falso**: o `docker-compose.yml` tagueia como `andrecesco/eduonline-auth-api:latest` (linha 105) e equivalentes. Seguir o README ao pé da letra produz imagens com nomes que os Deployments não referenciam — e o deploy falha com `ImagePullBackOff`. É exatamente o caso de documentação que **promete** o que o código não entrega.
  - `README.md:30` afirma "Health checks e endpoints de Swagger para todas as APIs" — mas os endpoints `/health/ready` e `/health/live` não existem, conforme teste falhando.
  - Não há documentação do requisito de seed/migração no contexto Docker, nem aviso de que o compose os desativa.
  - Excesso de documentos sobrepostos (`DOCKER-README.md`, `CI-CD-TESTING-GUIDE.md`, `SECURITY-COMPLIANCE-MATRIX.md`, `ProjetoMod05.md`, `infra/ARCHITECTURE.md`, `infra/kubernetes/README.md`, além de 8 guias em `infra/**/*-GUIDE.md`), vários descrevendo componentes não operantes.

## Conclusão

O projeto demonstra **conhecimento técnico real e amplo** de DevOps. Vários artefatos são de qualidade acima da média para o nível: os 6 Dockerfiles são multi-stage com ordenação correta de camadas para cache e base Alpine oficial; os Deployments trazem `securityContext` endurecido, probes, `resources` e HPA; o `docker-compose.yml` usa `condition: service_healthy`, limites de recursos e rotação de logs; e o uso de Git é **exemplar**, com 13 Pull Requests de branches temáticas mergeadas em `main` e workflow disparando em PR e push. Metade dos apontamentos do Módulo 4 foi resolvida, com destaque para a eliminação dos segredos JWT hardcoded e a reescrita do README.

Entretanto, há uma **lacuna decisiva entre o que está declarado e o que funciona**, e ela atinge os três pilares do escopo:

1. **O Kubernetes não sobe.** Os 5 Deployments referenciam `eduonline/<serviço>:latest`, imagem que não existe no Docker Hub (404 confirmado), não é produzida pelo `docker compose build` (que tagueia `andrecesco/eduonline-*`) e não é carregada no cluster por nenhum script (`setup-kind.ps1` não faz `kind load`). O resultado inevitável é `ImagePullBackOff`. É provavelmente o defeito de **menor esforço de correção e maior impacto** de todo o projeto: alinhar os nomes de imagem e adicionar o `kind load` destrava o critério de sucesso central do módulo.

2. **O pipeline não valida nem entrega.** O workflow faz apenas `dotnet build`. Não roda testes, não publica no Docker Hub e não executa lint/análise estática — três exigências explícitas do escopo. O `sonar-project.properties` está pronto na raiz mas nenhum workflow o invoca. O job `docker` sobe containers num runner efêmero e os descarta, sem verificar nem publicar nada. As imagens no Docker Hub existem, mas foram enviadas à mão.

3. **A qualidade não está sob rede de proteção.** `dotnet test` **falha** (exit 1): 5 testes quebrados, sendo 4 deles nos fluxos de negócio centrais (matrícula, progresso, certificado) e 1 provando que `/health/ready` retorna 404 porque só `/health` é mapeado. A cobertura de branches é de **19,2%**, contra os 80% exigidos. Como o CI não roda testes, essas falhas passam despercebidas e o pipeline permanece verde — os dois problemas se retroalimentam.

Há ainda um **desequilíbrio de prioridades** que merece reflexão. O repositório contém Vault, External Secrets, Elasticsearch, Kibana, Fluentd, Promtail, Jaeger, Prometheus, Alertmanager, cert-manager, PodSecurityPolicies (recurso removido do K8s desde a 1.25) e auditoria de API server — tudo muito além do escopo, duplicado em duas árvores paralelas (`infra/security/` vs `infra/kubernetes/security/`, `infra/observability/` vs `infra/kubernetes/observability/`), com metade órfã do kustomization e nada disso validado. Enquanto isso, os segredos reais estão em texto puro **commitados** em dois lugares (`.env` rastreado no Git e `secrets.yaml`), e o provisionamento de banco no compose está quebrado (migrações desativadas + `InitDB.sql` montado num caminho que a imagem do SQL Server ignora, criando um banco que nenhum serviço usa). Sofisticação declarativa não substitui o básico funcionando.

**Recomendações priorizadas:**

1. **Corrigir o nome das imagens**: unificar compose e Deployments (ex.: `andrecesco/eduonline-auth-api:latest` nos dois) e adicionar `kind load docker-image` ao `setup-kind.ps1`. Corrigir também o `README.md:64-71`.
2. **Adicionar `dotnet test` ao workflow** e fazer o build falhar com os testes. Em seguida, corrigir os 5 testes quebrados — implementar `/health/ready` e `/health/live` em `ObservabilityExtensions.cs` e investigar os 400/404 de `AlunoIntegrationTest`.
3. **Completar o pipeline**: `docker/login-action` + `docker/build-push-action` com push para o Docker Hub (tag por SHA além de `latest`), e um passo de análise estática consumindo o `sonar-project.properties` já existente. Encadear os jobs com `needs:` e remover o `docker compose up -d` inócuo.
4. **Separar liveness de readiness**: liveness deve checar apenas o processo; readiness checa dependências. Hoje ambos apontam para `/health` agregado, o que reinicia pods em falha transitória de banco — o oposto da resiliência exigida.
5. **Consertar o provisionamento de banco** no compose: reativar `EnableMigrations`/`EnableSeedData` ou executar o `InitDB.sql` por um passo real (job/entrypoint próprio), e alinhar os nomes de banco entre `InitDB.sql`, compose e `secrets.yaml`.
6. **Remover a duplicação de infraestrutura** e cortar os componentes fora do escopo (Vault, ELK, PSP, cert-manager). Menos superfície, tudo funcionando, vale mais do que amplitude declarativa não validada.
7. Eliminar o `dotnet build` redundante dos Dockerfiles (fica só `dotnet publish`) e adicionar `USER` não-root nas imagens.

O caminho para uma entrega forte é curto: as peças de qualidade já existem e estão bem escritas. O que falta é **fechar o laço entre o declarado e o executado** — nomes de imagem alinhados, testes rodando no CI e segredos fora do repositório.
