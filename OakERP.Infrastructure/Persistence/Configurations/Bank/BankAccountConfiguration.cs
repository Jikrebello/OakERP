using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Infrastructure.Persistence.Configurations.Bank;

internal class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OpeningBalance).HasColumnType("numeric(18,2)");

        builder
            .HasOne(x => x.GlAccount)
            .WithMany()
            .HasForeignKey(x => x.GlAccountNo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}