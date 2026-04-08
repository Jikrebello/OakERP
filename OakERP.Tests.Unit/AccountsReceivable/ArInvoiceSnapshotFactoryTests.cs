using OakERP.Domain.Entities.AccountsReceivable;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArInvoiceSnapshotFactoryTests
{
    [Fact]
    public void BuildInvoiceSnapshot_Should_Order_Lines_And_Preserve_Optional_Metadata()
    {
        Guid itemId = Guid.NewGuid();
        Guid locationId = Guid.NewGuid();
        Guid taxRateId = Guid.NewGuid();

        var invoice = new ArInvoice
        {
            Id = Guid.NewGuid(),
            DocNo = "ARINV-3001",
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DaysFromToday(-4),
            DueDate = DaysFromToday(-4).AddDays(30),
            CurrencyCode = "ZAR",
            ShipTo = "Customer site",
            Memo = "Snapshot test",
            TaxTotal = 15m,
            DocTotal = 165m,
            Lines =
            [
                new ArInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    LineNo = 2,
                    Description = "Item line",
                    ItemId = itemId,
                    LocationId = locationId,
                    TaxRateId = taxRateId,
                    Qty = 1m,
                    UnitPrice = 100m,
                    LineTotal = 100m,
                },
                new ArInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    LineNo = 1,
                    Description = "Service line",
                    RevenueAccount = "4000",
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
            ],
        };

        var snapshot = ArInvoiceSnapshotFactory.BuildInvoiceSnapshot(invoice);

        snapshot.Lines.Select(x => x.LineNo).ShouldBe([1, 2]);
        snapshot.Lines[0].RevenueAccount.ShouldBe("4000");
        snapshot.Lines[1].ItemId.ShouldBe(itemId);
        snapshot.Lines[1].LocationId.ShouldBe(locationId);
        snapshot.Lines[1].TaxRateId.ShouldBe(taxRateId);
    }
}
