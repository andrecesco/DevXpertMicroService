# Feedback – Avaliação Geral

## Organização do Projeto

- **Pontos positivos:**
  - Estrutura geral coerente para microsserviços, com separação entre APIs, domínios, dados, aplicação, BFF e testes em `src/` e `test/`.
  - Arquivo de solução presente na raiz em `EduOnline.slnx`.
  - Os bounded contexts principais aparecem fisicamente isolados em projetos próprios: `EduOnline.Auth.ApiRest`, `EduOnline.Conteudos.ApiRest`, `EduOnline.Alunos.ApiRest`, `EduOnline.Pagamentos.ApiRest` e `EduOnline.WebApps.ApiRest`.
- **Pontos negativos:**
  - Há arquivos acessórios na raiz sem relação direta com a entrega principal, como `fact_aaa.snippet`, `test-output.txt` e `test-output-full.txt`, o que reduz a limpeza do repositório.
  - O arquivo `README.md` não reflete com precisão a topologia completa do ecossistema distribuído, o que afeta a percepção da organização global da solução.

## Arquitetura de Microsserviços

- **Pontos positivos:**
  - Cada contexto possui seu próprio `DbContext` e sua própria configuração de conexão: `ApplicationDbContext` em `src/EduOnline.Auth.ApiRest/Data/ApplicationDbContext.cs`, `AlunosContext` em `src/EduOnline.Alunos.Data/Context/AlunosContext.cs`, `ConteudosContext` em `src/EduOnline.Conteudos.Data/Context/ConteudosContext.cs` e `PagamentosContext` em `src/EduOnline.Pagamentos.Data/PagamentosContext.cs`.
  - As strings de conexão estão separadas por serviço em `appsettings.json`, com bancos distintos para Auth, Alunos, Conteúdos e Pagamentos.
  - A integração entre Alunos e Pagamentos segue desacoplamento assíncrono por eventos com RabbitMQ, em vez de acesso direto ao banco de outro serviço.
  - A presença do BFF em `src/EduOnline.WebApps.ApiRest` evita que o front-end precise coordenar parte das chamadas mais sensíveis.
- **Pontos negativos:**
  - O endpoint anônimo de cadastro de aluno em `src/EduOnline.Alunos.ApiRest/Controllers/AlunoController.cs:185-188` expõe a criação do registro de aluno fora da Auth API, o que enfraquece a centralização do fluxo de cadastro exigida pelo escopo.

## Bounded Contexts e APIs

- **Pontos positivos:**
  - As APIs esperadas pelo escopo estão presentes: Auth, Conteúdos, Alunos, Pagamentos e BFF.
  - As responsabilidades principais estão, em geral, bem distribuídas: autenticação em Auth, cursos/aulas em Conteúdos, matrículas/progresso/certificados em Alunos e processamento financeiro em Pagamentos.
  - O BFF orquestra matrícula consultando Conteúdos antes de enviar o comando para Alunos em `src/EduOnline.WebApps.ApiRest/Controllers/AlunoController.cs`.
- **Pontos negativos:**
  - O fluxo de progresso não evidencia integração HTTP entre Conteúdos e Alunos conforme o caso de uso do escopo; o progresso é atualizado diretamente pela API de Alunos via `src/EduOnline.Alunos.ApiRest/Controllers/AlunoController.cs` sem validação do consumo da aula na API de Conteúdos.

## Fluxos de Negócio e Integração

- **Pontos positivos:**
  - **Login e emissão de JWT** implementados em `src/EduOnline.Auth.ApiRest/Controllers/AuthController.cs` e `src/EduOnline.Auth.ApiRest/Services/AuthenticationService.cs`.
  - **Cadastro e manutenção de cursos/aulas** implementados em `src/EduOnline.Conteudos.ApiRest/Controllers/CursosController.cs`.
  - **Matrícula e pagamento** seguem fluxo distribuído: Alunos publica `CursoCompradoIntegrationEvent` em `src/EduOnline.Alunos.Application/Commands/AlunoCommandHandler.cs:76`, Pagamentos consome em `src/EduOnline.Pagamentos.ApiRest/BackgroundServices/CursoCompradoConsumerHostedService.cs:19`, e Pagamentos publica `PagamentoRealizadoIntegrationEvent` / `PagamentoRecusadoIntegrationEvent` em `src/EduOnline.Pagamentos.Domain/PagamentoService.cs:36-44`, consumidos por Alunos em `src/EduOnline.Alunos.ApiRest/BackgroundServices/PagamentoIntegrationEventsConsumerHostedService.cs:19-20`.
  - **Finalização de curso e certificado** estão presentes em `src/EduOnline.Alunos.ApiRest/Controllers/AlunoController.cs`, `src/EduOnline.Alunos.Application/Commands/AlunoCommandHandler.cs` e `src/EduOnline.Alunos.Application/Events/MatriculaEventHandler.cs`.
- **Pontos negativos:**
  - O fluxo de **realização de aula** está apenas parcialmente aderente ao escopo: existe atualização de progresso por aula, mas não encontrei evidência de chamada HTTP da API de Conteúdos para a API de Alunos ao consumir a aula.
  - A execução do fluxo de pagamento não se mostrou resiliente em ambiente sem EventStore; `dotnet test EduOnline.slnx --no-build` falhou em 1 teste de integração de Pagamentos por indisponibilidade de `localhost:2113`.
  - Três projetos de teste reportaram “No test is available” no comando de teste (`EduOnline.Alunos.IntegrationTest`, `EduOnline.Conteudos.UnitTest` e `EduOnline.Conteudos.IntegrationTest`), o que reduz a confiança prática na cobertura automatizada desses fluxos.

## Autenticação e Autorização

- **Pontos positivos:**
  - A emissão de JWT está centralizada na Auth API, com geração de claims e roles em `src/EduOnline.Auth.ApiRest/Services/AuthenticationService.cs`.
  - As demais APIs validam JWT via `AddJwtConfiguration()` e aplicam `UseAuthConfiguration()` em seus `Program.cs`.
  - Há uso consistente de `[Authorize]`, `[Authorize(Roles = ...)]` e verificação do usuário autenticado com `IAspNetUser` em Auth, Alunos, Conteúdos, Pagamentos e BFF.

## Resiliência e Comunicação

- **Pontos positivos:**
  - O projeto usa RabbitMQ para publicação e consumo de eventos de integração em `src/EduOnline.Core/Mensagens/RabbitMq/RabbitMqEventBus.cs`.
  - O BFF aplica retry e circuit breaker com Polly em `src/EduOnline.Core.Api/Extensions/PollyExtensions.cs:12`, `:23` e `:38`.
  - Há health checks e validação de RabbitMQ nas APIs que usam mensageria.
- **Pontos negativos:**
  - As políticas de retry/circuit breaker ficaram concentradas nos `HttpClient` do BFF; não encontrei o mesmo nível de proteção nas integrações internas críticas além da mensageria.
  - O `RabbitMqEventBus` captura exceções de publicação/assinatura e apenas registra warning/erro, sem evidência de reprocessamento ou compensação.
  - O fluxo crítico de pagamento continua vulnerável à indisponibilidade do EventStore, conforme o teste de integração falho.

## Execução Local e Infraestrutura

- **Pontos positivos:**
  - Há migração automática no startup em Auth, Alunos, Conteúdos e Pagamentos via `UseDbMigrationHelper()` nos `Program.cs`.
  - Há seed automática em Auth, Alunos e Conteúdos com `EnableSeedData` e helpers dedicados.
  - As APIs expõem Swagger/OpenAPI no ambiente de desenvolvimento.
  - As configurações oferecem opção de SQL Server e SQLite por serviço.

## Documentação

- **Pontos positivos:**
  - Existe `README.md` na raiz e Swagger nas APIs em desenvolvimento.
  - O README ao menos registra a existência de JWT, Swagger e EventStore.
- **Pontos negativos:**
  - O `README.md` está incompleto para o escopo de microsserviços: não orienta subida coordenada de Auth, Alunos, Conteúdos, Pagamentos e BFF.
  - O README instrui basicamente a executar `src/EduOnline.WebApps.ApiRest/`, o que não basta para reproduzir o ecossistema distribuído.
  - A documentação não descreve RabbitMQ local, portas dos serviços, ordem de inicialização, dependências reais dos testes nem o comportamento de seed por serviço.

## Conclusão

O projeto demonstra boa evolução para um modelo distribuído e atende a parte mais importante do escopo estrutural: os bounded contexts principais estão separados, o BFF existe, a Auth API centraliza login e emissão de JWT, e o fluxo de matrícula/pagamento foi desenhado com mensageria entre Alunos e Pagamentos. Também há seed e migração automática em boa parte dos serviços, além de Swagger e cobertura de testes razoável para vários módulos.

Os principais riscos técnicos remanescentes estão na aderência fina ao escopo e na robustez operacional. O fluxo de progresso não evidencia a integração HTTP entre Conteúdos e Alunos descrita no enunciado, a validação JWT usa segredos fallback hardcoded, a API de Alunos expõe um endpoint anônimo de provisionamento, e o fluxo de pagamentos ainda quebra quando o EventStore não está disponível. A documentação também não acompanha a complexidade real do ecossistema distribuído.

## 📊 Matriz de Avaliação

| **Critério**                | **Peso** | **Descrição**                                                             | **Nota** |
| --------------------------- | -------- | ------------------------------------------------------------------------- | -------- |
| **Funcionalidade**          | 30%      | Atendimento aos requisitos funcionais e fluxos do domínio.                | 8        |
| **Qualidade do Código**     | 30%      | Clareza, organização, aderência ao escopo e qualidade estrutural.         | 8        |
| **Eficiência e Desempenho** | 10%      | Eficiência das soluções e ausência de gargalos evidentes.                 | 6        |
| **Inovação e Diferenciais** | 10%      | Soluções bem aplicadas, boas decisões técnicas e diferenciais relevantes. | 9        |
| **Documentação**            | 10%      | Qualidade e completude da documentação.                                   | 6        |
| **Resolução de Feedbacks**  | 10%      | Capacidade de responder feedbacks anteriores.                             | 10       |

🎯 Nota Final: 7.9 / 10
