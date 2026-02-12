namespace EduOnline.Alunos.Application.Queries.Dtos;

public class CertificadoDto
{
    public Guid Id { get; set; }
    public Guid MatriculaId { get; set; }
    public string Link { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
