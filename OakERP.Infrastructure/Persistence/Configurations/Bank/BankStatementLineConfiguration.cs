using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Infrastructure.Persistence.Configurations.Bank;

internal class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("bank_statement_lines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TxnDate).HasColumnType("date");
        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.Reference).HasMaxLength(128);
        builder.Property(x => x.Counterparty).HasMaxLength(256);
        builder.Property(x => x.ExternalLineId).HasMaxLength(128);
        builder.Property(x => x.RawCode).HasMaxLength(64);
        builder.Property(x => x.MatchStatus).HasMaxLength(16);

        builder
            .HasOne(x => x.Statement)
            .WithMany(s => s.Lines)
            .HasForeignKey(x => x.BankStatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.BankTransaction)
            .WithMany()
            .HasForeignKey(x => x.BankTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => new { x.BankAccountId, x.TxnDate });
        builder.HasIndex(x => x.MatchStatus);

        builder
            .HasIndex(x => new { x.BankStatementId, x.ExternalLineId })
            .IsUnique()
            .HasFilter("\"external_line_id\" IS NOT NULL");

        // Guards
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_bankstmtline_amount_nonzero", "\"amount\" <> 0");
            t.HasCheckConstraint(
                "ck_bankstmtline_matchstatus_allowed",
                "\"match_status\" IN ('unmatched','proposed','matched','ignored')"
            );
        });
    }
}
