using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.Common;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

internal class ArReceiptConfiguration : IEntityTypeConfiguration<ArReceipt>
{
    public void Configure(EntityTypeBuilder<ArReceipt> builder)
    {
        builder.ToTable("ar_receipts");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.DocNo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Memo).HasMaxLength(512);

        builder.Property(x => x.ReceiptDate).HasColumnType("date");
        builder.Property(x => x.PostingDate).HasColumnType("date");
        builder.Property(x => x.ClearedDate).HasColumnType("date");
        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DocStatus).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.AmountForeign).HasColumnType("numeric(18,2)"); // or (18, <Currency.Decimals>)

        builder
            .HasOne(x => x.Currency)
            .WithMany(c => c.ArReceipts)
            .HasForeignKey(x => x.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        // Index
        builder.HasIndex(x => x.CurrencyCode);

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
            .WithMany(c => c.ArReceipts)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.BankAccount)
            .WithMany(b => b.ArReceipts)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.DocNo).IsUnique();
        builder.HasIndex(x => x.ReceiptDate);
        builder.HasIndex(x => x.PostingDate);
        builder.HasIndex(x => x.ClearedDate);
        builder.HasIndex(x => x.DocStatus);
        builder.HasIndex(x => new { x.CustomerId, x.ReceiptDate });
        builder.HasIndex(x => new { x.CustomerId, x.PostingDate });
        builder.HasIndex(x => new { x.BankAccountId, x.ReceiptDate });
        builder.HasIndex(x => new { x.BankAccountId, x.PostingDate });

        // Data integrity
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_arreceipt_amount_positive", "\"Amount\" > 0");

            // Posted → PostingDate required
            t.HasCheckConstraint(
                "ck_arreceipt_posted_requires_postingdate",
                "(\"DocStatus\" <> 'posted'::doc_status) OR (\"PostingDate\" IS NOT NULL)"
            );

            // ClearedDate cannot be before PostingDate (when both present)
            t.HasCheckConstraint(
                "ck_arreceipt_cleared_not_before_posting",
                "(\"ClearedDate\" IS NULL) OR (\"PostingDate\" IS NULL) OR (\"ClearedDate\" >= \"PostingDate\")"
            );

            // --- FX sanity checks ---
            // Either both FX fields are NULL, or both are NOT NULL
            t.HasCheckConstraint(
                "ck_arreceipt_fx_pair_nullness",
                "(\"AmountForeign\" IS NULL) = (\"ExchangeRate\" IS NULL)"
            );

            // If set, they must be positive
            t.HasCheckConstraint(
                "ck_arreceipt_fx_positive",
                "(\"AmountForeign\" IS NULL OR \"AmountForeign\" > 0) AND "
                    + "(\"ExchangeRate\" IS NULL OR \"ExchangeRate\" > 0)"
            );

            // Currency code length 3 (extra guard)
            t.HasCheckConstraint("ck_arreceipt_currency_len3", "char_length(\"CurrencyCode\") = 3");

            // functional vs foreign math consistency with 2-dp tolerance
            // If both FX fields present, then |(AmountForeign * ExchangeRate) - Amount| <= 0.01
            t.HasCheckConstraint(
                "ck_arreceipt_fx_consistency",
                "(\"AmountForeign\" IS NULL OR \"ExchangeRate\" IS NULL) OR "
                    + "abs((\"AmountForeign\" * \"ExchangeRate\") - \"Amount\") <= 0.01"
            );
        });
    }
}