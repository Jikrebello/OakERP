using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Recievable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

public class ArInvoiceConfiguration : IEntityTypeConfiguration<ArInvoice>
{
    public void Configure(EntityTypeBuilder<ArInvoice> builder)
    {
        builder.ToTable("ar_invoices");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DocNo).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.DueDate });

        builder.Property(x => x.TaxTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DocTotal).HasColumnType("numeric(18,2)");
    }
}
