using EduOnline.Alunos.Domain.Models;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Alunos.Data.Context;

public class AlunosContext : EduOnlineContext
{
    public AlunosContext() { }

    public AlunosContext(DbContextOptions<AlunosContext> options)
        : base(options) { }

    public AlunosContext(DbContextOptions<AlunosContext> options, IMediatorHandler mediatorHandler) : base(options, mediatorHandler)
    {
    }

    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }
    public DbSet<Certificado> Certificados { get; set; }
}
