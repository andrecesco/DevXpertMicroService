using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.DomainObjects;

namespace EduOnline.Pagamentos.Data;

public static class MediatorExtension
{
    public static async Task PublicarEventos(this IMediatorHandler mediator, PagamentosContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.Notificacoes != null && x.Entity.Notificacoes.Any());

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.Notificacoes)
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.LimparEventos());

        var tasks = domainEvents
            .Select(async (domainEvent) => {
                await mediator.PublicarEvento(domainEvent);
            });

        await Task.WhenAll(tasks);
    }
}
