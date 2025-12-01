using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Common;

namespace OakERP.Infrastructure.Persistence.Configurations.Common;

internal class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("tax_rates");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RatePercent).HasColumnType("numeric(6,3)");
        builder.Property(x => x.EffectiveFrom).HasColumnType("date");
        builder.Property(x => x.EffectiveTo).HasColumnType("date");

        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        // Handy computed column: fraction form (0.150000 for 15%)
        builder
            .Property<decimal>("rate_fraction")
            .HasColumnType("numeric(9,6)")
            .HasComputedColumnSql("(\"rate_percent\" / 100.0)", stored: true);

        // Indexes
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.EffectiveFrom);
        builder.HasIndex(x => new { x.Name, x.EffectiveFrom }).IsUnique();

        // Data integrity checks
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_taxrate_pct_range",
                "\"rate_percent\" >= 0 AND \"rate_percent\" <= 100"
            );
            t.HasCheckConstraint("ck_taxrate_name_not_blank", "btrim(\"name\") <> ''");
            t.HasCheckConstraint(
                "ck_taxrate_dates",
                "\"effective_to\" IS NULL OR \"effective_to\" >= \"effective_from\""
            );
        });
    }
}