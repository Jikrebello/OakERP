using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Infrastructure.Accounts_Receivable;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArReceiptSnapshotFactoryTests
{
    private readonly ArReceiptSnapshotFactory _factory = new();

    [Fact]
    public void BuildReceiptSnapshot_Should_Order_Allocations_And_Use_Overrides()
    {
        var receipt = ArReceiptServiceTestFactory.CreateDraftReceipt(amount: 100m);
        var laterAllocation = new ArReceiptAllocation
        {
            Id = Guid.NewGuid(),
            ArReceiptId = receipt.Id,
            ArInvoiceId = Guid.NewGuid(),
            AllocationDate = new DateOnly(2026, 4, 7),
            AmountApplied = 20m,
        };
        var earlierAllocation = new ArReceiptAllocation
        {
            Id = Guid.NewGuid(),
            ArReceiptId = receipt.Id,
            ArInvoiceId = Guid.NewGuid(),
            AllocationDate = new DateOnly(2026, 4, 6),
            AmountApplied = 30m,
        };

        var snapshot = _factory.BuildReceiptSnapshot(receipt, [laterAllocation, earlierAllocation]);

        snapshot.AllocatedAmount.ShouldBe(50m);
        snapshot.UnappliedAmount.ShouldBe(50m);
        snapshot
            .Allocations.Select(x => x.AllocationId)
            .ShouldBe([earlierAllocation.Id, laterAllocation.Id]);
    }

    [Fact]
    public void BuildInvoiceSnapshots_Should_Use_Settled_Amount_Overrides()
    {
        var invoice = ArReceiptServiceTestFactory.CreatePostedInvoice(
            docTotal: 120m,
            docNo: "ARINV-3001"
        );

        var snapshots = _factory.BuildInvoiceSnapshots(
            [invoice],
            new Dictionary<Guid, decimal> { [invoice.Id] = 45m }
        );

        snapshots.Count.ShouldBe(1);
        snapshots[0].SettledAmount.ShouldBe(45m);
        snapshots[0].RemainingAmount.ShouldBe(75m);
    }
}
