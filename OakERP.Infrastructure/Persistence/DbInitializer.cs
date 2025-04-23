using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OakERP.Domain.Entities;

namespace OakERP.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration
    )
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Ensure role exists
        string[] roles = ["Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Create default tenant
        var tenant = db.Tenants.FirstOrDefault(t => t.Name == "SystemTenant");
        if (tenant is null)
        {
            tenant = new Tenant { Name = "SystemTenant" };

            if (tenant.License is null)
            {
                var license = new License
                {
                    Key = Guid.NewGuid().ToString("N"),
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    TenantId = tenant.Id,
                };

                db.Licenses.Add(license);
                await db.SaveChangesAsync();
            }

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        // 3. Create admin user
        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@oak.local";
        var adminPass = configuration["Seed:AdminPassword"] ?? "admin123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                TenantId = tenant.Id,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(adminUser, adminPass);
            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to seed admin user: " + result.Errors.First().Description
                );
            }

            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}