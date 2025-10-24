using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Infrastructure.Persistence.Configurations.General_Ledger;

internal class GlJournalLineConfiguration : IEntityTypeConfiguration<GlJournalLine>
{
    public void Configure(EntityTypeBuilder<GlJournalLine> builder)
    {
        builder.ToTable("gl_journal_lines");

        // PK
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.AccountNo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Debit).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Credit).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Description).HasMaxLength(512);
        builder
            .Property<decimal>("SignedAmount")
            .HasColumnType("numeric(18,2)")
            .HasComputedColumnSql("(\"Debit\" - \"Credit\")", stored: true)
            .ValueGeneratedOnAddOrUpdate();

        // Index
        builder.HasIndex(x => new { x.JournalId, x.LineNo }).IsUnique();
        builder.HasIndex(x => x.AccountNo);
        builder.HasIndex("SignedAmount");

        // Relationships
        builder
            .HasOne(x => x.Journal)
            .WithMany(j => j.Lines)
            .HasForeignKey(x => x.JournalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Account)
            .WithMany(a => a.JournalLines)
            .HasForeignKey(x => x.AccountNo)
            .HasPrincipalKey("AccountNo") // explicit (PK already AccountNo)
            .OnDelete(DeleteBehavior.Restrict);

        // Data integrity
        builder.ToTable(t =>
        {
            // positive line number
            t.HasCheckConstraint("ck_gjl_lineno_positive", "\"LineNo\" > 0");

            // nonnegative + exactly one side > 0 (no zero rows, no double-sided)
            t.HasCheckConstraint(
                "ck_gjl_one_sided_amount",
                "(\"Debit\" >= 0) AND (\"Credit\" >= 0) AND "
                    + "((\"Debit\" = 0 AND \"Credit\" > 0) OR (\"Credit\" = 0 AND \"Debit\" > 0))"
            );
        });
    }
}