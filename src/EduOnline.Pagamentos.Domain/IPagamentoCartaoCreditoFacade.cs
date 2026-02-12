namespace EduOnline.Pagamentos.Domain;

public interface IPagamentoCartaoCreditoFacade
{
    Transacao RealizarPagamento(Curso curso, Pagamento pagamento);
}
