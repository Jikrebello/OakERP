using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using OakERP.Application.AccountsReceivable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Posting.Inventory;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Bank;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Inventory;
using OakERP.Domain.Repository_Interfaces.Users;
using OakERP.Infrastructure.Accounts_Receivable;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding.Base;
using OakERP.Infrastructure.Posting;
using OakERP.Infrastructure.Posting.Accounts_Receivable;
using OakERP.Infrastructure.Posting.General_Ledger;
using OakERP.Infrastructure.Posting.Inventory;
using OakERP.Infrastructure.Repositories.Accounts_Receivable;
using OakERP.Infrastructure.Repositories.Bank;
using OakERP.Infrastructure.Repositories.General_Ledger;
using OakERP.Infrastructure.Repositories.Inventory;
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
                    npgsql
                        .MapEnum<DocStatus>("doc_status")
                        .MapEnum<GlAccountType>("gl_account_type")
                        .MapEnum<ItemType>("item_type")
                        .MapEnum<InventoryTransactionType>("inventory_transaction_type");
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
        services.AddScoped<IArInvoiceRepository, ArInvoiceRepository>();
        services.AddScoped<IArReceiptRepository, ArReceiptRepository>();
        services.AddScoped<IArReceiptAllocationRepository, ArReceiptAllocationRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IFiscalPeriodRepository, FiscalPeriodRepository>();
        services.AddScoped<IGlAccountRepository, GlAccountRepository>();
        services.AddScoped<IGlEntryRepository, GlEntryRepository>();
        services.AddScoped<IInventoryLedgerRepository, InventoryLedgerRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
        return services;
    }

    public static IServiceCollection AddAccountsReceivableServices(this IServiceCollection services)
    {
        services.AddScoped<IArReceiptService, ArReceiptService>();

        return services;
    }

    public static IServiceCollection AddPostingServices(this IServiceCollection services)
    {
        services.AddScoped<IPostingService, PostingService>();
        services.AddScoped<IPostingEngine, ArInvoicePostingEngine>();
        services.AddScoped<IPostingRuleProvider, ArInvoicePostingRuleProvider>();
        services.AddScoped<IGlSettingsProvider, AppSettingGlSettingsProvider>();
        services.AddScoped<IArInvoicePostingContextBuilder, ArInvoicePostingContextBuilder>();
        services.AddScoped<IInventoryCostService, MovingAverageInventoryCostService>();

        return services;
    }
}
