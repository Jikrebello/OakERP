using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Posting;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Infrastructure.Posting;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingServiceTestFactory
{
    public Mock<IArInvoiceRepository> ArInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<IFiscalPeriodRepository> FiscalPeriodRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlAccountRepository> GlAccountRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlEntryRepository> GlEntryRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlSettingsProvider> GlSettingsProvider { get; } = new(MockBehavior.Strict);
    public Mock<IPostingRuleProvider> PostingRuleProvider { get; } = new(MockBehavior.Strict);
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
            GlSettingsProvider.Object,
            PostingRuleProvider.Object,
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
                    AccountKey = Common.Enums.AccountKey.AccountsReceivable,
                    AmountSource = Common.Enums.AmountSource.HeaderDocTotal,
                    Scope = "Header",
                    Side = Common.Enums.RuleSide.Debit,
                },
                new PostingRuleLine
                {
                    AccountKey = Common.Enums.AccountKey.Revenue,
                    AmountSource = Common.Enums.AmountSource.LineNet,
                    Scope = "Line",
                    Side = Common.Enums.RuleSide.Credit,
                },
                new PostingRuleLine
                {
                    AccountKey = Common.Enums.AccountKey.TaxOutput,
                    AmountSource = Common.Enums.AmountSource.HeaderTaxTotal,
                    Scope = "Tax",
                    Side = Common.Enums.RuleSide.Credit,
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
            DocStatus = Common.Enums.DocStatus.Draft,
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
}
