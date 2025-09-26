using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class ApPaymentConfiguration : IEntityTypeConfiguration<ApPayment>
{
    public void Configure(EntityTypeBuilder<ApPayment> builder)
    {
        builder.ToTable("ap_payments");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DocNo).IsUnique();
    }
}
