using EduOnline.Alunos.Domain.Models;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Data;
using EduOnline.Core.Mensagens;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Alunos.Data.Context;

public class AlunosContext(DbContextOptions<AlunosContext> options, IMediatorHandler mediatorHandler) : DbContext(options), IUnitOfWork
{
    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }
    public DbSet<Certificado> Certificados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(
                e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
            property.SetAnnotation("Relational:ColumnType", "varchar(100)");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlunosContext).Assembly);

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
        if (sucesso) await mediatorHandler.PublicarEventos(this);

        return sucesso;
    }
}
