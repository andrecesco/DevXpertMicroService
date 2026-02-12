# **EduOnline - Plataforma de Educação onlines**

## **1. Apresentação**

Bem-vindo ao repositório do projeto **EduOnline**. Este projeto é uma entrega do MBA DevXpert Full Stack .NET e é referente ao módulo **Arquitetura, Modelagem e Qualidade de Software**.
Desenvolver uma plataforma educacional online com múltiplos bounded contexts (BC), aplicando DDD, TDD, CQRS e padrões arquiteturais para gestão eficiente de conteúdos educacionais, alunos e processos financeiros.

## **2. Tecnologias Utilizadas**

- **Linguagem de Programação:** C#
- **Frameworks:**
  - ASP.NET Core Web API
- **Banco de Dados:** SQL Server
- **Autenticação e Autorização:**
  - ASP.NET Core Identity
  - JWT (JSON Web Token) para autenticação na API
- **Documentação da API:** Swagger

## **3. Funcionalidades Implementadas**

- Cadastro de aulas e cursos pelo perfil de administradores 
- Cadastro de alunos na plataforma
- Matrículas em aulas com pagamento através do cartão de crédito
- Progresso das aulas
- Emissão de certificado ao final do progresso de todas as aulas do curso
- Todas as informações trafegadas pelo CQRS do contexto de Alunos é registrada no Event Store

## **4. Como Executar o Projeto**

### **Pré-requisitos**

- .NET SDK 9.0 ou superior
- SQL Server
- Visual Studio 2022 ou superior (ou qualquer IDE de sua preferência)
- Git

### **Passos para Execução**

1. **Clone o Repositório:**

2. **Configuração do Banco de Dados:**

4. **Executar a API:**
   - `cd src/EduOnline.WebApps.ApiRest/`
   - `dotnet run`
   - Acesse a documentação da API em: https://localhost:7098/swagger

## **5. Instruções de Configuração**

- **JWT para API:** As chaves de configuração do JWT estão no `appsettings.json`.
- **Migrações do Banco de Dados:** As migrações são gerenciadas pelo Entity Framework Core. Não é necessário aplicar devido a configuração do Seed de dados.

## **6. Documentação da API**

A documentação da API está disponível através do Swagger. Após iniciar a API, acesse a documentação em:

https://localhost:7098/swagger

## **7. Docker do KurrentDb para event sourcing**
Basta rodar os comandos do docker abaixo, isso é importante para o correto funcionamento da aplicação.

`docker pull docker.kurrent.io/kurrent-latest/kurrentdb:latest`

`docker run --name kurrentdb-node -it -p 2113:2113 \
    docker.kurrent.io/kurrent-latest/kurrentdb:latest --insecure --run-projections=All \
    --enable-atom-pub-over-http`

Após o container estiver rodando é possível acessar através do link http://localhost:2113

## **8. Avaliação**

- Este projeto é parte de um curso acadêmico e não aceita contribuições externas. 
- Para feedbacks ou dúvidas utilize o recurso de Issues
- O arquivo `FEEDBACK.md` é um resumo das avaliações do instrutor e deverá ser modificado apenas por ele.
