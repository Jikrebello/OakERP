using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Auth.Identity;
using OakERP.Common.Enums;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsPayable;
using OakERP.Domain.Posting.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.Posting.Inventory;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Bank;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.Inventory;
using OakERP.Domain.RepositoryInterfaces.Users;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Persistence.Seeding.Base;
using OakERP.Infrastructure.Posting;
using OakERP.Infrastructure.Posting.AccountsPayable;
using OakERP.Infrastructure.Posting.AccountsReceivable;
using OakERP.Infrastructure.Posting.GeneralLedger;
using OakERP.Infrastructure.Posting.Inventory;
using OakERP.Infrastructure.Repositories.AccountsPayable;
using OakERP.Infrastructure.Repositories.AccountsReceivable;
using OakERP.Infrastructure.Repositories.Bank;
using OakERP.Infrastructure.Repositories.Common;
using OakERP.Infrastructure.Repositories.GeneralLedger;
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
        services.AddScoped<IPersistenceFailureClassifier, PersistenceFailureClassifier>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IApInvoiceRepository, ApInvoiceRepository>();
        services.AddScoped<IApPaymentRepository, ApPaymentRepository>();
        services.AddScoped<IApPaymentAllocationRepository, ApPaymentAllocationRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IArInvoiceRepository, ArInvoiceRepository>();
        services.AddScoped<IArReceiptRepository, ArReceiptRepository>();
        services.AddScoped<IArReceiptAllocationRepository, ArReceiptAllocationRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IFiscalPeriodRepository, FiscalPeriodRepository>();
        services.AddScoped<IGlAccountRepository, GlAccountRepository>();
        services.AddScoped<IGlEntryRepository, GlEntryRepository>();
        services.AddScoped<IInventoryLedgerRepository, InventoryLedgerRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
        return services;
    }

    public static IServiceCollection AddPostingServices(this IServiceCollection services)
    {
        services.AddScoped<IPostingEngine, PostingEngine>();
        services.AddScoped<IPostingRuleProvider, PostingRuleProvider>();
        services.AddScoped<IGlSettingsProvider, AppSettingGlSettingsProvider>();
        services.AddScoped<IApInvoicePostingContextBuilder, ApInvoicePostingContextBuilder>();
        services.AddScoped<IApPaymentPostingContextBuilder, ApPaymentPostingContextBuilder>();
        services.AddScoped<IArInvoicePostingContextBuilder, ArInvoicePostingContextBuilder>();
        services.AddScoped<IArReceiptPostingContextBuilder, ArReceiptPostingContextBuilder>();
        services.AddScoped<IInventoryCostService, MovingAverageInventoryCostService>();

        return services;
    }
}
