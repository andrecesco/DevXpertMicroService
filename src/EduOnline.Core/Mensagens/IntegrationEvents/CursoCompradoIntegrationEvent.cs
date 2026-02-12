namespace EduOnline.Core.Mensagens.IntegrationEvents;

public class CursoCompradoIntegrationEvent : IntegrationEvent
{
    public Guid CursoId { get; private set; }
    public Guid AlunoId { get; private set; }
    public decimal Total { get; private set; }
    public string NomeCartao { get; private set; }
    public string NumeroCartao { get; private set; }
    public string ExpiracaoCartao { get; private set; }
    public string CvvCartao { get; private set; }
    public CursoCompradoIntegrationEvent(Guid matriculaId, Guid cursoId, Guid alunoId, decimal total, string nomeCartao, string numeroCartao, string expiracaoCartao, string cvvCartao)
    {
        AggregateId = matriculaId;
        CursoId = cursoId;
        AlunoId = alunoId;
        Total = total;
        NomeCartao = nomeCartao;
        NumeroCartao = numeroCartao;
        ExpiracaoCartao = expiracaoCartao;
        CvvCartao = cvvCartao;
    }
}
