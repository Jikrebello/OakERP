using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

internal class ArInvoiceLineConfiguration : IEntityTypeConfiguration<ArInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ArInvoiceLine> builder)
    {
        builder.ToTable("ar_invoice_lines");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.Qty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)");
        builder.Property(x => x.LineTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.RevenueAccount).HasMaxLength(20); // FK to GlAccount.AccountNo

        builder.HasIndex(x => new { x.ArInvoiceId, x.LineNo }).IsUnique();

        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.TaxRateId);
        builder.HasIndex(x => x.RevenueAccount);

        // Relationships
        builder
            .HasOne(x => x.Invoice)
            .WithMany(i => i.Lines)
            .HasForeignKey(x => x.ArInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Item)
            .WithMany(i => i.ArInvoiceLines)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Revenue)
            .WithMany(a => a.ArInvoiceLines)
            .HasForeignKey(x => x.RevenueAccount)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.TaxRate)
            .WithMany()
            .HasForeignKey(x => x.TaxRateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Data integrity
        builder.ToTable(t =>
        {
            // positive line number
            t.HasCheckConstraint("ck_arline_lineno_positive", "\"LineNo\" > 0");

            // nonnegative qty/price/total
            t.HasCheckConstraint("ck_arline_qty_nonnegative", "\"Qty\" >= 0");
            t.HasCheckConstraint("ck_arline_price_nonnegative", "\"UnitPrice\" >= 0");
            t.HasCheckConstraint("ck_arline_total_nonnegative", "\"LineTotal\" >= 0");

            // Posting precedence (doc rule): if RevenueAccount present → use it; else use Item’s revenue account
            t.HasCheckConstraint(
                "ck_arline_has_item_or_revenue",
                "(\"ItemId\" IS NOT NULL) OR (\"RevenueAccount\" IS NOT NULL)"
            );
        });
    }
}