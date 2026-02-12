using EduOnline.Alunos.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduOnline.Alunos.Data.Mappings;

public sealed class MatriculasMapping : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.CursoId)
            .IsRequired();

        builder.Property(m => m.CursoNome)
            .IsRequired();

        builder.Property(m => m.Validade)
            .IsRequired();

        builder.OwnsOne(c => c.HistoricoAprendizagem, cp =>
        {
            cp.Property(c => c.TotalAulas)
                .HasColumnName("TotalAulas")
                .IsRequired();

            cp.Property(c => c.AulasConcluidas)
                .HasColumnName("AulasConcluidas")
                .IsRequired();
        });

        builder.ToTable("Matriculas");
    }
}
