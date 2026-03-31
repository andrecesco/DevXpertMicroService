using EduOnline.Conteudos.Domain;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Conteudos.Data.Context;

public class ConteudosContext : EduOnlineContext
{
    public ConteudosContext() { }

    public ConteudosContext(DbContextOptions options)
        : base(options) { }

    public ConteudosContext(DbContextOptions options, IMediatorHandler mediatorHandler) : base(options, mediatorHandler)
    {
    }

    public DbSet<Aula> Aulas { get; set; }
    public DbSet<Curso> Cursos { get; set; }
}
