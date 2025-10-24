using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Infrastructure.Persistence.Configurations.General_Ledger;

internal class GlAccountConfiguration : IEntityTypeConfiguration<GlAccount>
{
    public void Configure(EntityTypeBuilder<GlAccount> builder)
    {
        builder.ToTable("gl_accounts");

        // PK
        builder.HasKey(x => x.AccountNo);

        // Columns
        builder.Property(x => x.AccountNo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ParentAccount).HasMaxLength(20);
        builder.Property(x => x.Type).IsRequired();

        // Timestamps
        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        // Relationships
        builder
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentAccount)
            .HasPrincipalKey(x => x.AccountNo)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ParentAccount);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Name);

        // Data integrity
        builder.ToTable(t =>
        {
            // no blanks
            t.HasCheckConstraint("ck_glacct_no_not_blank", "btrim(\"AccountNo\") <> ''");
            t.HasCheckConstraint("ck_glacct_name_not_blank", "btrim(\"Name\") <> ''");

            // parent cannot equal self (prevents trivial cycle)
            t.HasCheckConstraint(
                "ck_glacct_parent_not_self",
                "\"ParentAccount\" IS NULL OR \"ParentAccount\" <> \"AccountNo\""
            );

            // constrain allowed characters for AccountNo (A–Z, 0–9, dash, dot)
            t.HasCheckConstraint(
                "ck_glacct_no_chars",
                "\"AccountNo\" ~ '^[A-Z0-9][A-Z0-9\\.-]{0,19}$'"
            );

            // force uppercase codes for consistency
            t.HasCheckConstraint("ck_glacct_no_upper", "\"AccountNo\" = upper(\"AccountNo\")");
        });
    }
}