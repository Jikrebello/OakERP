using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Infrastructure.Persistence.Configurations.General_Ledger;

internal class GlJournalLineConfiguration : IEntityTypeConfiguration<GlJournalLine>
{
    public void Configure(EntityTypeBuilder<GlJournalLine> builder)
    {
        builder.ToTable("gl_journal_lines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountNo).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.JournalId, x.LineNo }).IsUnique();

        builder
            .HasOne(x => x.Journal)
            .WithMany(j => j.Lines)
            .HasForeignKey(x => x.JournalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Account)
            .WithMany(a => a.JournalLines)
            .HasForeignKey(x => x.AccountNo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}