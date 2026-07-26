using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class AtualizarAlunoRequest
{
    public string Nome { get; set; } = string.Empty;
    public DateOnly? DataNascimento { get; set; }
}
