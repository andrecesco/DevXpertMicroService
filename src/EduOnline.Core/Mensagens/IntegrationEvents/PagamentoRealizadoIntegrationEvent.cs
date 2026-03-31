namespace EduOnline.Core.Mensagens.IntegrationEvents;

public class PagamentoRealizadoIntegrationEvent : IntegrationEvent
{
    public Guid CursoId { get; private set; }
    public Guid AlunoId { get; private set; }
    public Guid PagamentoId { get; private set; }
    public Guid TransacaoId { get; private set; }
    public decimal Total { get; private set; }

    public PagamentoRealizadoIntegrationEvent(Guid aggregateId, Guid cursoId, Guid alunoId, Guid pagamentoId, Guid transacaoId, decimal total)
    {
        AggregateId = aggregateId;
        CursoId = cursoId;
        AlunoId = alunoId;
        PagamentoId = pagamentoId;
        TransacaoId = transacaoId;
        Total = total;
    }
}
