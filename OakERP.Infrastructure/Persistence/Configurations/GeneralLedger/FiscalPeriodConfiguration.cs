using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Infrastructure.Persistence.Configurations.GeneralLedger;

internal class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("fiscal_periods");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.FiscalYear).IsRequired();
        builder.Property(x => x.PeriodNo).IsRequired();
        builder.Property(x => x.PeriodStart).HasColumnType("date");
        builder.Property(x => x.PeriodEnd).HasColumnType("date");
        builder.Property(x => x.Status).HasMaxLength(16).IsRequired(); // 'open' | 'closed' (and maybe 'locked' later)

        // Indexes
        builder.HasIndex(x => new { x.FiscalYear, x.PeriodNo }).IsUnique();
        builder.HasIndex(x => x.FiscalYear);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PeriodStart);
        builder.HasIndex(x => x.PeriodEnd);

        // Data integrity (PostgreSQL CHECKs)
        builder.ToTable(t =>
        {
            // PeriodNo 1..12
            t.HasCheckConstraint("ck_fiscper_periodno_range", "\"period_no\" BETWEEN 1 AND 12");

            // Start <= End
            t.HasCheckConstraint("ck_fiscper_start_le_end", "\"period_start\" <= \"period_end\"");

            // Allowed statuses (adjust if you add more later)
            t.HasCheckConstraint("ck_fiscper_status_allowed", "\"status\" IN ('open','closed')");
        });
    }
}
