using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.Application.Queries.Dtos;

[ExcludeFromCodeCoverage]
public class HistoricoAprendizagemDto
{
    public int TotalAulas { get; set; }
    public Guid[] AulasConcluidas { get; set; } = [];
}
