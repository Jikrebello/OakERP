using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Infrastructure.Persistence.Configurations.General_Ledger;

internal class GlJournalConfiguration : IEntityTypeConfiguration<GlJournal>
{
    public void Configure(EntityTypeBuilder<GlJournal> builder)
    {
        builder.ToTable("gl_journals");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.JournalNo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.JournalDate).HasColumnType("date");
        builder.Property(x => x.PostingDate).HasColumnType("date");
        builder.Property(x => x.Memo).HasMaxLength(512);
        builder.Property(x => x.DocStatus).IsRequired();

        // Timestamps
        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        // Indexes
        builder.HasIndex(x => x.JournalNo).IsUnique();
        builder.HasIndex(x => x.JournalDate);
        builder.HasIndex(x => x.PostingDate);
        builder.HasIndex(x => x.DocStatus);

        // Relationship
        builder
            .HasOne(x => x.Period)
            .WithMany(p => p.Journals)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        // Data integrity
        builder.ToTable(t =>
        {
            // no blank journal numbers
            t.HasCheckConstraint("ck_gljournal_no_not_blank", "btrim(\"JournalNo\") <> ''");

            // if Posted, require a PostingDate (keeps GL timing explicit)
            t.HasCheckConstraint(
                "ck_gljournal_posted_requires_postingdate",
                "(\"DocStatus\" <> 'posted'::doc_status) OR (\"PostingDate\" IS NOT NULL)"
            );

            // PostingDate not before JournalDate
            t.HasCheckConstraint(
                "ck_gljournal_posting_ge_journal",
                "\"PostingDate\" IS NULL OR \"PostingDate\" >= \"JournalDate\""
            );
        });
    }
}