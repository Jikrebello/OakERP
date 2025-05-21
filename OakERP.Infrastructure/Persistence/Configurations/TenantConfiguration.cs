using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity type <see cref="Tenant"/> for use with Entity Framework Core.
/// </summary>
/// <remarks>This configuration defines the primary key, required properties, default values, and relationships
/// for the <see cref="Tenant"/> entity. It ensures that the <see cref="Tenant"/> entity is properly mapped to the
/// database schema.</remarks>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()").ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired();

        builder
            .HasOne(t => t.License)
            .WithOne(l => l.Tenant)
            .HasForeignKey<License>(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd();
    }
}