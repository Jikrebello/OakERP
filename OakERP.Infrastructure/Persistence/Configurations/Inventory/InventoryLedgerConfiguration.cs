using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

public class InventoryLedgerConfiguration : IEntityTypeConfiguration<InventoryLedger>
{
    public void Configure(EntityTypeBuilder<InventoryLedger> builder)
    {
        builder.ToTable("inventory_ledgers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Qty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)");
        builder.Property(x => x.ValueChange).HasColumnType("numeric(18,2)");
    }
}
