using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

internal class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.Sku).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Uom).HasMaxLength(16).IsRequired();

        builder.Property(x => x.DefaultPrice).HasColumnType("numeric(18,4)");
        builder.Property(x => x.Type).IsRequired();

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
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.Type);

        // Relationships
        builder
            .HasOne(x => x.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.DefaultRevenueAccountNo).HasMaxLength(20);
        builder.Property(x => x.DefaultExpenseAccountNo).HasMaxLength(20);

        builder
            .HasOne(x => x.DefaultRevenueAccount)
            .WithMany()
            .HasForeignKey(x => x.DefaultRevenueAccountNo)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.DefaultExpenseAccount)
            .WithMany()
            .HasForeignKey(x => x.DefaultExpenseAccountNo)
            .HasPrincipalKey("AccountNo")
            .OnDelete(DeleteBehavior.SetNull);

        // Data integrity
        builder.ToTable(t =>
        {
            // No blank codes/names
            t.HasCheckConstraint("ck_item_sku_not_blank", "btrim(\"sku\") <> ''");
            t.HasCheckConstraint("ck_item_name_not_blank", "btrim(\"name\") <> ''");

            // UoM looks like a short code; keep uppercase (optional)
            t.HasCheckConstraint("ck_item_uom_upper", "\"uom\" = upper(\"uom\")");

            // Price must be nonnegative
            t.HasCheckConstraint("ck_item_defaultprice_nonneg", "\"default_price\" >= 0");
        });
    }
}