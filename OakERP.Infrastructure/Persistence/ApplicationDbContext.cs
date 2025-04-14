using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>().HasKey(t => t.Id);

        builder.Entity<Tenant>().Property(t => t.Name).IsRequired();

        builder.Entity<Tenant>().Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
