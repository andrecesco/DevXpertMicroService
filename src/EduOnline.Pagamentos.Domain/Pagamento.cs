using EduOnline.Core.DomainObjects;

namespace EduOnline.Pagamentos.Domain;

public class Pagamento : Entity, IAggregateRoot
{
    public Guid AlunoId { get; set; }
    public Guid CursoId { get; set; }
    public decimal Total { get; set; }
    public string? NomeCartao { get; set; }
    public string? NumeroCartao { get; set; }
    public string? ExpiracaoCartao { get; set; }
    public string? CvvCartao { get; set; }

    //EF Relations
    public Transacao? Transacao { get; set; }
}
