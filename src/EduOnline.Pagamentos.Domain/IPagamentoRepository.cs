using EduOnline.Core.Data;
using System.Linq.Expressions;

namespace EduOnline.Pagamentos.Domain;

public interface IPagamentoRepository : IRepository<Pagamento>
{
    Task<Pagamento?> ObterPorIdAsync(Guid id);
    Task<List<Pagamento>> ObterTodosAsync();
    Task<List<Pagamento>> BuscarAsync(Expression<Func<Pagamento, bool>> predicate);
    Task<Transacao?> ObterTransacaoPorId(Guid id);
    void Adicionar(Pagamento pagamento);
    void Atualizar(Pagamento pagamento);
    void AdicionarTransacao(Transacao transacao);
    void AtualizarTransacao(Transacao transacao);
}
