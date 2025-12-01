using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

internal class StockCountConfiguration : IEntityTypeConfiguration<StockCount>
{
    public void Configure(EntityTypeBuilder<StockCount> builder)
    {
        builder.ToTable("stock_counts");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.CountNo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ScheduledOn).HasColumnType("date");
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
            .HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CountNo).IsUnique();
        builder.HasIndex(x => new { x.LocationId, x.ScheduledOn });
        builder.HasIndex(x => x.DocStatus);

        // Data integrity
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_sc_countno_not_blank", "btrim(\"count_no\") <> ''");
        });
    }
}