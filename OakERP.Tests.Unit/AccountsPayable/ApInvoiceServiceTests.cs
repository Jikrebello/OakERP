using System.Net;
using Moq;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsPayable;

public sealed class ApInvoiceServiceTests
{
    private readonly ApInvoiceServiceTestFactory _factory = new();

    [Fact]
    public async Task CreateAsync_Should_Create_Draft_Invoice_And_Default_Due_Date()
    {
        var vendor = ApInvoiceServiceTestFactory.CreateVendor(termsDays: 14);
        var currency = ApInvoiceServiceTestFactory.CreateCurrency();
        var account = ApInvoiceServiceTestFactory.CreateGlAccount();
        var command = new CreateApInvoiceCommand
        {
            DocNo = "APINV-3001",
            VendorId = vendor.Id,
            InvoiceNo = "VEN-3001",
            InvoiceDate = new DateOnly(2026, 4, 5),
            CurrencyCode = "ZAR",
            TaxTotal = 15m,
            DocTotal = 115m,
            PerformedBy = "unit-user",
            Lines =
            [
                new()
                {
                    Description = "Rent",
                    AccountNo = account.AccountNo,
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
                new()
                {
                    Description = "Utilities",
                    AccountNo = account.AccountNo,
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
            ],
        };

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.ExistsVendorInvoiceNoAsync(
                    vendor.Id,
                    command.InvoiceNo,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(account.AccountNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(account);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.AddAsync(It.IsAny<OakERP.Domain.Entities.AccountsPayable.ApInvoice>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeTrue();
        result.Invoice.ShouldNotBeNull();
        result.Invoice!.DocStatus.ShouldBe(OakERP.Common.Enums.DocStatus.Draft);
        result.Invoice.DueDate.ShouldBe(command.InvoiceDate.AddDays(14));
        result.Invoice.Lines.Count.ShouldBe(2);
        result.Invoice.Lines.Select(x => x.LineNo).ShouldBe([1, 2]);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Inactive_Vendor()
    {
        var vendor = ApInvoiceServiceTestFactory.CreateVendor(isActive: false);
        var command = ApInvoiceServiceTestFactory.CreateCommand(vendor.Id);

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        result.Message.ShouldContain("active vendors");
        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Duplicate_Vendor_Invoice_Number()
    {
        var vendor = ApInvoiceServiceTestFactory.CreateVendor();
        var currency = ApInvoiceServiceTestFactory.CreateCurrency();
        var command = ApInvoiceServiceTestFactory.CreateCommand(vendor.Id);

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.ExistsVendorInvoiceNoAsync(
                    vendor.Id,
                    command.InvoiceNo,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.StatusCode.ShouldBe((int)HttpStatusCode.Conflict);
        result.Message.ShouldContain("vendor");
        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Missing_Or_Inactive_Gl_Account()
    {
        var vendor = ApInvoiceServiceTestFactory.CreateVendor();
        var currency = ApInvoiceServiceTestFactory.CreateCurrency();
        var command = ApInvoiceServiceTestFactory.CreateCommand(vendor.Id, accountNo: "5999");

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);
        _factory
            .CurrencyRepository.Setup(x =>
                x.FindNoTrackingAsync("ZAR", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(currency);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.ExistsVendorInvoiceNoAsync(
                    vendor.Id,
                    command.InvoiceNo,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("5999", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((OakERP.Domain.Entities.GeneralLedger.GlAccount?)null);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        result.Message.ShouldContain("missing or inactive");
    }
}
