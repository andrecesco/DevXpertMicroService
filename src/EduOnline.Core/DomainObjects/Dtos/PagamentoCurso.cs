using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Core.DomainObjects.Dtos;

[ExcludeFromCodeCoverage]
public class PagamentoCurso
{
    public Guid MatriculaId { get; set; }
    public Guid CursoId { get; set; }
    public Guid AlunoId { get; set; }
    public decimal Total { get; set; }
    public string? NomeCartao { get; set; }
    public string? NumeroCartao { get; set; }
    public string? ExpiracaoCartao { get; set; }
    public string? CvvCartao { get; set; }
}
