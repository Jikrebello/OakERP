using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Entities.Users;
using OakERP.Infrastructure.Persistence.Seeding.Views.Accounts_Payable;
using OakERP.Infrastructure.Persistence.Seeding.Views.Accounts_Recievable;
using OakERP.Infrastructure.Persistence.Seeding.Views.Inventory;

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
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    #region Accounts Payable

    public DbSet<ApInvoice> ApInvoices => Set<ApInvoice>();
    public DbSet<ApInvoiceLine> ApInvoiceLines => Set<ApInvoiceLine>();
    public DbSet<ApPayment> ApPayments => Set<ApPayment>();
    public DbSet<ApPaymentAllocation> ApPaymentAllocations => Set<ApPaymentAllocation>();
    public DbSet<Vendor> Vendors => Set<Vendor>();

    #endregion Accounts Payable

    #region Accounts Receivable

    public DbSet<ArInvoice> ArInvoices => Set<ArInvoice>();
    public DbSet<ArInvoiceLine> ArInvoiceLines => Set<ArInvoiceLine>();
    public DbSet<ArReceipt> ArReceipts => Set<ArReceipt>();
    public DbSet<ArReceiptAllocation> ArReceiptAllocations => Set<ArReceiptAllocation>();
    public DbSet<Customer> Customers => Set<Customer>();

    #endregion Accounts Receivable

    #region Bank

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

    #endregion Bank

    #region General Ledger

    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<GlAccount> GlAccounts => Set<GlAccount>();
    public DbSet<GlEntry> GlEntries => Set<GlEntry>();
    public DbSet<GlJournal> GlJournals => Set<GlJournal>();
    public DbSet<GlJournalLine> GlJournalLines => Set<GlJournalLine>();

    #endregion General Ledger

    #region Inventory

    public DbSet<InventoryLedger> InventoryLedgers => Set<InventoryLedger>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockCountLine> StockCountLines => Set<StockCountLine>();

    #endregion Inventory

    #region Users

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<License> Licenses => Set<License>();

    #endregion Users

    #region Views

    public DbSet<APOpenItemView> APOpenItems => Set<APOpenItemView>();
    public DbSet<AROpenItemView> AROpenItems => Set<AROpenItemView>();
    public DbSet<ItemBalanceView> ItemBalances => Set<ItemBalanceView>();

    #endregion Views

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // PostgreSQL enums
        builder.HasPostgresEnum<GlAccountType>();
        builder.HasPostgresEnum<DocStatus>();
        builder.HasPostgresEnum<ItemType>();
        builder.HasPostgresEnum<InventoryTransactionType>();

        // Apply all IEntityTypeConfiguration<> in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Database views
        builder.Entity<ItemBalanceView>().ToView("public.v_item_balance").HasNoKey();
        builder.Entity<AROpenItemView>().ToView("public.v_ar_open_items").HasNoKey();
        builder.Entity<APOpenItemView>().ToView("public.v_ap_open_items").HasNoKey();

        foreach (
            var prop in builder
                .Model.GetEntityTypes()
                .SelectMany(et => et.GetProperties())
                .Where(p => p.ClrType == typeof(DateOnly))
        )
        {
            prop.SetColumnType("date");
        }

        // Add xmin optimistic concurrency to all table-backed, key (i.e., updatable) entities
        foreach (var et in builder.Model.GetEntityTypes())
        {
            if (et.IsKeyless)
                continue;
            if (et.GetTableName() is null)
                continue;

            builder
                .Entity(et.ClrType)
                .Property<uint>("xmin")
                .HasColumnName("xmin")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}