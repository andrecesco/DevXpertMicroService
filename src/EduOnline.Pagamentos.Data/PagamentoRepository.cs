using EduOnline.Core.Data;
using EduOnline.Pagamentos.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EduOnline.Pagamentos.Data;

public class PagamentoRepository(PagamentosContext context) : IPagamentoRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Pagamento?> ObterPorIdAsync(Guid id)
    {
        return await context.Pagamentos
            .Include(a => a.Transacao)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Pagamento>> ObterTodosAsync()
    {
        return await context.Pagamentos
            .Include(a => a.Transacao)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Pagamento>> BuscarAsync(Expression<Func<Pagamento, bool>> predicate)
    {
        return await context.Pagamentos
            .Include(a => a.Transacao)
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Transacao?> ObterTransacaoPorId(Guid id)
    {
        return await context.Transacoes
            .Include(t => t.Pagamento)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public void Adicionar(Pagamento pagamento)
    {
        context.Pagamentos.Add(pagamento);
    }

    public void AdicionarTransacao(Transacao transacao)
    {
        context.Transacoes.Add(transacao);
    }

    public void Atualizar(Pagamento pagamento)
    {
        context.Pagamentos.Update(pagamento);
    }

    public void AtualizarTransacao(Transacao transacao)
    {
        context.Transacoes.Update(transacao);
    }

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}
