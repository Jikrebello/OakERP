using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

internal class ArReceiptAllocationConfiguration : IEntityTypeConfiguration<ArReceiptAllocation>
{
    public void Configure(EntityTypeBuilder<ArReceiptAllocation> builder)
    {
        builder.ToTable("ar_receipt_allocations");

        builder.HasKey(x => x.Id);
    }
}