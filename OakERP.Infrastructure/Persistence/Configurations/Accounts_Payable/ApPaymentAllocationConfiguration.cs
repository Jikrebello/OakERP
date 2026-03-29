using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class ApPaymentAllocationConfiguration : IEntityTypeConfiguration<ApPaymentAllocation>
{
    public void Configure(EntityTypeBuilder<ApPaymentAllocation> builder)
    {
        builder.ToTable("ap_payment_allocations");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.AllocationDate).HasColumnType("date");
        builder.Property(x => x.AmountApplied).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DiscountTaken).HasColumnType("numeric(18,2)");
        builder.Property(x => x.WriteOffAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Memo).HasMaxLength(512);

        // Indexes / uniqueness
        builder.HasIndex(x => new { x.ApPaymentId, x.AllocationDate });
        builder.HasIndex(x => new { x.ApInvoiceId, x.AllocationDate });
        builder.HasIndex(x => x.ApPaymentId);
        builder.HasIndex(x => x.ApInvoiceId);

        // Relationships
        builder
            .HasOne(x => x.Payment)
            .WithMany(p => p.Allocations)
            .HasForeignKey(x => x.ApPaymentId)
            .OnDelete(DeleteBehavior.Cascade); // delete payment → delete its allocation events

        builder
            .HasOne(x => x.Invoice)
            .WithMany(i => i.Allocations)
            .HasForeignKey(x => x.ApInvoiceId)
            .OnDelete(DeleteBehavior.Restrict); // protect invoices that have been allocated

        // Data integrity
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_apalloc_amount_positive", "\"amount_applied\" > 0");
            t.HasCheckConstraint(
                "ck_apalloc_discount_nonneg",
                "\"discount_taken\" IS NULL OR \"discount_taken\" >= 0"
            );
            t.HasCheckConstraint(
                "ck_apalloc_writeoff_nonneg",
                "\"write_off_amount\" IS NULL OR \"write_off_amount\" >= 0"
            );
            // Disallow “everything zero”
            t.HasCheckConstraint(
                "ck_apalloc_has_value",
                "(\"amount_applied\" > 0) OR "
                    + "(COALESCE(\"discount_taken\",0) > 0) OR "
                    + "(COALESCE(\"write_off_amount\",0) > 0)"
            );
        });
    }
}
