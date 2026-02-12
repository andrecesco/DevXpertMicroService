using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.Domain.Models;

public class Aluno : Entity, IAggregateRoot
{
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateOnly? DataNascimento { get; private set; }
    public bool Ativo { get; private set; } = true;

    //EF Relations
    public List<Matricula> Matriculas { get; private set; } = [];

    private Aluno() { }

    public Aluno(Guid id, string nome, string email, DateOnly? dataNascimento)
    {
        Id = id;
        Nome = nome;
        Email = email;
        DataNascimento = dataNascimento;
        Ativo = true;
        Validar();
    }

    public void AlterarNome(string nome)
    {
        Nome = nome;
        Validar();
    }

    public void AlterarDataNascimento(DateOnly dataNascimento)
    {
        Validacoes.ValidarSeMaiorQue(dataNascimento, DateOnly.FromDateTime(DateTime.UtcNow), "O campo Data de Nascimento deve ser menor do que a data atual");
        DataNascimento = dataNascimento;
    }

    public void AdicionarMatricula(Matricula matricula)
    {
        Matriculas.Add(matricula);
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    void Validar()
    {
        Validacoes.ValidarSeIgual(Id, Guid.Empty, "O campo Id deve ser preenchido");
        Validacoes.ValidarSeVazio(Nome, "O campo Nome deve ser preenchido");
        Validacoes.ValidarTamanho(Nome, 100, "O campo Nome deve ter no máximo 100 caracteres");
        Validacoes.ValidarSeVazio(Email, "O campo Email deve ser preenchido");
        Validacoes.ValidarTamanho(Email, 100, "O campo Email deve ter no máximo 100 caracteres");
    }
}
