using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Infrastructure.Persistence.Configurations.Bank;

internal class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.ToTable("bank_statements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StatementFrom).HasColumnType("date");
        builder.Property(x => x.StatementTo).HasColumnType("date");
        builder.Property(x => x.Source).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ExternalId).HasMaxLength(128);
        builder.Property(x => x.FileName).HasMaxLength(256);
        builder.Property(x => x.Notes).HasMaxLength(1024);
        builder.Property(x => x.OpeningBalance).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ClosingBalance).HasColumnType("numeric(18,2)");

        builder
            .HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder
            .HasIndex(x => new
            {
                x.BankAccountId,
                x.StatementFrom,
                x.StatementTo,
            })
            .IsUnique();
        builder
            .HasIndex(x => new { x.Source, x.ExternalId })
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL");

        // Guards
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_bankstmt_range_valid", "\"StatementTo\" >= \"StatementFrom\"");
        });
    }
}
