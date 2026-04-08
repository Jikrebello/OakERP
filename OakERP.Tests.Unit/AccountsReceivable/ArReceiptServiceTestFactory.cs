using Microsoft.Extensions.Logging;
using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Settlements.Documents;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArReceiptServiceTestFactory
{
    public Mock<IArReceiptRepository> ArReceiptRepository { get; } = new(MockBehavior.Strict);
    public Mock<IArReceiptAllocationRepository> ArReceiptAllocationRepository { get; } =
        new(MockBehavior.Strict);
    public Mock<IArInvoiceRepository> ArInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<ICustomerRepository> CustomerRepository { get; } = new(MockBehavior.Strict);
    public Mock<IBankAccountRepository> BankAccountRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlSettingsProvider> GlSettingsProvider { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<IPersistenceFailureClassifier> PersistenceFailureClassifier { get; } =
        new(MockBehavior.Strict);
    public Mock<IClock> Clock { get; } = new(MockBehavior.Strict);
    public Mock<ILogger<ArReceiptService>> Logger { get; } = new();

    public ArReceiptServiceTestFactory()
    {
        GlSettingsProvider
            .Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSettings());

        ArReceiptAllocationRepository
            .Setup(x => x.AddAsync(It.IsAny<ArReceiptAllocation>()))
            .Returns(Task.CompletedTask);

        UnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
        PersistenceFailureClassifier
            .Setup(x => x.IsUniqueConstraint(It.IsAny<Exception>(), It.IsAny<string>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsApInvoiceDocNoConflict(It.IsAny<Exception>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsApInvoiceVendorInvoiceNoConflict(It.IsAny<Exception>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsApPaymentDocNoConflict(It.IsAny<Exception>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsArReceiptDocNoConflict(It.IsAny<Exception>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsConcurrencyConflict(It.IsAny<Exception>()))
            .Returns(false);
        Clock.SetupGet(x => x.UtcNow).Returns(new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero));
    }

    public ArReceiptService CreateService() =>
        new(
            ArReceiptRepository.Object,
            ArReceiptAllocationRepository.Object,
            ArInvoiceRepository.Object,
            CustomerRepository.Object,
            BankAccountRepository.Object,
            new SettlementDocumentWorkflowDependencies(
                GlSettingsProvider.Object,
                UnitOfWork.Object,
                PersistenceFailureClassifier.Object,
                Clock.Object
            ),
            Logger.Object
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

    public static Customer CreateCustomer(Guid? id = null, bool isActive = true) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            CustomerCode = "CUST-001",
            Name = "Acme Customer",
            IsActive = isActive,
        };

    public static BankAccount CreateBankAccount(
        Guid? id = null,
        bool isActive = true,
        string currencyCode = "ZAR"
    ) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Main Bank",
            GlAccountNo = "1000",
            CurrencyCode = currencyCode,
            IsActive = isActive,
        };

    public static ArInvoice CreatePostedInvoice(
        Guid? id = null,
        Guid? customerId = null,
        string currencyCode = "ZAR",
        decimal docTotal = 100m,
        decimal settledAmount = 0m,
        string docNo = "ARINV-1001",
        DocStatus status = DocStatus.Posted
    )
    {
        var invoice = new ArInvoice
        {
            Id = id ?? Guid.NewGuid(),
            DocNo = docNo,
            CustomerId = customerId ?? Guid.NewGuid(),
            InvoiceDate = new DateOnly(2026, 4, 1),
            DueDate = new DateOnly(2026, 5, 1),
            CurrencyCode = currencyCode,
            DocTotal = docTotal,
            DocStatus = status,
        };

        if (settledAmount > 0m)
        {
            invoice.Allocations.Add(
                new ArReceiptAllocation
                {
                    ArInvoiceId = invoice.Id,
                    ArReceiptId = Guid.NewGuid(),
                    AllocationDate = new DateOnly(2026, 4, 2),
                    AmountApplied = settledAmount,
                }
            );
        }

        return invoice;
    }

    public static ArReceipt CreateDraftReceipt(
        Guid? id = null,
        Guid? customerId = null,
        decimal amount = 100m,
        decimal allocatedAmount = 0m,
        string currencyCode = "ZAR"
    )
    {
        var receipt = new ArReceipt
        {
            Id = id ?? Guid.NewGuid(),
            DocNo = "RCPT-1001",
            CustomerId = customerId ?? Guid.NewGuid(),
            BankAccountId = Guid.NewGuid(),
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = amount,
            CurrencyCode = currencyCode,
            DocStatus = DocStatus.Draft,
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
}
