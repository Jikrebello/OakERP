using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence.Seeding;

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
        string[] roles = ["Admin", "User"];
        foreach (var role in roles)
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
        var email = config["Seed:AdminEmail"] ?? "admin@oak.local";
        var pass = config["Seed:AdminPassword"] ?? "admin123";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Email = email,
                UserName = email,
                TenantId = tenant.Id,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(admin, pass);
            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to seed admin user: " + result.Errors.First().Description
                );
            }

            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}