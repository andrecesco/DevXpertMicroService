using EduOnline.Alunos.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.Data.Context;

[ExcludeFromCodeCoverage]
public static class AlunoSeedHelper
{
    public static async Task SeedAsync(this AlunosContext context, Guid userId, string nomePadrao, string emailPadrao)
    {
        //Realiza a carga inicial dos dados
        if (context.Alunos.Any())
            return;

        var aluno = new Aluno(userId, nomePadrao, emailPadrao, null);

        context.Alunos.Add(aluno);

        await context.SaveChangesAsync();
    }
}
