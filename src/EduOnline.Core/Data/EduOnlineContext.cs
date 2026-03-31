using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Extensions;
using EduOnline.Core.Mensagens;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Core.Data;

public abstract class EduOnlineContext : DbContext, IUnitOfWork
{
    private readonly IMediatorHandler? _mediatorHandler;

    protected EduOnlineContext() { }

    protected EduOnlineContext(DbContextOptions options) : base(options) { }

    protected EduOnlineContext(DbContextOptions options, IMediatorHandler mediatorHandler)
        : base(options)
    {
        _mediatorHandler = mediatorHandler;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Event>();

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(
                e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
            property.SetAnnotation("Relational:ColumnType", "varchar(100)");

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.Ignore<Event>();
    }

    public async Task<bool> Commit()
    {
        foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("DataCriacao") != null))
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("DataCriacao").CurrentValue = DateTime.Now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("DataCriacao").IsModified = false;
            }
        }

        var sucesso = await base.SaveChangesAsync() > 0;
        if (sucesso && _mediatorHandler is not null) await _mediatorHandler.PublicarEventos(this);

        return sucesso;
    }
}
