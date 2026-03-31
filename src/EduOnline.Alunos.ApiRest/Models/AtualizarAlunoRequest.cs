using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class AtualizarAlunoRequest
{
    public string Nome { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
}

[ExcludeFromCodeCoverage]
public class AdicionarAlunoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
