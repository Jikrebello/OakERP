using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repository_Interfaces.Users;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding.Base;
using OakERP.Infrastructure.Repositories.Users;

namespace OakERP.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        });

        return services;
    }

    public static IServiceCollection AddApplicationDb(
        this IServiceCollection services,
        IConfiguration config,
        Action<NpgsqlDbContextOptionsBuilder>? configureNpgsql = null
    )
    {
        var cs =
            config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured."
            );

        services.AddDbContext<ApplicationDbContext>(opt =>
        {
            var builder = opt.UseNpgsql(
                cs,
                npgsql =>
                {
                    configureNpgsql?.Invoke(npgsql);
                }
            );
            builder.UseSnakeCaseNamingConvention();
        });

        return services;
    }

    public static IServiceCollection AddSeedersFromAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        var seederType = typeof(ISeeder);

        var candidates = assemblies
            .Where(a => a != null)
            .Distinct()
            .SelectMany(a => a.GetTypes())
            .Where(t => seederType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .Distinct()
            .ToList();

        foreach (var type in candidates)
        {
            services.AddScoped(seederType, type);
        }

        return services;
    }

    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
        return services;
    }
}
