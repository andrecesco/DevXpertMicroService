using EduOnline.Core.DomainObjects;

namespace EduOnline.Conteudos.Domain;

public class Aula : Entity
{
    public Guid CursoId { get; set; }
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? LinkMaterial { get; set; }
    public int DuracaoEmMinutos { get; set; }

    //EF Relations
    public Curso? Curso { get; set; }

    public Aula() { }
}
