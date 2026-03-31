using EduOnline.Pagamentos.Domain;

namespace EduOnline.Pagamentos.AntiCorruption;

public class PagamentoCartaoCreditoFacade(IPayPalGateway payPalGateway, IConfigurationManager configManager) : IPagamentoCartaoCreditoFacade
{
    private readonly IPayPalGateway _payPalGateway = payPalGateway;
    private readonly IConfigurationManager _configManager = configManager;

    public Transacao RealizarPagamento(Curso curso, Pagamento pagamento)
    {
        var apiKey = _configManager.GetValue("apiKey");
        var encriptionKey = _configManager.GetValue("encriptionKey");

        var serviceKey = _payPalGateway.GetPayPalServiceKey(apiKey, encriptionKey);
        var cardHashKey = _payPalGateway.GetCardHashKey(serviceKey, pagamento.NumeroCartao ?? string.Empty);

        var pagamentoResult = _payPalGateway.CommitTransaction(cardHashKey, curso.Id.ToString(), pagamento.Total);

        var transacao = new Transacao
        {
            Total = curso.Valor,
            PagamentoId = pagamento.Id
        };

        if (pagamentoResult)
        {
            transacao.StatusTransacaoId = StatusTransacao.Aprovado.Id;
            return transacao;
        }

        transacao.StatusTransacaoId = StatusTransacao.Recusado.Id;
        return transacao;
    }
}
