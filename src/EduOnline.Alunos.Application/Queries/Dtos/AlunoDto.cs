namespace EduOnline.Alunos.Application.Queries.Dtos;

public class AlunoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}
