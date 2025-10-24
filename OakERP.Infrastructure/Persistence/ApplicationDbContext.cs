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

        // 1) Register PostgreSQL enums (names = DB enum type names)
        builder.HasPostgresEnum<DocStatus>("doc_status");
        builder.HasPostgresEnum<GlAccountType>("gl_account_type");
        builder.HasPostgresEnum<ItemType>("item_type");
        builder.HasPostgresEnum<InventoryTransactionType>("inventory_transaction_type");

        // 2) Apply configurations so all properties are in the model
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // 3) Map every CLR enum property to its PostgreSQL enum column type
        void MapEnum<TEnum>(string pgType)
            where TEnum : struct, Enum
        {
            foreach (var et in builder.Model.GetEntityTypes())
            {
                foreach (var p in et.GetProperties().Where(p => p.ClrType == typeof(TEnum)))
                {
                    // Apply HasColumnType("<pg_enum_name>") to each matching property
                    builder.Entity(et.ClrType).Property(p.Name).HasColumnType(pgType);
                }
            }
        }

        MapEnum<DocStatus>("doc_status");
        MapEnum<GlAccountType>("gl_account_type");
        MapEnum<ItemType>("item_type");
        MapEnum<InventoryTransactionType>("inventory_transaction_type");

        // 4) Views
        builder.Entity<ItemBalanceView>().ToView("public.v_item_balance").HasNoKey();
        builder.Entity<AROpenItemView>().ToView("public.v_ar_open_items").HasNoKey();
        builder.Entity<APOpenItemView>().ToView("public.v_ap_open_items").HasNoKey();

        // 5) Global DateOnly -> date
        foreach (
            var prop in builder
                .Model.GetEntityTypes()
                .SelectMany(et => et.GetProperties())
                .Where(p => p.ClrType == typeof(DateOnly))
        )
        {
            prop.SetColumnType("date");
        }

        // 6) xmin concurrency token on table-backed, key entities
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