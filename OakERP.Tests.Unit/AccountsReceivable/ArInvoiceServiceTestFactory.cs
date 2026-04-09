using Microsoft.Extensions.Logging;
using Moq;
using OakERP.Application.AccountsReceivable.Invoices.Support;
using OakERP.Application.Common.Orchestration;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.Inventory;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArInvoiceServiceTestFactory
{
    public Mock<IArInvoiceRepository> ArInvoiceRepository { get; } = new(MockBehavior.Strict);
    public Mock<ICustomerRepository> CustomerRepository { get; } = new(MockBehavior.Strict);
    public Mock<ICurrencyRepository> CurrencyRepository { get; } = new(MockBehavior.Strict);
    public Mock<IGlAccountRepository> GlAccountRepository { get; } = new(MockBehavior.Strict);
    public Mock<IItemRepository> ItemRepository { get; } = new(MockBehavior.Strict);
    public Mock<ILocationRepository> LocationRepository { get; } = new(MockBehavior.Strict);
    public Mock<ITaxRateRepository> TaxRateRepository { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<IPersistenceFailureClassifier> PersistenceFailureClassifier { get; } =
        new(MockBehavior.Strict);
    public Mock<IClock> Clock { get; } = new(MockBehavior.Strict);
    public Mock<ILogger<ArInvoiceService>> Logger { get; } = new();

    public ArInvoiceServiceTestFactory()
    {
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
            .Setup(x => x.IsArInvoiceDocNoConflict(It.IsAny<Exception>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsArReceiptDocNoConflict(It.IsAny<Exception>()))
            .Returns(false);
        PersistenceFailureClassifier
            .Setup(x => x.IsConcurrencyConflict(It.IsAny<Exception>()))
            .Returns(false);
        Clock.SetupGet(x => x.UtcNow).Returns(UtcAtHourDaysFromToday(0));
    }

    public ArInvoiceService CreateService() =>
        new(
            new ArInvoiceCreateDependencies(
                ArInvoiceRepository.Object,
                CustomerRepository.Object,
                CurrencyRepository.Object,
                GlAccountRepository.Object,
                ItemRepository.Object,
                LocationRepository.Object,
                TaxRateRepository.Object
            ),
            new InvoiceCreateWorkflowDependencies(
                UnitOfWork.Object,
                PersistenceFailureClassifier.Object,
                Clock.Object
            ),
            Logger.Object
        );

    public static Customer CreateCustomer(
        Guid? id = null,
        bool isActive = true,
        int termsDays = 30
    ) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            CustomerCode = "CUST-001",
            Name = "Acme Customer",
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

    public static GlAccount CreateRevenueAccount(string accountNo = "4000", bool isActive = true) =>
        new()
        {
            AccountNo = accountNo,
            Name = "Revenue",
            Type = GlAccountType.Revenue,
            IsActive = isActive,
        };

    public static Item CreateItem(
        Guid? id = null,
        bool isActive = true,
        ItemType itemType = ItemType.Stock
    ) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Sku = "SKU-001",
            Name = "Stock Item",
            Type = itemType,
            IsActive = isActive,
        };

    public static Location CreateLocation(Guid? id = null, bool isActive = true) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Code = "MAIN",
            Name = "Main Warehouse",
            IsActive = isActive,
        };

    public static TaxRate CreateTaxRate(
        Guid? id = null,
        bool isActive = true,
        bool isInput = false
    ) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = isInput ? "Input VAT 15%" : "Output VAT 15%",
            RatePercent = 15m,
            IsActive = isActive,
            IsInput = isInput,
            EffectiveFrom = DaysFromToday(-100),
        };

    public static CreateArInvoiceCommand CreateCommand(
        Guid customerId,
        Guid itemId,
        Guid locationId,
        Guid taxRateId,
        string serviceRevenueAccount = "4000"
    ) =>
        new()
        {
            DocNo = "ARINV-3001",
            CustomerId = customerId,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            ShipTo = "Customer site",
            Memo = "Mixed invoice",
            TaxTotal = 15m,
            DocTotal = 165m,
            PerformedBy = "unit-user",
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "Consulting services",
                    RevenueAccount = serviceRevenueAccount,
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
                new ArInvoiceLineInputDto
                {
                    Description = "Stock item",
                    ItemId = itemId,
                    Qty = 1m,
                    UnitPrice = 100m,
                    TaxRateId = taxRateId,
                    LocationId = locationId,
                    LineTotal = 100m,
                },
            ],
        };
}
