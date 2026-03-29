using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Payable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Payable;

internal class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendors");

        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(v => v.VendorCode).HasMaxLength(40).IsRequired();
        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();

        builder.Property(v => v.Phone).HasMaxLength(40);
        builder.Property(v => v.Email).HasMaxLength(256);
        builder.Property(v => v.Address).HasMaxLength(512);
        builder.Property(v => v.TaxNumber).HasMaxLength(40);

        // Timestamps
        builder
            .Property(v => v.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(v => v.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(v => v.TermsDays).IsRequired();

        // Indexes
        builder.HasIndex(v => v.VendorCode).IsUnique();
        builder.HasIndex(v => v.Name);
        builder.HasIndex(v => v.IsActive);

        builder.HasIndex(v => v.Email).IsUnique().HasFilter("\"email\" IS NOT NULL");

        builder.HasIndex(v => v.TaxNumber).IsUnique().HasFilter("\"tax_number\" IS NOT NULL");

        // Data integrity
        builder.ToTable(t =>
        {
            // no empty/whitespace vendor codes or names
            t.HasCheckConstraint("ck_vendor_code_not_blank", "btrim(\"vendor_code\") <> ''");
            t.HasCheckConstraint("ck_vendor_name_not_blank", "btrim(\"name\") <> ''");

            // sane terms range (tweak to preference)
            t.HasCheckConstraint("ck_vendor_termsdays_range", "\"terms_days\" BETWEEN 0 AND 180");

            // ultra-light email shape check (optional; not a full regex)
            t.HasCheckConstraint(
                "ck_vendor_email_basic_shape",
                "\"email\" IS NULL OR (position('@' in \"email\") > 1 AND position('.' in \"email\") > 3)"
            );
        });
    }
}
