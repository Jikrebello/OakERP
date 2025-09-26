using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Recievable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

public class ArReceiptAllocationConfiguration : IEntityTypeConfiguration<ArReceiptAllocation>
{
    public void Configure(EntityTypeBuilder<ArReceiptAllocation> builder)
    {
        builder.ToTable("ar_receipt_allocations");

        builder.HasKey(x => x.Id);
    }
}
