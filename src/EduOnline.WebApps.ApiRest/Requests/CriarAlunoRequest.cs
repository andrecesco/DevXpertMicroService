using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Bff.ApiRest.Requests;

[ExcludeFromCodeCoverage]
public class CriarAlunoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
