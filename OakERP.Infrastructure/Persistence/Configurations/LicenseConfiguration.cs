using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity type <see cref="License"/> for use with Entity Framework Core.
/// </summary>
/// <remarks>This configuration defines the primary key for the <see cref="License"/> entity and establishes a
/// one-to-one relationship with the <see cref="Tenant"/> entity. The relationship is configured with a cascading delete
/// behavior, ensuring that when a <see cref="Tenant"/> is deleted, its associated <see cref="License"/> is also
/// removed.</remarks>
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