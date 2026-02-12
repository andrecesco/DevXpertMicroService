using EduOnline.Pagamentos.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduOnline.Pagamentos.Data.Mappings;

public class TransacoesMappings : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.PagamentoId)
            .IsRequired();
        builder.Property(a => a.Total)
            .IsRequired();
        builder.ToTable("Transacoes");
    }
}
