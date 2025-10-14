using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Accounts_Receivable;

namespace OakERP.Infrastructure.Persistence.Configurations.Accounts_Recievable;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        // Columns
        builder.Property(c => c.CustomerCode).HasMaxLength(40).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();

        builder.Property(c => c.Phone).HasMaxLength(40);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Address).HasMaxLength(512);
        builder.Property(c => c.TaxNumber).HasMaxLength(40);

        builder.Property(c => c.TermsDays).IsRequired();
        builder.Property(c => c.CreditLimit).HasColumnType("numeric(18,2)");

        // Timestamps
        builder
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd();

        builder
            .Property(c => c.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAddOrUpdate();

        // Indexes
        builder.HasIndex(c => c.CustomerCode).IsUnique();
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.IsActive);

        // Optional uniques (filtered so NULLs are allowed)
        builder.HasIndex(c => c.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");

        builder.HasIndex(c => c.TaxNumber).IsUnique().HasFilter("\"TaxNumber\" IS NOT NULL");

        // Data integrity
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_customer_code_not_blank", "btrim(\"CustomerCode\") <> ''");
            t.HasCheckConstraint("ck_customer_name_not_blank", "btrim(\"Name\") <> ''");

            // sane terms window (adjust to your policy)
            t.HasCheckConstraint("ck_customer_termsdays_range", "\"TermsDays\" BETWEEN 0 AND 180");

            // credit limit must be null or >= 0
            t.HasCheckConstraint(
                "ck_customer_creditlimit_nonneg",
                "\"CreditLimit\" IS NULL OR \"CreditLimit\" >= 0"
            );

            // ultra-light email shape check (optional)
            t.HasCheckConstraint(
                "ck_customer_email_basic_shape",
                "\"Email\" IS NULL OR (position('@' in \"Email\") > 1 AND position('.' in \"Email\") > 3)"
            );
        });
    }
}