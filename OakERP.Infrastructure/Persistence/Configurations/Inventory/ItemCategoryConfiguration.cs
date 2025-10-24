using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

internal class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
{
    public void Configure(EntityTypeBuilder<ItemCategory> builder)
    {
        builder.ToTable("item_categories");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.InventoryAccount).HasMaxLength(20);
        builder.Property(x => x.CogsAccount).HasMaxLength(20);
        builder.Property(x => x.AdjustAccount).HasMaxLength(20);
        builder.Property(x => x.RevenueAccount).HasMaxLength(20);
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name).IsUnique();

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
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasIndex(x => x.InventoryAccount);
        builder.HasIndex(x => x.CogsAccount);
        builder.HasIndex(x => x.AdjustAccount);
        builder.HasIndex(x => x.RevenueAccount);

        // Relationships (point to GlAccount.AccountNo explicitly)
        builder
            .HasOne(x => x.InvAccount)
            .WithMany()
            .HasForeignKey(x => x.InventoryAccount)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Cogs)
            .WithMany()
            .HasForeignKey(x => x.CogsAccount)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Adjust)
            .WithMany()
            .HasForeignKey(x => x.AdjustAccount)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Revenue)
            .WithMany()
            .HasForeignKey(x => x.RevenueAccount)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.SetNull);
    }
}