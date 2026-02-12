namespace EduOnline.Alunos.Application.Queries.Dtos;

public class HistoricoAprendizagemDto
{
    public int TotalAulas { get; set; }
    public Guid[] AulasConcluidas { get; set; } = [];
}
