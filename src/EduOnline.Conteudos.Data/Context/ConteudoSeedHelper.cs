using EduOnline.Conteudos.Domain;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.Data.Context;

[ExcludeFromCodeCoverage]
public static class ConteudoSeedHelper
{
    public static async Task SeedAsync(this ConteudosContext context)
    {
        if (context.Cursos.Any())
            return;

        var cursos = new List<Curso>
        {
            new()
            {
                Nome = "Curso Inicial de C#",
                Autor = "EduOnline",
                Validade = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                Ativo = true,
                Valor = 199.99m,
                ConteudoProgramatico = new ConteudoProgramatico
                {
                    Tema = "Fundamentos da linguagem C#",
                    NivelId = 1,
                    CargaHoraria = 40
                },
                Aulas =
                [
                    new()
                    {
                        Titulo = "Introdução ao C#",
                        Descricao = "Visão geral da linguagem, ecossistema .NET e ambiente de desenvolvimento.",
                        LinkMaterial = "https://material.eduonline.com/csharp/introducao",
                        DuracaoEmMinutos = 30
                    },
                    new()
                    {
                        Titulo = "Tipos de Dados e Variáveis",
                        Descricao = "Declaração de variáveis, tipos primitivos e conversões básicas.",
                        LinkMaterial = "https://material.eduonline.com/csharp/tipos-variaveis",
                        DuracaoEmMinutos = 45
                    },
                    new()
                    {
                        Titulo = "Estruturas de Controle",
                        Descricao = "Uso de if, switch, for, while e foreach em cenários práticos.",
                        LinkMaterial = "https://material.eduonline.com/csharp/estruturas-controle",
                        DuracaoEmMinutos = 50
                    }
                ]
            },
            new()
            {
                Nome = "ASP.NET Core Web API na Prática",
                Autor = "EduOnline",
                Validade = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                Ativo = true,
                Valor = 349.90m,
                ConteudoProgramatico = new ConteudoProgramatico
                {
                    Tema = "Construção de APIs REST com ASP.NET Core",
                    NivelId = 2,
                    CargaHoraria = 60
                },
                Aulas =
                [
                    new()
                    {
                        Titulo = "Arquitetura de uma Web API",
                        Descricao = "Organização por camadas, controllers, contratos e boas práticas.",
                        LinkMaterial = "https://material.eduonline.com/aspnet/api-arquitetura",
                        DuracaoEmMinutos = 55
                    },
                    new()
                    {
                        Titulo = "Versionamento e Documentação com OpenAPI",
                        Descricao = "Estratégias de versionamento e documentação efetiva para consumidores da API.",
                        LinkMaterial = "https://material.eduonline.com/aspnet/openapi-versionamento",
                        DuracaoEmMinutos = 40
                    },
                    new()
                    {
                        Titulo = "Autenticação e Autorização com JWT",
                        Descricao = "Proteção de endpoints, claims e políticas de autorização.",
                        LinkMaterial = "https://material.eduonline.com/aspnet/jwt-autorizacao",
                        DuracaoEmMinutos = 65
                    },
                    new()
                    {
                        Titulo = "Tratamento Global de Erros e Logs",
                        Descricao = "Padrões de resposta de erro e observabilidade em APIs de produção.",
                        LinkMaterial = "https://material.eduonline.com/aspnet/erros-logs",
                        DuracaoEmMinutos = 45
                    }
                ]
            },
            new()
            {
                Nome = "Microsserviços com .NET",
                Autor = "EduOnline",
                Validade = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                Ativo = true,
                Valor = 499.00m,
                ConteudoProgramatico = new ConteudoProgramatico
                {
                    Tema = "Arquitetura distribuída e comunicação entre serviços",
                    NivelId = 3,
                    CargaHoraria = 80
                },
                Aulas =
                [
                    new()
                    {
                        Titulo = "Bounded Context e desenho de domínio",
                        Descricao = "Separação de responsabilidades e modelagem estratégica com DDD.",
                        LinkMaterial = "https://material.eduonline.com/microservices/bounded-context",
                        DuracaoEmMinutos = 60
                    },
                    new()
                    {
                        Titulo = "Comunicação síncrona e resiliente",
                        Descricao = "HTTP entre serviços, timeout, retry e circuit breaker.",
                        LinkMaterial = "https://material.eduonline.com/microservices/comunicacao-sincrona",
                        DuracaoEmMinutos = 55
                    },
                    new()
                    {
                        Titulo = "Mensageria com RabbitMQ",
                        Descricao = "Eventos de integração, filas, exchange e processamento assíncrono.",
                        LinkMaterial = "https://material.eduonline.com/microservices/rabbitmq",
                        DuracaoEmMinutos = 70
                    },
                    new()
                    {
                        Titulo = "Observabilidade e rastreamento distribuído",
                        Descricao = "Correlation ID, logs e diagnóstico de fluxo entre APIs.",
                        LinkMaterial = "https://material.eduonline.com/microservices/observabilidade",
                        DuracaoEmMinutos = 50
                    }
                ]
            },
            new()
            {
                Nome = "Banco de Dados para APIs .NET",
                Autor = "EduOnline",
                Validade = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                Ativo = true,
                Valor = 279.00m,
                ConteudoProgramatico = new ConteudoProgramatico
                {
                    Tema = "Persistência relacional com Entity Framework Core",
                    NivelId = 2,
                    CargaHoraria = 50
                },
                Aulas =
                [
                    new()
                    {
                        Titulo = "Modelagem de entidades e relacionamentos",
                        Descricao = "Entidades, value objects, agregados e mapeamentos.",
                        LinkMaterial = "https://material.eduonline.com/dados/modelagem-ef",
                        DuracaoEmMinutos = 45
                    },
                    new()
                    {
                        Titulo = "Migrações e versionamento de schema",
                        Descricao = "Criação, evolução e manutenção de migrações com EF Core.",
                        LinkMaterial = "https://material.eduonline.com/dados/migracoes-ef",
                        DuracaoEmMinutos = 40
                    },
                    new()
                    {
                        Titulo = "Estratégias de seed para ambientes locais",
                        Descricao = "Carga inicial de dados para desenvolvimento e testes.",
                        LinkMaterial = "https://material.eduonline.com/dados/seed-local",
                        DuracaoEmMinutos = 35
                    }
                ]
            }
        };

        context.Cursos.AddRange(cursos);

        await context.SaveChangesAsync();
    }
}
