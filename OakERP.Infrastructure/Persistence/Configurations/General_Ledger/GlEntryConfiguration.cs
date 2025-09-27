using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Infrastructure.Persistence.Configurations.General_Ledger;

internal class GlEntryConfiguration : IEntityTypeConfiguration<GlEntry>
{
    public void Configure(EntityTypeBuilder<GlEntry> builder)
    {
        builder.ToTable("gl_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Debit).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Credit).HasColumnType("numeric(18,2)");
    }
}