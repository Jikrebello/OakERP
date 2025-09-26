using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Infrastructure.Persistence.Configurations.Inventory;

public class StockCountLineConfiguration : IEntityTypeConfiguration<StockCountLine>
{
    public void Configure(EntityTypeBuilder<StockCountLine> builder)
    {
        builder.ToTable("stock_count_lines");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.StockCountId, x.LineNo }).IsUnique();
    }
}
