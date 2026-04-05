using System.Net;
using OakERP.Application.AccountsPayable;
using OakERP.Infrastructure.Accounts_Payable;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsPayable;

public sealed class ApInvoiceCommandValidatorTests
{
    private readonly ApInvoiceCommandValidator _validator = new();

    [Fact]
    public void ValidateCreate_Should_Reject_Item_Based_Lines()
    {
        var command = new CreateApInvoiceCommand
        {
            DocNo = "APINV-2001",
            VendorId = Guid.NewGuid(),
            InvoiceNo = "VEN-2001",
            InvoiceDate = new DateOnly(2026, 4, 5),
            DocTotal = 10m,
            Lines =
            [
                new ApInvoiceLineInputDTO
                {
                    AccountNo = "5000",
                    ItemId = Guid.NewGuid(),
                    Qty = 1m,
                    UnitPrice = 10m,
                    LineTotal = 10m,
                },
            ],
        };

        var result = _validator.ValidateCreate(command);

        result.Failure.ShouldNotBeNull();
        result.Failure!.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        result.Failure.Message.ShouldContain("Item-based");
    }

    [Fact]
    public void ValidateCreate_Should_Reject_Total_Mismatch()
    {
        var command = new CreateApInvoiceCommand
        {
            DocNo = "APINV-2002",
            VendorId = Guid.NewGuid(),
            InvoiceNo = "VEN-2002",
            InvoiceDate = new DateOnly(2026, 4, 5),
            TaxTotal = 5m,
            DocTotal = 20m,
            Lines =
            [
                new ApInvoiceLineInputDTO
                {
                    AccountNo = "5000",
                    Qty = 1m,
                    UnitPrice = 10m,
                    LineTotal = 10m,
                },
            ],
        };

        var result = _validator.ValidateCreate(command);

        result.Failure.ShouldNotBeNull();
        result.Failure!.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        result.Failure.Message.ShouldContain("Document total");
    }
}
