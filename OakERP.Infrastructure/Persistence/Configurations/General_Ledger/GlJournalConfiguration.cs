using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Infrastructure.Persistence.Configurations.General_Ledger;

public class GlJournalConfiguration : IEntityTypeConfiguration<GlJournal>
{
    public void Configure(EntityTypeBuilder<GlJournal> builder)
    {
        builder.ToTable("gl_journals");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.JournalNo).IsUnique();
    }
}
