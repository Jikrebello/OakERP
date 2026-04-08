using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Payable;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Inventory;
using OakERP.Infrastructure.Posting;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingServiceTestFactory
{
    public Mock<IApPaymentRepository> ApPaymentRepository { get; } = new(MockBehavior.Strict);
    public Mock<IApInvoiceRepository> ApInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<IArInvoiceRepository> ArInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<IArReceiptRepository> ArReceiptRepository { get; } = new(MockBehavior.Strict);
    public Mock<IFiscalPeriodRepository> FiscalPeriodRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlAccountRepository> GlAccountRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlEntryRepository> GlEntryRepository { get; } = new(MockBehavior.Strict);
    public Mock<IInventoryLedgerRepository> InventoryLedgerRepository { get; } =
        new(MockBehavior.Strict);
    public Mock<IGlSettingsProvider> GlSettingsProvider { get; } = new(MockBehavior.Strict);
    public Mock<IPostingRuleProvider> PostingRuleProvider { get; } = new(MockBehavior.Strict);
    public Mock<IApPaymentPostingContextBuilder> ApPaymentPostingContextBuilder { get; } =
        new(MockBehavior.Strict);
    public Mock<IApInvoicePostingContextBuilder> ApInvoicePostingContextBuilder { get; } =
        new(MockBehavior.Strict);
    public Mock<IArInvoicePostingContextBuilder> PostingContextBuilder { get; } =
        new(MockBehavior.Strict);
    public Mock<IArReceiptPostingContextBuilder> ReceiptPostingContextBuilder { get; } =
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
            new PostingSourceRepositories(
                ApPaymentRepository.Object,
                ApInvoiceRepository.Object,
                ArInvoiceRepository.Object,
                ArReceiptRepository.Object
            ),
            new PostingPersistenceDependencies(
                FiscalPeriodRepository.Object,
                GlAccountRepository.Object,
                GlEntryRepository.Object,
                InventoryLedgerRepository.Object,
                UnitOfWork.Object
            ),
            new PostingRuntimeDependencies(
                GlSettingsProvider.Object,
                PostingRuleProvider.Object,
                PostingEngine.Object
            ),
            new PostingContextBuilders(
                ApPaymentPostingContextBuilder.Object,
                ApInvoicePostingContextBuilder.Object,
                PostingContextBuilder.Object,
                ReceiptPostingContextBuilder.Object
            )
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
            Status = FiscalPeriodStatuses.Open,
        };

    public static PostingRule CreateRule() =>
        new()
        {
            Name = "AR Invoice Runtime Rule",
            Lines =
            [
                new PostingRuleLine
                {
                    AccountKey = AccountKey.AccountsReceivable,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                    Side = RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.Revenue,
                    AmountSource = AmountSource.LineNet,
                    Scope = PostingRuleScopes.Line,
                    Side = RuleSide.Credit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.TaxOutput,
                    AmountSource = AmountSource.HeaderTaxTotal,
                    Scope = PostingRuleScopes.Tax,
                    Side = RuleSide.Credit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.Cogs,
                    AmountSource = AmountSource.LineCogsValue,
                    Scope = PostingRuleScopes.LineStock,
                    Side = RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.InventoryAsset,
                    AmountSource = AmountSource.LineCogsValue,
                    Scope = PostingRuleScopes.LineStock,
                    Side = RuleSide.Credit,
                },
            ],
        };

    public static PostingRule CreateReceiptRule() =>
        new()
        {
            Name = "AR Receipt Runtime Rule",
            Lines =
            [
                new PostingRuleLine
                {
                    AccountKey = AccountKey.Bank,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                    Side = RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.AccountsReceivable,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                    Side = RuleSide.Credit,
                },
            ],
        };

    public static PostingRule CreateApInvoiceRule() =>
        new()
        {
            Name = "AP Invoice Runtime Rule",
            Lines =
            [
                new PostingRuleLine
                {
                    AccountKey = AccountKey.AccountsPayable,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                    Side = RuleSide.Credit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.Expense,
                    AmountSource = AmountSource.LineNet,
                    Scope = PostingRuleScopes.Line,
                    Side = RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.TaxInput,
                    AmountSource = AmountSource.HeaderTaxTotal,
                    Scope = PostingRuleScopes.Tax,
                    Side = RuleSide.Debit,
                },
            ],
        };

    public static PostingRule CreateApPaymentRule() =>
        new()
        {
            Name = "AP Payment Runtime Rule",
            Lines =
            [
                new PostingRuleLine
                {
                    AccountKey = AccountKey.Bank,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
                    Side = RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = AccountKey.AccountsPayable,
                    AmountSource = AmountSource.HeaderDocTotal,
                    Scope = PostingRuleScopes.Header,
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

    public static ArReceipt CreateReceipt(
        decimal amount = 100m,
        decimal allocatedAmount = 0m,
        string currencyCode = "ZAR",
        bool bankAccountActive = true
    )
    {
        var receipt = new ArReceipt
        {
            Id = Guid.NewGuid(),
            DocNo = "RCPT-1001",
            CustomerId = Guid.NewGuid(),
            BankAccountId = Guid.NewGuid(),
            ReceiptDate = new DateOnly(2026, 3, 20),
            Amount = amount,
            CurrencyCode = currencyCode,
            DocStatus = DocStatus.Draft,
            BankAccount = new BankAccount
            {
                Id = Guid.NewGuid(),
                Name = "Main Bank",
                GlAccountNo = "1000",
                CurrencyCode = currencyCode,
                IsActive = bankAccountActive,
            },
        };

        if (allocatedAmount > 0m)
        {
            receipt.Allocations.Add(
                new ArReceiptAllocation
                {
                    ArReceiptId = receipt.Id,
                    ArInvoiceId = Guid.NewGuid(),
                    AllocationDate = receipt.ReceiptDate,
                    AmountApplied = allocatedAmount,
                }
            );
        }

        return receipt;
    }

    public static ApInvoice CreateApInvoice(decimal taxTotal = 15m)
    {
        decimal expenseTotal = 100m;

        return new ApInvoice
        {
            Id = Guid.NewGuid(),
            DocNo = "AP-1001",
            VendorId = Guid.NewGuid(),
            InvoiceNo = "SUP-1001",
            InvoiceDate = new DateOnly(2026, 4, 5),
            DueDate = new DateOnly(2026, 5, 5),
            CurrencyCode = "ZAR",
            TaxTotal = taxTotal,
            DocTotal = expenseTotal + taxTotal,
            DocStatus = DocStatus.Draft,
            Lines =
            [
                new ApInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    LineNo = 2,
                    AccountNo = "5100",
                    Qty = 1m,
                    UnitPrice = 40m,
                    LineTotal = 40m,
                },
                new ApInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    LineNo = 1,
                    AccountNo = "5000",
                    Qty = 1m,
                    UnitPrice = 60m,
                    LineTotal = 60m,
                },
            ],
        };
    }

    public static ApPayment CreateApPayment(
        decimal amount = 100m,
        decimal allocatedAmount = 0m,
        string bankCurrencyCode = "ZAR",
        bool bankAccountActive = true
    )
    {
        var payment = new ApPayment
        {
            Id = Guid.NewGuid(),
            DocNo = "APPAY-1001",
            VendorId = Guid.NewGuid(),
            BankAccountId = Guid.NewGuid(),
            PaymentDate = new DateOnly(2026, 4, 20),
            Amount = amount,
            DocStatus = DocStatus.Draft,
            BankAccount = new BankAccount
            {
                Id = Guid.NewGuid(),
                Name = "Main Bank",
                GlAccountNo = "1000",
                CurrencyCode = bankCurrencyCode,
                IsActive = bankAccountActive,
            },
        };

        if (allocatedAmount > 0m)
        {
            payment.Allocations.Add(
                new ApPaymentAllocation
                {
                    ApPaymentId = payment.Id,
                    ApInvoiceId = Guid.NewGuid(),
                    AllocationDate = payment.PaymentDate,
                    AmountApplied = allocatedAmount,
                }
            );
        }

        return payment;
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

    public static ArReceiptPostingContext CreateReceiptPostingContext(
        ArReceipt receipt,
        PostingRule? rule = null
    )
    {
        var settings = CreateSettings();
        var period = CreateOpenPeriod();

        return new ArReceiptPostingContext(
            receipt,
            receipt.ReceiptDate,
            period,
            settings,
            rule ?? CreateReceiptRule(),
            receipt.BankAccount.GlAccountNo
        );
    }

    public static ApInvoicePostingContext CreateApInvoicePostingContext(
        ApInvoice invoice,
        PostingRule? rule = null
    )
    {
        var settings = CreateSettings();
        var period = CreateOpenPeriod();
        var lines = invoice
            .Lines.OrderBy(x => x.LineNo)
            .Select(line => new ApInvoicePostingLineContext(line, line.AccountNo!))
            .ToList();

        return new ApInvoicePostingContext(
            invoice,
            lines,
            invoice.InvoiceDate,
            period,
            settings,
            rule ?? CreateApInvoiceRule()
        );
    }

    public static ApPaymentPostingContext CreateApPaymentPostingContext(
        ApPayment payment,
        PostingRule? rule = null
    )
    {
        var settings = CreateSettings();
        var period = CreateOpenPeriod();

        return new ApPaymentPostingContext(
            payment,
            payment.PaymentDate,
            period,
            settings,
            rule ?? CreateApPaymentRule(),
            payment.BankAccount.GlAccountNo,
            ApSettlementCalculator.GetPaymentAllocatedAmount(payment),
            ApSettlementCalculator.GetPaymentUnappliedAmount(payment)
        );
    }
}
