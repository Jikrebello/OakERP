using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

public class ArInvoiceLineConfiguration : IEntityTypeConfiguration<ArInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ArInvoiceLine> builder)
    {
        builder.ToTable("ar_invoice_lines");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ArInvoiceId, x.LineNo }).IsUnique();

        builder.Property(x => x.Qty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)");
        builder.Property(x => x.LineTotal).HasColumnType("numeric(18,2)");
    }
}
