using EduOnline.Conteudos.Domain;

namespace EduOnline.Conteudos.Data.Context;

public static class ConteudoSeedHelper
{
    public static async Task SeedAsync(this ConteudosContext context)
    {
        //Realiza a carga inicial dos dados
        if (context.Cursos.Any())
            return;

        var aluno = new Curso
        {
            Nome = "Curso Inicial de C#",
            Autor = "EduOnline",
            Validade = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
            Ativo = true,
            Valor = 199.99m,
            ConteudoProgramatico = new ConteudoProgramatico
            {
                Tema = "Um Breve Introdução ao C#",
                NivelId = 1,
                CargaHoraria = 40
            },
            Aulas =
            [
                new() {
                    Titulo = "Introdução ao C#",
                    Descricao = "Visão geral da linguagem C# e suas aplicações.",
                    LinkMaterial = "http://material.eduonline.com/introducao-csharp",
                    DuracaoEmMinutos = 30
                },
                new Aula
                {
                    Titulo = "Tipos de Dados e Variáveis",
                    Descricao = "Aprenda sobre os tipos de dados e como declarar variáveis em C#.",
                    LinkMaterial = "http://material.eduonline.com/tipos-dados-variaveis",
                    DuracaoEmMinutos = 45
                },
                new Aula
                {
                    Titulo = "Estruturas de Controle",
                    Descricao = "Entenda as estruturas de controle como if, switch, loops e mais.",
                    LinkMaterial = "http://material.eduonline.com/estruturas-controle",
                    DuracaoEmMinutos = 50
                }
            ]
        };

        context.Cursos.Add(aluno);

        await context.SaveChangesAsync();
    }
}
