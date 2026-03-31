using EduOnline.Core.DomainObjects;

namespace EduOnline.Pagamentos.Domain;

public class Transacao : Entity
{
    public Guid PagamentoId { get; set; }
    public decimal Total { get; set; }
    public int? StatusTransacaoId { get; set; }

    //EF Relations
    public Pagamento? Pagamento { get; set; }
}
