using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.Domain.Models;

public class Certificado : Entity
{
    public Guid MatriculaId { get; private set; }
    public string Link { get; private set; } = string.Empty;

    //EF Relations
    public Matricula? Matricula { get; private set; }

    private Certificado() { }
    public Certificado(Guid id, Guid matriculaId, string link)
    {
        Id = id;
        MatriculaId = matriculaId;
        Link = link;
        Validar();
    }

    void Validar()
    {
        Validacoes.ValidarSeIgual(Id, Guid.Empty, "O campo Id deve ser preenchido");
        Validacoes.ValidarSeVazio(Link, "O campo Link deve ser preenchido");
        Validacoes.ValidarTamanho(Link, 300, "O campo Link deve ter no máximo 300 caracteres");
    }
}
