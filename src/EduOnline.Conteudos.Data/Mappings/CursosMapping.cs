using EduOnline.Conteudos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduOnline.Conteudos.Data.Mappings;

public sealed class CursosMapping : IEntityTypeConfiguration<Curso>
{
    public void Configure(EntityTypeBuilder<Curso> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired();

        builder.Property(c => c.Autor)
            .IsRequired();

        builder.Property(c => c.Validade)
            .IsRequired();

        builder.OwnsOne(c => c.ConteudoProgramatico, cp =>
        {
            cp.Property(c => c.Tema)
                .HasColumnName("Tema")
                .HasColumnType("varchar(2048)")
                .IsRequired();

            cp.Property(c => c.NivelId)
                .HasColumnName("NivelId")
                .IsRequired();

            cp.Property(c => c.CargaHoraria)
                .HasColumnName("CargaHoraria")
                .IsRequired();
        });

        builder.ToTable("Cursos");
    }
}
