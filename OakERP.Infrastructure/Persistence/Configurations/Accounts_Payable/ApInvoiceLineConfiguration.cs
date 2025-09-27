using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;
using InvItem = OakERP.Domain.Entities.Inventory.Item;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class ApInvoiceLineConfiguration : IEntityTypeConfiguration<ApInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ApInvoiceLine> builder)
    {
        builder.ToTable("ap_invoice_lines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Qty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)");
        builder.Property(x => x.LineTotal).HasColumnType("numeric(18,2)");

        builder.HasIndex(x => new { x.ApInvoiceId, x.LineNo }).IsUnique();

        builder
            .HasOne(x => x.Invoice)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.ApInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountNo)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.TaxRate)
            .WithMany()
            .HasForeignKey(x => x.TaxRateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<InvItem>()
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}