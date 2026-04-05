using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OakERP.Application.AccountsPayable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Bank;
using OakERP.Infrastructure.Accounts_Payable;

namespace OakERP.Tests.Unit.AccountsPayable;

public sealed class ApPaymentServiceTestFactory
{
    public Mock<IApPaymentRepository> ApPaymentRepository { get; } = new(MockBehavior.Strict);
    public Mock<IApPaymentAllocationRepository> ApPaymentAllocationRepository { get; } =
        new(MockBehavior.Strict);
    public Mock<IApInvoiceRepository> ApInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<IVendorRepository> VendorRepository { get; } = new(MockBehavior.Strict);
    public Mock<IBankAccountRepository> BankAccountRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlSettingsProvider> GlSettingsProvider { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<ILogger<ApPaymentService>> Logger { get; } = new();

    public ApPaymentServiceTestFactory()
    {
        GlSettingsProvider
            .Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSettings());

        ApPaymentAllocationRepository
            .Setup(x => x.AddAsync(It.IsAny<ApPaymentAllocation>()))
            .Returns(Task.CompletedTask);

        UnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
    }

    public ApPaymentService CreateService() =>
        new(
            ApPaymentRepository.Object,
            ApPaymentAllocationRepository.Object,
            ApInvoiceRepository.Object,
            VendorRepository.Object,
            BankAccountRepository.Object,
            GlSettingsProvider.Object,
            new ApPaymentCommandValidator(),
            new ApPaymentSnapshotFactory(),
            UnitOfWork.Object,
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

    public static Vendor CreateVendor(Guid? id = null, bool isActive = true) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            VendorCode = "VEND-001",
            Name = "Acme Vendor",
            TermsDays = 30,
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

    public static ApInvoice CreatePostedInvoice(
        Guid? id = null,
        Guid? vendorId = null,
        string currencyCode = "ZAR",
        decimal docTotal = 100m,
        decimal settledAmount = 0m,
        string docNo = "APINV-1001",
        DocStatus status = DocStatus.Posted
    )
    {
        var invoice = new ApInvoice
        {
            Id = id ?? Guid.NewGuid(),
            DocNo = docNo,
            VendorId = vendorId ?? Guid.NewGuid(),
            InvoiceNo = $"{docNo}-V",
            InvoiceDate = new DateOnly(2026, 4, 1),
            DueDate = new DateOnly(2026, 5, 1),
            CurrencyCode = currencyCode,
            DocTotal = docTotal,
            DocStatus = status,
        };

        if (settledAmount > 0m)
        {
            invoice.Allocations.Add(
                new ApPaymentAllocation
                {
                    ApInvoiceId = invoice.Id,
                    ApPaymentId = Guid.NewGuid(),
                    AllocationDate = new DateOnly(2026, 4, 2),
                    AmountApplied = settledAmount,
                }
            );
        }

        return invoice;
    }

    public static ApPayment CreateDraftPayment(
        Guid? id = null,
        Guid? vendorId = null,
        Guid? bankAccountId = null,
        decimal amount = 100m,
        decimal allocatedAmount = 0m,
        string bankCurrencyCode = "ZAR"
    )
    {
        var payment = new ApPayment
        {
            Id = id ?? Guid.NewGuid(),
            DocNo = "APPAY-1001",
            VendorId = vendorId ?? Guid.NewGuid(),
            BankAccountId = bankAccountId ?? Guid.NewGuid(),
            PaymentDate = new DateOnly(2026, 4, 5),
            Amount = amount,
            DocStatus = DocStatus.Draft,
            BankAccount = CreateBankAccount(bankAccountId, currencyCode: bankCurrencyCode),
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
}
