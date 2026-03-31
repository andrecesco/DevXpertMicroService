using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.Application.Queries.Dtos;

[ExcludeFromCodeCoverage]
public class CertificadoDto
{
    public Guid Id { get; set; }
    public Guid MatriculaId { get; set; }
    public string Link { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
