using EduOnline.Core.DomainObjects;

namespace EduOnline.Conteudos.Domain;

public class Curso : Entity, IAggregateRoot
{
    public string? Nome { get; set; }
    public string? Autor { get; set; }
    public DateOnly Validade { get; set; }
    public bool Ativo { get; set; } = true;
    public decimal Valor { get; set; }
    public ConteudoProgramatico ConteudoProgramatico { get; set; } = new ConteudoProgramatico();

    //EF Relations
    public List<Aula>? Aulas { get; set; } = [];

    public Curso() { }
}
