using EduOnline.Alunos.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduOnline.Alunos.Data.Mappings;

public sealed class AlunosMapping : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome)
            .IsRequired();

        builder.Property(a => a.Email)
            .IsRequired();

        builder.ToTable("Alunos");
    }
}
