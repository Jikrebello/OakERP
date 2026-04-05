using System.Net;
using OakERP.Application.AccountsReceivable;
using OakERP.Infrastructure.Accounts_Receivable;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArReceiptCommandValidatorTests
{
    private readonly ArReceiptCommandValidator _validator = new();

    [Fact]
    public void ValidateCreate_Should_Normalize_Fields_And_Default_PerformedBy()
    {
        var command = new CreateArReceiptCommand
        {
            DocNo = "  rcpt-3001  ",
            CustomerId = Guid.NewGuid(),
            BankAccountId = Guid.NewGuid(),
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 100m,
            CurrencyCode = "  zar ",
            Memo = "  first receipt  ",
        };

        var result = _validator.ValidateCreate(
            command,
            ArReceiptServiceTestFactory.CreateSettings()
        );

        result.Failure.ShouldBeNull();
        result.DocNo.ShouldBe("rcpt-3001");
        result.CurrencyCode.ShouldBe("ZAR");
        result.Memo.ShouldBe("first receipt");
        result.PerformedBy.ShouldBe("system");
    }

    [Fact]
    public void ValidateAllocate_Should_Reject_Duplicate_Invoice_Ids()
    {
        Guid invoiceId = Guid.NewGuid();
        var command = new AllocateArReceiptCommand
        {
            ReceiptId = Guid.NewGuid(),
            Allocations =
            [
                new ArReceiptAllocationInputDTO { ArInvoiceId = invoiceId, AmountApplied = 10m },
                new ArReceiptAllocationInputDTO { ArInvoiceId = invoiceId, AmountApplied = 15m },
            ],
        };

        var result = _validator.ValidateAllocate(command);

        result.Failure.ShouldNotBeNull();
        result.Failure!.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        result.Failure.Message.ShouldContain("only once per allocation request");
    }
}
