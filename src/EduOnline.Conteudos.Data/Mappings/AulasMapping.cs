using EduOnline.Conteudos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduOnline.Conteudos.Data.Mappings;

public sealed class AulasMapping : IEntityTypeConfiguration<Aula>
{
    public void Configure(EntityTypeBuilder<Aula> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.CursoId)
            .IsRequired();

        builder.Property(a => a.Titulo)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(a => a.LinkMaterial)
            .HasColumnType("varchar(2048)")
            .IsRequired();

        builder.ToTable("Aulas");
    }
}
