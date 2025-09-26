using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

public class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
{
    public void Configure(EntityTypeBuilder<ItemCategory> builder)
    {
        builder.ToTable("item_categories");

        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.InvAccount)
            .WithMany()
            .HasForeignKey(x => x.InventoryAccount)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne(x => x.Cogs)
            .WithMany()
            .HasForeignKey(x => x.CogsAccount)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasOne(x => x.Adjust)
            .WithMany()
            .HasForeignKey(x => x.AdjustAccount)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
