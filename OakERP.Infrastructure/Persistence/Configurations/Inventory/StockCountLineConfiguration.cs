using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

internal class StockCountLineConfiguration : IEntityTypeConfiguration<StockCountLine>
{
    public void Configure(EntityTypeBuilder<StockCountLine> builder)
    {
        builder.ToTable("stock_count_lines");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.ExpectedQty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.CountedQty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.VarianceQty).HasColumnType("numeric(18,4)");

        // Index
        builder.HasIndex(x => new { x.StockCountId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.StockCountId, x.ItemId }).IsUnique();

        // Relationships
        builder
            .HasOne(x => x.StockCount)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.StockCountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Data integrity
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_scl_lineno_positive", "\"line_no\" > 0");
            t.HasCheckConstraint("ck_scl_expected_nonneg", "\"expected_qty\" >= 0");
            t.HasCheckConstraint("ck_scl_counted_nonneg", "\"counted_qty\" >= 0");

            builder
                .Property(x => x.VarianceQty)
                .HasColumnType("numeric(18,4)")
                .HasComputedColumnSql("\"counted_qty\" - \"expected_qty\"", stored: true)
                .ValueGeneratedOnAddOrUpdate();
        });
    }
}