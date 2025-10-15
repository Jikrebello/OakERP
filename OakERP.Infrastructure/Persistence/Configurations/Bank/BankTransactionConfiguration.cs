using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Infrastructure.Persistence.Configurations.Bank;

internal class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("bank_transactions");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.TxnDate).HasColumnType("date");
        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");

        builder.Property(x => x.DrAccountNo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CrAccountNo).HasMaxLength(20).IsRequired();

        builder.Property(x => x.SourceType).HasMaxLength(64);
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.ExternalRef).HasMaxLength(128);

        builder.Property(x => x.ReconciledDate).HasColumnType("date");

        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        // Relationships
        builder
            .HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.DrAccount)
            .WithMany()
            .HasForeignKey(x => x.DrAccountNo)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CrAccount)
            .WithMany()
            .HasForeignKey(x => x.CrAccountNo)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.BankAccountId, x.TxnDate });
        builder.HasIndex(x => x.IsReconciled);
        builder.HasIndex(x => x.ReconciledDate);
        builder.HasIndex(x => x.DrAccountNo);
        builder.HasIndex(x => x.CrAccountNo);

        builder
            .HasIndex(x => new { x.SourceType, x.SourceId })
            .IsUnique()
            .HasFilter("\"SourceType\" IS NOT NULL AND \"SourceId\" IS NOT NULL");

        builder.ToTable(t =>
        {
            // Amount cannot be zero
            t.HasCheckConstraint("ck_banktxn_amount_nonzero", "\"Amount\" <> 0");

            // DR and CR must be different accounts
            t.HasCheckConstraint("ck_banktxn_dr_neq_cr", "\"DrAccountNo\" <> \"CrAccountNo\"");

            // If reconciled, a date must be present
            t.HasCheckConstraint(
                "ck_banktxn_reconciled_requires_date",
                "(NOT \"IsReconciled\") OR (\"ReconciledDate\" IS NOT NULL)"
            );
        });
    }
}
