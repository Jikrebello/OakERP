using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Infrastructure.Persistence.Configurations.Bank;

internal class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.BankName).HasMaxLength(120);
        builder.Property(x => x.AccountNumber).HasMaxLength(64);

        builder.Property(x => x.GlAccountNo).HasMaxLength(20).IsRequired();

        builder.Property(x => x.OpeningBalance).HasColumnType("numeric(18,2)");

        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        builder.Property(x => x.OpeningBalance).HasColumnType("numeric(18,2)");

        // Relationships
        builder
            .HasOne(x => x.GlAccount)
            .WithMany()
            .HasForeignKey(x => x.GlAccountNo)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Currency)
            .WithMany(c => c.BankAccounts)
            .HasForeignKey(x => x.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.GlAccount)
            .WithMany()
            .HasForeignKey(x => x.GlAccountNo)
            .OnDelete(DeleteBehavior.Restrict);

        // Timestamps
        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        // Indexes
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.CurrencyCode);
        builder.HasIndex(x => x.GlAccountNo);

        builder.HasIndex(x => x.Name).IsUnique();
        builder
            .HasIndex(x => x.AccountNumber)
            .IsUnique()
            .HasFilter("\"account_number\" IS NOT NULL");

        // Data integrity (Postgres CHECK constraints)
        builder.ToTable(t =>
        {
            // No blank names
            t.HasCheckConstraint("ck_bankacct_name_not_blank", "btrim(\"name\") <> ''");

            // Currency code guard
            t.HasCheckConstraint("ck_bankacct_currency_len3", "char_length(\"currency_code\") = 3");
        });
    }
}
