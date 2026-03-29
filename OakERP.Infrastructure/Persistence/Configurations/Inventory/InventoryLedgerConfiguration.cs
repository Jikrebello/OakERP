using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

internal class InventoryLedgerConfiguration : IEntityTypeConfiguration<InventoryLedger>
{
    public void Configure(EntityTypeBuilder<InventoryLedger> builder)
    {
        builder.ToTable("inventory_ledgers");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.TrxDate).HasColumnType("date");

        builder.Property(x => x.Qty).HasColumnType("numeric(18,4)");

        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)");

        builder.Property(x => x.ValueChange).HasColumnType("numeric(18,2)");

        builder.Property(x => x.SourceType).HasMaxLength(64).IsRequired();

        builder.Property(x => x.SourceId).IsRequired();

        builder.Property(x => x.Note).HasMaxLength(512);

        builder.Property(x => x.CreatedBy).HasMaxLength(64);

        // Timestamps
        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        // Relationships
        builder
            .HasOne(x => x.Item)
            .WithMany(i => i.InventoryLedgers)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.ItemId, x.TrxDate });
        builder.HasIndex(x => new
        {
            x.ItemId,
            x.LocationId,
            x.TrxDate,
        });
        builder.HasIndex(x => new { x.SourceType, x.SourceId });
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.TransactionType);

        // Data integrity
        builder.ToTable(t =>
        {
            // Qty cannot be zero (no no-op rows)
            t.HasCheckConstraint("ck_invledg_qty_nonzero", "\"qty\" <> 0");

            // Nonnegative unit cost
            t.HasCheckConstraint("ck_invledg_unitcost_nonneg", "\"unit_cost\" >= 0");

            // ValueChange sign should follow Qty sign:
            //  - incoming stock (qty > 0) => value_change >= 0
            //  - outgoing stock (qty < 0) => value_change <= 0
            t.HasCheckConstraint(
                "ck_invledg_valuechange_sign",
                "(\"qty\" > 0 AND \"value_change\" >= 0) OR "
                    + "(\"qty\" < 0 AND \"value_change\" <= 0)"
            );

            t.HasCheckConstraint(
                "ck_invledg_source_pairing",
                "(\"source_type\" IS NULL) OR (\"source_id\" IS NOT NULL)"
            );
        });
    }
}
