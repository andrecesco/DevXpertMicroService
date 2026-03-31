using EduOnline.Pagamentos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduOnline.Pagamentos.Data.Mappings;

public class PagamentosMappings : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.CursoId)
            .IsRequired();

        builder.Property(a => a.Total)
            .IsRequired();

        builder.Property(a => a.NomeCartao)
            .IsRequired();

        builder.Property(a => a.NumeroCartao)
            .IsRequired();

        builder.Property(a => a.ExpiracaoCartao)
            .IsRequired();

        builder.Property(a => a.CvvCartao)
            .IsRequired();

        builder.ToTable("Pagamentos");
    }
}
