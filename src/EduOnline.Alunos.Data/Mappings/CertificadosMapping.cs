using EduOnline.Alunos.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduOnline.Alunos.Data.Mappings;

public sealed class CertificadosMapping : IEntityTypeConfiguration<Certificado>
{
    public void Configure(EntityTypeBuilder<Certificado> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Link)
            .IsRequired();

        builder.ToTable("Certificados");
    }
}
