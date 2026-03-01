namespace EduOnline.Alunos.ApiRest.Models;

public class AtualizarAlunoRequest
{
    public string Nome { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
}
