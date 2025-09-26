using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class ApPaymentAllocationConfiguration : IEntityTypeConfiguration<ApPaymentAllocation>
{
    public void Configure(EntityTypeBuilder<ApPaymentAllocation> builder)
    {
        builder.ToTable("ap_payment_allocations");

        builder.HasKey(x => x.Id);
    }
}
