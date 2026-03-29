using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Inventory;
using OakERP.Infrastructure.Posting;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingServiceTestFactory
{
    public Mock<IArInvoiceRepository> ArInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<IFiscalPeriodRepository> FiscalPeriodRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlAccountRepository> GlAccountRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlEntryRepository> GlEntryRepository { get; } = new(MockBehavior.Strict);
    public Mock<IInventoryLedgerRepository> InventoryLedgerRepository { get; } =
        new(MockBehavior.Strict);
    public Mock<IGlSettingsProvider> GlSettingsProvider { get; } = new(MockBehavior.Strict);
    public Mock<IPostingRuleProvider> PostingRuleProvider { get; } = new(MockBehavior.Strict);
    public Mock<IArInvoicePostingContextBuilder> PostingContextBuilder { get; } =
        new(MockBehavior.Strict);
    public Mock<IPostingEngine> PostingEngine { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);

    public PostingServiceTestFactory()
    {
        UnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
    }

    public PostingService CreateService() =>
        new(
            ArInvoiceRepository.Object,
            FiscalPeriodRepository.Object,
            GlAccountRepository.Object,
            GlEntryRepository.Object,
            InventoryLedgerRepository.Object,
            GlSettingsProvider.Object,
            PostingRuleProvider.Object,
            PostingContextBuilder.Object,
            PostingEngine.Object,
            UnitOfWork.Object
        );

    public static GlPostingSettings CreateSettings() =>
        new(
            BaseCurrencyCode: "ZAR",
            ArControlAccountNo: "1100",
            ApControlAccountNo: "2000",
            DefaultRevenueAccountNo: "4000",
            DefaultExpenseAccountNo: "5000",
            DefaultInventoryAssetAccountNo: "1300",
            DefaultCogsAccountNo: "5100",
            DefaultTaxOutputAccountNo: "2100",
            DefaultTaxInputAccountNo: "2200"
        );

    public static FiscalPeriod CreateOpenPeriod() =>
        new()
        {
            Id = Guid.NewGuid(),
            FiscalYear = 2026,
            PeriodNo = 3,
            PeriodStart = new DateOnly(2026, 3, 1),
            PeriodEnd = new DateOnly(2026, 3, 31),
            Status = "open",
        };

    public static PostingRule CreateRule() =>
        new()
        {
            Name = "AR Invoice Slice 1A",
            Lines =
            [
                new PostingRuleLine
                {
                    AccountKey = AccountKey.AccountsReceivable,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = "Header",
                    Side = RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.Revenue,
                    AmountSource = AmountSource.LineNet,
                    Scope = "Line",
                    Side = RuleSide.Credit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.TaxOutput,
                    AmountSource = AmountSource.HeaderTaxTotal,
                    Scope = "Tax",
                    Side = RuleSide.Credit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.Cogs,
                    AmountSource = AmountSource.LineCogsValue,
                    Scope = "Line.Stock",
                    Side = RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.InventoryAsset,
                    AmountSource = AmountSource.LineCogsValue,
                    Scope = "Line.Stock",
                    Side = RuleSide.Credit,
                },
            ],
        };

    public static ArInvoice CreateInvoice() =>
        new()
        {
            Id = Guid.NewGuid(),
            DocNo = "AR-1001",
            CustomerId = Guid.NewGuid(),
            InvoiceDate = new DateOnly(2026, 3, 15),
            DueDate = new DateOnly(2026, 4, 14),
            CurrencyCode = "ZAR",
            DocTotal = 115m,
            TaxTotal = 15m,
            DocStatus = DocStatus.Draft,
            Lines =
            [
                new ArInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    LineNo = 1,
                    LineTotal = 100m,
                    Qty = 1m,
                    UnitPrice = 100m,
                    RevenueAccount = "4000",
                },
            ],
        };

    public static ArInvoice CreateStockInvoice(bool includeLocation = true)
    {
        Guid itemId = Guid.NewGuid();

        return new ArInvoice
        {
            Id = Guid.NewGuid(),
            DocNo = "AR-STK-1001",
            CustomerId = Guid.NewGuid(),
            InvoiceDate = new DateOnly(2026, 3, 15),
            DueDate = new DateOnly(2026, 4, 14),
            CurrencyCode = "ZAR",
            DocTotal = 115m,
            TaxTotal = 15m,
            DocStatus = DocStatus.Draft,
            Lines =
            [
                new ArInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    LineNo = 1,
                    ItemId = itemId,
                    Item = new Item
                    {
                        Id = itemId,
                        Type = ItemType.Stock,
                        DefaultRevenueAccountNo = "4000",
                        Category = new ItemCategory
                        {
                            RevenueAccount = "4000",
                            CogsAccount = "5100",
                            InventoryAccount = "1300",
                        },
                    },
                    LocationId = includeLocation ? Guid.NewGuid() : null,
                    Qty = 1m,
                    UnitPrice = 100m,
                    RevenueAccount = "4000",
                    LineTotal = 100m,
                },
            ],
        };
    }

    public static ArInvoicePostingContext CreatePostingContext(
        ArInvoice invoice,
        PostingRule? rule = null
    )
    {
        var settings = CreateSettings();
        var period = CreateOpenPeriod();
        var lines = invoice
            .Lines.OrderBy(x => x.LineNo)
            .Select(line =>
            {
                bool isStock = line.Item?.Type == ItemType.Stock;
                decimal? unitCost = isStock ? 12.3456m : null;
                decimal? lineCogsValue = isStock
                    ? Math.Round(line.Qty * unitCost!.Value, 2, MidpointRounding.AwayFromZero)
                    : null;

                return new ArInvoicePostingLineContext(
                    line,
                    isStock,
                    line.RevenueAccount ?? settings.DefaultRevenueAccountNo,
                    line.LocationId,
                    isStock
                        ? line.Item?.Category?.CogsAccount ?? settings.DefaultCogsAccountNo
                        : null,
                    isStock
                        ? line.Item?.Category?.InventoryAccount
                            ?? settings.DefaultInventoryAssetAccountNo
                        : null,
                    unitCost,
                    lineCogsValue
                );
            })
            .ToList();

        return new ArInvoicePostingContext(
            invoice,
            lines,
            invoice.InvoiceDate,
            period,
            settings.BaseCurrencyCode,
            1m,
            settings,
            rule ?? CreateRule()
        );
    }
}
