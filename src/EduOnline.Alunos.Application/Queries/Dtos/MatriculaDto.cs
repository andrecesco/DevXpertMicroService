namespace EduOnline.Alunos.Application.Queries.Dtos;

public class MatriculaDto
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public Guid CursoId { get; set; }
    public string CursoNome { get; set; } = string.Empty;
    public DateOnly Validade { get; set; }
    public int StatusId { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public int PagamentoStatusId { get; set; }
    public string PagamentoStatusNome { get; set; } = string.Empty;
    public HistoricoAprendizagemDto? HistoricoAprendizagem { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}
