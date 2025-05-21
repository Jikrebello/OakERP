using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the application, providing access to the application's data models and managing
/// database interactions.
/// </summary>
/// <remarks>This context is derived from <see cref="IdentityDbContext{TUser}"/> and includes additional DbSet
/// properties for application-specific entities. It also applies
/// entity configurations from the assembly containing the <see cref="ApplicationDbContext"/> type.</remarks>
/// <param name="options"></param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<License> Licenses => Set<License>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}