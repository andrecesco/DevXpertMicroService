using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Core.Extensions;

public static class MediatorDbContextExtension
{
    public static async Task PublicarEventos<T>(this IMediatorHandler mediator, T ctx) where T : DbContext
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.Notificacoes != null && x.Entity.Notificacoes.Any());

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.Notificacoes ?? [])
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.LimparEventos());

        var tasks = domainEvents
            .Select(async (domainEvent) =>
            {
                await mediator.PublicarEvento(domainEvent);
            });

        await Task.WhenAll(tasks);
    }
}
