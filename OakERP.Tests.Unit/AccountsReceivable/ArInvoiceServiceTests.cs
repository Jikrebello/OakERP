using Microsoft.EntityFrameworkCore;
using Moq;
using OakERP.Common.Errors;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArInvoiceServiceTests
{
    private readonly ArInvoiceServiceTestFactory _factory = new();

    [Fact]
    public async Task CreateAsync_Should_Create_Draft_Invoice_And_Default_Due_Date_With_Mixed_Lines()
    {
        var customer = ArInvoiceServiceTestFactory.CreateCustomer(termsDays: 14);
        var currency = ArInvoiceServiceTestFactory.CreateCurrency();
        var revenueAccount = ArInvoiceServiceTestFactory.CreateRevenueAccount();
        var item = ArInvoiceServiceTestFactory.CreateItem();
        var location = ArInvoiceServiceTestFactory.CreateLocation();
        var taxRate = ArInvoiceServiceTestFactory.CreateTaxRate();
        var command = ArInvoiceServiceTestFactory.CreateCommand(
            customer.Id,
            item.Id,
            location.Id,
            taxRate.Id
        );

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(revenueAccount.AccountNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(revenueAccount);
        _factory
            .ItemRepository.Setup(x =>
                x.FindNoTrackingAsync(item.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(item);
        _factory
            .LocationRepository.Setup(x =>
                x.FindNoTrackingAsync(location.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(location);
        _factory.TaxRateRepository.Setup(x => x.GetByIdAsync(taxRate.Id)).ReturnsAsync(taxRate);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.AddAsync(It.IsAny<OakERP.Domain.Entities.AccountsReceivable.ArInvoice>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeTrue();
        result.Invoice.ShouldNotBeNull();
        result.Invoice!.DueDate.ShouldBe(command.InvoiceDate.AddDays(14));
        result.Invoice.DocStatus.ShouldBe(OakERP.Common.Enums.DocStatus.Draft);
        result.Invoice.ShipTo.ShouldBe("Customer site");
        result.Invoice.Lines.Count.ShouldBe(2);
        result.Invoice.Lines[0].RevenueAccount.ShouldBe("4000");
        result.Invoice.Lines[1].ItemId.ShouldBe(item.Id);
        result.Invoice.Lines[1].LocationId.ShouldBe(location.Id);
        result.Invoice.Lines[1].TaxRateId.ShouldBe(taxRate.Id);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Inactive_Customer()
    {
        var customer = ArInvoiceServiceTestFactory.CreateCustomer(isActive: false);
        var command = ArInvoiceServiceTestFactory.CreateCommand(
            customer.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Validation);
        result.Message.ShouldContain("active customers");
        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Missing_Or_Inactive_Revenue_Account()
    {
        var customer = ArInvoiceServiceTestFactory.CreateCustomer();
        var currency = ArInvoiceServiceTestFactory.CreateCurrency();
        var item = ArInvoiceServiceTestFactory.CreateItem();
        var location = ArInvoiceServiceTestFactory.CreateLocation();
        var taxRate = ArInvoiceServiceTestFactory.CreateTaxRate();
        var command = ArInvoiceServiceTestFactory.CreateCommand(
            customer.Id,
            item.Id,
            location.Id,
            taxRate.Id,
            serviceRevenueAccount: "4999"
        );

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("4999", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((OakERP.Domain.Entities.GeneralLedger.GlAccount?)null);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Validation);
        result.Message.ShouldContain("missing or inactive");
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Input_Tax_Rate()
    {
        var customer = ArInvoiceServiceTestFactory.CreateCustomer();
        var currency = ArInvoiceServiceTestFactory.CreateCurrency();
        var revenueAccount = ArInvoiceServiceTestFactory.CreateRevenueAccount();
        var item = ArInvoiceServiceTestFactory.CreateItem();
        var location = ArInvoiceServiceTestFactory.CreateLocation();
        var taxRate = ArInvoiceServiceTestFactory.CreateTaxRate(isInput: true);
        var command = ArInvoiceServiceTestFactory.CreateCommand(
            customer.Id,
            item.Id,
            location.Id,
            taxRate.Id
        );

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(revenueAccount.AccountNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(revenueAccount);
        _factory
            .ItemRepository.Setup(x =>
                x.FindNoTrackingAsync(item.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(item);
        _factory
            .LocationRepository.Setup(x =>
                x.FindNoTrackingAsync(location.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(location);
        _factory.TaxRateRepository.Setup(x => x.GetByIdAsync(taxRate.Id)).ReturnsAsync(taxRate);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Validation);
        result.Message.ShouldContain("input tax");
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Conflict_When_DocNo_Already_Exists()
    {
        var customer = ArInvoiceServiceTestFactory.CreateCustomer();
        var currency = ArInvoiceServiceTestFactory.CreateCurrency();
        var command = ArInvoiceServiceTestFactory.CreateCommand(
            customer.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Conflict);
        result.Message.ShouldContain("already exists");
        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Conflict_For_Persistence_Race()
    {
        var customer = ArInvoiceServiceTestFactory.CreateCustomer();
        var currency = ArInvoiceServiceTestFactory.CreateCurrency();
        var revenueAccount = ArInvoiceServiceTestFactory.CreateRevenueAccount();
        var item = ArInvoiceServiceTestFactory.CreateItem();
        var location = ArInvoiceServiceTestFactory.CreateLocation();
        var taxRate = ArInvoiceServiceTestFactory.CreateTaxRate();
        var command = ArInvoiceServiceTestFactory.CreateCommand(
            customer.Id,
            item.Id,
            location.Id,
            taxRate.Id
        );
        var duplicateException = new DbUpdateException("duplicate");

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(revenueAccount.AccountNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(revenueAccount);
        _factory
            .ItemRepository.Setup(x =>
                x.FindNoTrackingAsync(item.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(item);
        _factory
            .LocationRepository.Setup(x =>
                x.FindNoTrackingAsync(location.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(location);
        _factory.TaxRateRepository.Setup(x => x.GetByIdAsync(taxRate.Id)).ReturnsAsync(taxRate);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.AddAsync(It.IsAny<OakERP.Domain.Entities.AccountsReceivable.ArInvoice>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(duplicateException);
        _factory
            .PersistenceFailureClassifier.Setup(x => x.IsArInvoiceDocNoConflict(duplicateException))
            .Returns(true);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Conflict);
        result.Message.ShouldContain("already exists");
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }
}
