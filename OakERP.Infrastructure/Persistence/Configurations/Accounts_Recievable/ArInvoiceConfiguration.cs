using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

internal class ArInvoiceConfiguration : IEntityTypeConfiguration<ArInvoice>
{
    public void Configure(EntityTypeBuilder<ArInvoice> builder)
    {
        builder.ToTable("ar_invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ShipTo).HasMaxLength(512);
        builder.Property(x => x.Memo).HasMaxLength(512);

        builder.Property(x => x.TaxTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DocTotal).HasColumnType("numeric(18,2)");

        builder.Property(x => x.InvoiceDate).HasColumnType("date");
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.Property(x => x.PostingDate).HasColumnType("date");

        builder.Property(x => x.DocStatus).IsRequired();

        // Timestamps
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
            .HasOne(x => x.Customer)
            .WithMany(c => c.ArInvoices)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Currency)
            .WithMany(c => c.ArInvoices)
            .HasForeignKey(x => x.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.DocNo).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.DueDate });
        builder.HasIndex(x => x.InvoiceDate);
        builder.HasIndex(x => x.PostingDate);
        builder.HasIndex(x => x.DocStatus);
        builder.HasIndex(x => x.CurrencyCode);

        // Data integrity
        builder.ToTable(t =>
        {
            // Nonnegative totals
            t.HasCheckConstraint(
                "ck_arinvoice_totals_nonnegative",
                "(\"tax_total\" >= 0) AND (\"doc_total\" >= 0)"
            );

            // Due date after or equal to invoice date
            t.HasCheckConstraint(
                "ck_arinvoice_due_after_invoice",
                "\"due_date\" >= \"invoice_date\""
            );

            // When Posted, PostingDate must be present
            t.HasCheckConstraint(
                "ck_arinvoice_posted_requires_postingdate",
                "(\"doc_status\" <> 'posted'::doc_status) OR (\"posting_date\" IS NOT NULL)"
            );

            // Currency code must be 3 letters (DB guard in addition to length)
            t.HasCheckConstraint(
                "ck_arinvoice_currency_len3",
                "char_length(\"currency_code\") = 3"
            );
        });
    }
}