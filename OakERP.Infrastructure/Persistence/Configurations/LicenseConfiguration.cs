using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.HasKey(l => l.Id);

        builder
            .HasOne(l => l.Tenant)
            .WithOne(t => t.License)
            .HasForeignKey<License>(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}