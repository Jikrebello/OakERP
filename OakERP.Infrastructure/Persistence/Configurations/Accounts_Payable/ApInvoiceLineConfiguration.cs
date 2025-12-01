using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class ApInvoiceLineConfiguration : IEntityTypeConfiguration<ApInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ApInvoiceLine> builder)
    {
        builder.ToTable("ap_invoice_lines");

        builder.HasKey(x => x.Id);

        // Column definitions
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.AccountNo).HasMaxLength(20);
        builder.Property(x => x.Qty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)");
        builder.Property(x => x.LineTotal).HasColumnType("numeric(18,2)");

        // Indexes
        builder.HasIndex(x => new { x.ApInvoiceId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.ItemId, x.AccountNo });

        builder.HasIndex(x => x.AccountNo);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.TaxRateId);

        // Relationships
        builder
            .HasOne(x => x.Invoice)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.ApInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Account)
            .WithMany(a => a.ApInvoiceLines)
            .HasForeignKey(x => x.AccountNo)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.TaxRate)
            .WithMany(t => t.ApInvoiceLines)
            .HasForeignKey(x => x.TaxRateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Item)
            .WithMany(i => i.ApInvoiceLines)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_apline_lineno_positive", "\"line_no\" > 0");

            // Non-negative quantities and amounts
            t.HasCheckConstraint("ck_apline_qty_nonnegative", "\"qty\" >= 0");
            t.HasCheckConstraint("ck_apline_price_nonnegative", "\"unit_price\" >= 0");
            t.HasCheckConstraint("ck_apline_total_nonnegative", "\"line_total\" >= 0");

            t.HasCheckConstraint(
                "ck_apline_has_account_or_item",
                "(\"account_no\" IS NOT NULL) OR (\"item_id\" IS NOT NULL)"
            );
        });
    }
}