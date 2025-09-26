using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Infrastructure.Persistence.Configurations.General_Ledger;

public class GlAccountConfiguration : IEntityTypeConfiguration<GlAccount>
{
    public void Configure(EntityTypeBuilder<GlAccount> builder)
    {
        builder.ToTable("gl_accounts");

        builder
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentAccount)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
