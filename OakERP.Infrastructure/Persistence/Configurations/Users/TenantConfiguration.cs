using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Users;

namespace OakERP.Infrastructure.Persistence.Configurations.Users;

internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

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
