using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.AccountsPayable;

namespace OakERP.Infrastructure.Persistence.Configurations.AccountsPayable;

internal class ApPaymentConfiguration : IEntityTypeConfiguration<ApPayment>
{
    public void Configure(EntityTypeBuilder<ApPayment> builder)
    {
        builder.ToTable("ap_payments");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.DocNo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Memo).HasMaxLength(512);

        builder.Property(x => x.PaymentDate).HasColumnType("date");
        builder.Property(x => x.PostingDate).HasColumnType("date");
        builder.Property(x => x.ClearedDate).HasColumnType("date");

        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");
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
            .HasOne(x => x.Vendor)
            .WithMany(v => v.ApPayments)
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict); // keep payments if a vendor is blocked/deleted

        builder
            .HasOne(x => x.BankAccount)
            .WithMany(b => b.ApPayments)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict); // don't allow deleting a bank account with history

        // Indexes
        builder.HasIndex(x => x.DocNo).IsUnique();
        builder.HasIndex(x => x.PaymentDate);
        builder.HasIndex(x => x.PostingDate);
        builder.HasIndex(x => x.ClearedDate);
        builder.HasIndex(x => x.DocStatus);
        builder.HasIndex(x => new { x.VendorId, x.PaymentDate });
        builder.HasIndex(x => new { x.VendorId, x.PostingDate });
        builder.HasIndex(x => new { x.BankAccountId, x.PaymentDate });
        builder.HasIndex(x => new { x.BankAccountId, x.PostingDate });

        // Data integrity
        builder.ToTable(t =>
        {
            // Positive amount
            t.HasCheckConstraint("ck_appayment_amount_positive", "\"amount\" > 0");

            // When Posted, PostingDate must be provided
            t.HasCheckConstraint(
                "ck_appayment_posted_requires_postingdate",
                "(\"doc_status\" <> 'posted'::doc_status) OR (\"posting_date\" IS NOT NULL)"
            );

            // ClearedDate cannot precede PostingDate when both present
            t.HasCheckConstraint(
                "ck_appayment_cleared_not_before_posting",
                "(\"cleared_date\" IS NULL) OR (\"posting_date\" IS NULL) OR (\"cleared_date\" >= \"posting_date\")"
            );
        });
    }
}
