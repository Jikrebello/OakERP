using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Common;

namespace OakERP.Infrastructure.Persistence.Configurations.Common;

internal class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("app_settings");

        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key).HasMaxLength(128).IsRequired();

        builder.Property(x => x.ValueJson).HasColumnType("jsonb").IsRequired();

        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").HasDefaultValueSql("now()");
    }
}
