using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class ApInvoiceConfiguration : IEntityTypeConfiguration<ApInvoice>
{
    public void Configure(EntityTypeBuilder<ApInvoice> builder)
    {
        builder.ToTable("ap_invoices");

        // PK
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.InvoiceNo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Memo).HasMaxLength(512);

        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        builder
            .HasOne(x => x.Currency)
            .WithMany(c => c.ApInvoices)
            .HasForeignKey(x => x.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.TaxTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DocTotal).HasColumnType("numeric(18,2)");

        builder.Property(x => x.InvoiceDate).HasColumnType("date");
        builder.Property(x => x.DueDate).HasColumnType("date");

        builder.Property(x => x.DocStatus).IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        // Relationships
        builder
            .HasOne(x => x.Vendor)
            .WithMany(v => v.ApInvoices)
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.DocNo).IsUnique();
        builder.HasIndex(x => new { x.VendorId, x.InvoiceNo }).IsUnique();
        builder.HasIndex(x => new { x.VendorId, x.DueDate });
        builder.HasIndex(x => x.InvoiceDate);
        builder.HasIndex(x => x.DocStatus);

        // Data integrity checks
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_apinvoice_totals_nonnegative",
                "(\"tax_total\" >= 0) AND (\"doc_total\" >= 0)"
            );
            t.HasCheckConstraint(
                "ck_apinvoice_due_after_invoice",
                "\"due_date\" >= \"invoice_date\""
            );
            t.HasCheckConstraint(
                "ck_apinvoice_currency_len3",
                "char_length(\"currency_code\") = 3"
            );
        });
    }
}