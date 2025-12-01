using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Infrastructure.Persistence.Configurations.Bank;

internal class BankReconciliationConfiguration : IEntityTypeConfiguration<BankReconciliation>
{
    public void Configure(EntityTypeBuilder<BankReconciliation> builder)
    {
        builder.ToTable("bank_reconciliations");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.StatementFrom).HasColumnType("date");
        builder.Property(x => x.StatementTo).HasColumnType("date");
        builder.Property(x => x.OpeningBalance).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ClosingBalance).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Notes).HasMaxLength(1024);

        builder
            .HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new
        {
            x.BankAccountId,
            x.StatementFrom,
            x.StatementTo,
        });

        // Guards
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_bankrec_range_valid",
                "\"statement_to\" >= \"statement_from\""
            );
        });
    }
}