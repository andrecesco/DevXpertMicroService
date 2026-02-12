namespace EduOnline.Core.Mensagens.IntegrationEvents;

public class PagamentoRecusadoEvent : IntegrationEvent
{
    public Guid CursoId { get; private set; }
    public Guid AlunoId { get; private set; }
    public Guid PagamentoId { get; private set; }
    public Guid TransacaoId { get; private set; }
    public decimal Total { get; private set; }

    public PagamentoRecusadoEvent(Guid matriculaId, Guid cursoId, Guid alunoId, Guid pagamentoId, Guid transacaoId, decimal total)
    {
        AggregateId = matriculaId;
        CursoId = cursoId;
        AlunoId = alunoId;
        PagamentoId = pagamentoId;
        TransacaoId = transacaoId;
        Total = total;
    }
}
