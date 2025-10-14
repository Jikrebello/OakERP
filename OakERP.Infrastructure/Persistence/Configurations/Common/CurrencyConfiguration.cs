using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Common;

namespace OakERP.Infrastructure.Persistence.Configurations.Common;

internal sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currencies");

        builder.HasKey(c => c.Code);

        builder.Property(c => c.Code).HasMaxLength(3).IsRequired();

        builder.Property(c => c.NumericCode).IsRequired();

        builder.Property(c => c.Name).HasMaxLength(80).IsRequired();

        builder.Property(c => c.Symbol).HasMaxLength(8);

        builder.Property(c => c.Decimals).IsRequired();

        builder
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(c => c.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        // Indexes
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.NumericCode).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_currency_code_len3", "char_length(\"Code\") = 3");
            t.HasCheckConstraint("ck_currency_code_upper", "\"Code\" = upper(\"Code\")");
            t.HasCheckConstraint("ck_currency_decimals_range", "\"Decimals\" BETWEEN 0 AND 4");
            t.HasCheckConstraint("ck_currency_numeric_range", "\"NumericCode\" BETWEEN 1 AND 999");
        });
    }
}