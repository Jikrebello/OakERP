using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class ApInvoiceConfiguration : IEntityTypeConfiguration<ApInvoice>
{
    public void Configure(EntityTypeBuilder<ApInvoice> builder)
    {
        builder.ToTable("ap_invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNo).HasMaxLength(40).IsRequired();

        builder.HasIndex(x => x.DocNo).IsUnique();
        builder.HasIndex(x => new { x.VendorId, x.DueDate });

        // Money
        builder.Property(x => x.TaxTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DocTotal).HasColumnType("numeric(18,2)");

        // Dates
        builder.Property(x => x.InvoiceDate).HasColumnType("date");
        builder.Property(x => x.DueDate).HasColumnType("date");

        // Relationships
        builder
            .HasOne(x => x.Vendor)
            .WithMany()
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}