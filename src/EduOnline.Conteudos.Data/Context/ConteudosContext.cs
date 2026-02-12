using EduOnline.Conteudos.Domain;
using EduOnline.Core.Data;
using EduOnline.Core.Mensagens;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Conteudos.Data.Context;

public class ConteudosContext(DbContextOptions<ConteudosContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Aula> Aulas { get; set; }
    public DbSet<Curso> Cursos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(
                e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
            property.SetAnnotation("Relational:ColumnType", "varchar(100)");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConteudosContext).Assembly);

        modelBuilder.Entity<Curso>().HasQueryFilter(a => a.Ativo);

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

        return await base.SaveChangesAsync() > 0;
    }
}
