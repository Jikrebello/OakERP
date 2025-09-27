using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

public class ArReceiptConfiguration : IEntityTypeConfiguration<ArReceipt>
{
    public void Configure(EntityTypeBuilder<ArReceipt> builder)
    {
        builder.ToTable("ar_receipts");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DocNo).IsUnique();
    }
}
