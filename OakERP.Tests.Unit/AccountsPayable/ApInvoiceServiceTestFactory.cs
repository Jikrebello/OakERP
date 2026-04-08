using Microsoft.Extensions.Logging;
using Moq;
using OakERP.Application.AccountsPayable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Common;
using OakERP.Domain.Repository_Interfaces.General_Ledger;

namespace OakERP.Tests.Unit.AccountsPayable;

public sealed class ApInvoiceServiceTestFactory
{
    public Mock<IApInvoiceRepository> ApInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<IVendorRepository> VendorRepository { get; } = new(MockBehavior.Strict);
    public Mock<ICurrencyRepository> CurrencyRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlAccountRepository> GlAccountRepository { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<IPersistenceFailureClassifier> PersistenceFailureClassifier { get; } =
        new(MockBehavior.Strict);
    public Mock<ILogger<ApInvoiceService>> Logger { get; } = new();

    public ApInvoiceServiceTestFactory()
    {
        UnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
        PersistenceFailureClassifier
            .Setup(x => x.IsUniqueConstraint(It.IsAny<Exception>(), It.IsAny<string>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsConcurrencyConflict(It.IsAny<Exception>()))
            .Returns(false);
    }

    public ApInvoiceService CreateService() =>
        new(
            ApInvoiceRepository.Object,
            VendorRepository.Object,
            CurrencyRepository.Object,
            GlAccountRepository.Object,
            UnitOfWork.Object,
            PersistenceFailureClassifier.Object,
            Logger.Object
        );

    public static Vendor CreateVendor(Guid? id = null, bool isActive = true, int termsDays = 30) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            VendorCode = "VEND-001",
            Name = "Acme Vendor",
            IsActive = isActive,
            TermsDays = termsDays,
        };

    public static Currency CreateCurrency(string code = "ZAR", bool isActive = true) =>
        new()
        {
            Code = code,
            NumericCode = 710,
            Name = code,
            IsActive = isActive,
        };

    public static GlAccount CreateGlAccount(string accountNo = "5000", bool isActive = true) =>
        new()
        {
            AccountNo = accountNo,
            Name = "Expense",
            Type = OakERP.Common.Enums.GlAccountType.Expense,
            IsActive = isActive,
        };

    public static CreateApInvoiceCommand CreateCommand(
        Guid vendorId,
        string accountNo = "5000",
        decimal taxTotal = 0m,
        decimal docTotal = 100m
    ) =>
        new()
        {
            DocNo = "APINV-1001",
            VendorId = vendorId,
            InvoiceNo = "VEN-INV-001",
            InvoiceDate = new DateOnly(2026, 4, 5),
            CurrencyCode = "ZAR",
            TaxTotal = taxTotal,
            DocTotal = docTotal,
            PerformedBy = "unit-user",
            Lines =
            [
                new ApInvoiceLineInputDto
                {
                    Description = "Office rent",
                    AccountNo = accountNo,
                    Qty = 1m,
                    UnitPrice = docTotal - taxTotal,
                    LineTotal = docTotal - taxTotal,
                },
            ],
        };
}
