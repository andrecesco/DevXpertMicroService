using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class AdicionarMatriculaRequest
{
    public Guid CursoId { get; set; }
    public string CursoNome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int TotalAulas { get; set; }
    public string NomeCartao { get; set; } = string.Empty;
    public string NumeroCartao { get; set; } = string.Empty;
    public string ExpiracaoCartao { get; set; } = string.Empty;
    public string CvvCartao { get; set; } = string.Empty;
}
