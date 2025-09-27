using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.Bank;

namespace OakERP.Infrastructure.Persistence.Configurations.Bank;

internal class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("bank_transactions");

        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.DrAccount)
            .WithMany()
            .HasForeignKey(x => x.DrAccountNo)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(x => x.CrAccount)
            .WithMany()
            .HasForeignKey(x => x.CrAccountNo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}