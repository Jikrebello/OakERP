using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OakERP.Auth.Identity;
using OakERP.Common.Persistence;
using OakERP.Domain.Entities.Users;
using OakERP.Infrastructure.Persistence.Seeding.Base;

namespace OakERP.Infrastructure.Persistence.Seeding.Accounts;

public class RoleAndAdminSeeder(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    IConfiguration config
) : BaseSeeder
{
    public override int Order => 1;
    protected override bool RunInProduction => true;

    public override async Task SeedAsync()
    {
        // Roles
        foreach (var role in UserRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Tenant
        var tenant = db.Tenants.FirstOrDefault(t => t.Name == "SystemTenant");
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = "SystemTenant",
                License = new License
                {
                    Key = Guid.NewGuid().ToString("N"),
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                },
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        // Admin user
        var email =
            config["Seed:AdminEmail"]
            ?? throw new InvalidOperationException("Seed:AdminEmail is not configured.");
        var pass =
            config["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword is not configured.");

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Email = email,
                UserName = email,
                TenantId = tenant.Id,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
            };

            var result = await userManager.CreateAsync(admin, pass);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to seed admin user: " + result.Errors.First().Description
                );
            }

            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
