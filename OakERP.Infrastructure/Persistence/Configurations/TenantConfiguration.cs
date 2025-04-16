using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()").ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .ValueGeneratedOnAdd();
    }
}