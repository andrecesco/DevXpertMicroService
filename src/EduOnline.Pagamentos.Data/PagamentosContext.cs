using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Data;
using EduOnline.Pagamentos.Domain;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Pagamentos.Data;

public class PagamentosContext : EduOnlineContext
{
    public PagamentosContext() { }

    public PagamentosContext(DbContextOptions options)
        : base(options) { }

    public PagamentosContext(DbContextOptions options, IMediatorHandler mediatorHandler) : base(options, mediatorHandler)
    {
    }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }
}
