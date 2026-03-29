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

        builder.Property(c => c.IsOnHold).IsRequired();
        builder.Property(c => c.CreditHoldReason).HasMaxLength(256);
        builder.Property(c => c.CreditHoldUntil).HasColumnType("date");

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
        builder.HasIndex(c => c.IsOnHold);

        // Optional uniques (filtered so NULLs are allowed)
        builder.HasIndex(c => c.Email).IsUnique().HasFilter("\"email\" IS NOT NULL");

        builder.HasIndex(c => c.TaxNumber).IsUnique().HasFilter("\"tax_number\" IS NOT NULL");

        // Data integrity
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_customer_code_not_blank", "btrim(\"customer_code\") <> ''");
            t.HasCheckConstraint("ck_customer_name_not_blank", "btrim(\"name\") <> ''");

            // sane terms window (adjust to your policy)
            t.HasCheckConstraint("ck_customer_termsdays_range", "\"terms_days\" BETWEEN 0 AND 180");

            // credit limit must be null or >= 0
            t.HasCheckConstraint(
                "ck_customer_creditlimit_nonneg",
                "\"credit_limit\" IS NULL OR \"credit_limit\" >= 0"
            );

            // ultra-light email shape check (optional)
            t.HasCheckConstraint(
                "ck_customer_email_basic_shape",
                "\"email\" IS NULL OR (position('@' in \"email\") > 1 AND position('.' in \"email\") > 3)"
            );

            t.HasCheckConstraint(
                "ck_customer_hold_until_future",
                "\"credit_hold_until\" IS NULL OR \"credit_hold_until\" >= CURRENT_DATE"
            );
        });
    }
}
