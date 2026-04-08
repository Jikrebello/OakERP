using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Infrastructure.Persistence.Configurations.GeneralLedger;

internal class GlEntryConfiguration : IEntityTypeConfiguration<GlEntry>
{
    public void Configure(EntityTypeBuilder<GlEntry> builder)
    {
        builder.ToTable("gl_entries");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.EntryDate).HasColumnType("date");
        builder.Property(x => x.AccountNo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Debit).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Credit).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceNo).HasMaxLength(64);

        // Timestamps
        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        // Relationships
        builder
            .HasOne(x => x.Account)
            .WithMany(a => a.Entries)
            .HasForeignKey(x => x.AccountNo)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Period)
            .WithMany(p => p.Entries)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.AccountNo, x.EntryDate }); // account drilldowns by date
        builder.HasIndex(x => new { x.PeriodId, x.AccountNo }); // trial balance by period/account
        builder.HasIndex(x => x.PeriodId); // general period queries
        builder.HasIndex(x => new { x.SourceType, x.SourceId }); // trace back to origin

        // Data integrity
        builder.ToTable(t =>
        {
            // Non-negative and one-sided amount:
            // exactly one of Debit/Credit > 0 (and not both, and not both zero)
            t.HasCheckConstraint(
                "ck_glentry_one_sided_amount",
                "(\"debit\" >= 0) AND (\"credit\" >= 0) AND "
                    + "((\"debit\" = 0 AND \"credit\" > 0) OR (\"credit\" = 0 AND \"debit\" > 0))"
            );

            // require SourceId when SourceType is set
            t.HasCheckConstraint(
                "ck_glentry_source_pairing",
                "(\"source_type\" IS NULL) OR (\"source_id\" IS NOT NULL)"
            );
        });
    }
}
