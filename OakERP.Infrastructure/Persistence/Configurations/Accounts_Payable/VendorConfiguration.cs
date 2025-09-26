using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendors");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.VendorCode).IsUnique();
    }
}
