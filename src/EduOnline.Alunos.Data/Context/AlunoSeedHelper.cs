using EduOnline.Alunos.Domain.Models;

namespace EduOnline.Alunos.Data.Context;

public static class AlunoSeedHelper
{
    public static async Task SeedAsync(this AlunosContext context, Guid userId)
    {
        //Realiza a carga inicial dos dados
        if (context.Alunos.Any())
            return;

        var aluno = new Aluno(userId, "Aluno Teste", "aluno@aluno.com", null);

        context.Alunos.Add(aluno);

        await context.SaveChangesAsync();
    }
}
