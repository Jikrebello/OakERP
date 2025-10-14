using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

internal class ArReceiptAllocationConfiguration : IEntityTypeConfiguration<ArReceiptAllocation>
{
    public void Configure(EntityTypeBuilder<ArReceiptAllocation> builder)
    {
        builder.ToTable("ar_receipt_allocations");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.AllocationDate).HasColumnType("date");
        builder.Property(x => x.AmountApplied).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DiscountGiven).HasColumnType("numeric(18,2)");
        builder.Property(x => x.WriteOffAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Memo).HasMaxLength(512);

        // Indexes (history-friendly)
        builder.HasIndex(x => new { x.ArReceiptId, x.AllocationDate });
        builder.HasIndex(x => new { x.ArInvoiceId, x.AllocationDate });
        builder.HasIndex(x => x.ArReceiptId);
        builder.HasIndex(x => x.ArInvoiceId);

        // Relationships
        builder
            .HasOne(x => x.Receipt)
            .WithMany(r => r.Allocations)
            .HasForeignKey(x => x.ArReceiptId)
            .OnDelete(DeleteBehavior.Cascade); // delete receipt → delete its allocation events

        builder
            .HasOne(x => x.Invoice)
            .WithMany(i => i.Allocations)
            .HasForeignKey(x => x.ArInvoiceId)
            .OnDelete(DeleteBehavior.Restrict); // protect invoices with allocations

        // Data integrity
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_aralloc_amount_positive", "\"AmountApplied\" > 0");
            t.HasCheckConstraint(
                "ck_aralloc_discount_nonneg",
                "\"DiscountGiven\" IS NULL OR \"DiscountGiven\" >= 0"
            );
            t.HasCheckConstraint(
                "ck_aralloc_writeoff_nonneg",
                "\"WriteOffAmount\" IS NULL OR \"WriteOffAmount\" >= 0"
            );

            // Prevent no-op rows (at least one value > 0)
            t.HasCheckConstraint(
                "ck_aralloc_has_value",
                "(\"AmountApplied\" > 0) OR "
                    + "(COALESCE(\"DiscountGiven\", 0) > 0) OR "
                    + "(COALESCE(\"WriteOffAmount\", 0) > 0)"
            );
        });
    }
}