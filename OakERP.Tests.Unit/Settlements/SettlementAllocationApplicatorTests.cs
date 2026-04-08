using OakERP.Common.Enums;
using Shouldly;

namespace OakERP.Tests.Unit.Settlements;

public sealed class SettlementAllocationApplicatorTests
{
    private static readonly DateTimeOffset UpdatedAt = new(2026, 4, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ApplyAsync_Should_Reject_When_Request_Exceeds_Document_Unapplied_Amount()
    {
        var document = CreateDocument(unappliedAmount: 10m);
        Dictionary<Guid, FakeInvoice> invoices = CreateInvoices([
            CreateInvoiceEntry(docTotal: 50m),
        ]);
        var spec = CreateSpec(document, invoices);

        var (failure, _, _) = await SettlementAllocationApplicator.ApplyAsync(
            [new SettlementAllocationInput(invoices.Keys.Single(), 15m)],
            new DateOnly(2026, 4, 8),
            "unit-user",
            UpdatedAt,
            spec
        );

        failure.ShouldBe("doc-unapplied");
    }

    [Fact]
    public async Task ApplyAsync_Should_Reject_When_Request_Exceeds_Invoice_Remaining_Amount()
    {
        var document = CreateDocument(unappliedAmount: 20m);
        Dictionary<Guid, FakeInvoice> invoices = CreateInvoices([
            CreateInvoiceEntry(docTotal: 10m, settledAmount: 5m),
        ]);
        var spec = CreateSpec(document, invoices);

        var (failure, _, _) = await SettlementAllocationApplicator.ApplyAsync(
            [new SettlementAllocationInput(invoices.Keys.Single(), 6m)],
            new DateOnly(2026, 4, 8),
            "unit-user",
            UpdatedAt,
            spec
        );

        failure.ShouldBe("invoice-remaining:INV-1");
    }

    [Fact]
    public async Task ApplyAsync_Should_Keep_Invoice_Posted_On_Partial_Allocation()
    {
        var document = CreateDocument(unappliedAmount: 50m);
        Dictionary<Guid, FakeInvoice> invoices = CreateInvoices([
            CreateInvoiceEntry(docTotal: 100m),
        ]);
        var spec = CreateSpec(document, invoices);

        var (failure, settledAmounts, allocations) =
            await SettlementAllocationApplicator.ApplyAsync(
                [new SettlementAllocationInput(invoices.Keys.Single(), 40m)],
                new DateOnly(2026, 4, 8),
                "unit-user",
                UpdatedAt,
                spec
            );

        failure.ShouldBeNull();
        allocations.Count.ShouldBe(1);
        invoices.Values.Single().DocStatus.ShouldBe(DocStatus.Posted);
        settledAmounts[invoices.Keys.Single()].ShouldBe(40m);
    }

    [Fact]
    public async Task ApplyAsync_Should_Close_Invoice_On_Full_Allocation()
    {
        var document = CreateDocument(unappliedAmount: 50m);
        Dictionary<Guid, FakeInvoice> invoices = CreateInvoices([
            CreateInvoiceEntry(docTotal: 40m),
        ]);
        var spec = CreateSpec(document, invoices);

        var (failure, settledAmounts, allocations) =
            await SettlementAllocationApplicator.ApplyAsync(
                [new SettlementAllocationInput(invoices.Keys.Single(), 40m)],
                new DateOnly(2026, 4, 8),
                "unit-user",
                UpdatedAt,
                spec
            );

        failure.ShouldBeNull();
        allocations.Count.ShouldBe(1);
        invoices.Values.Single().DocStatus.ShouldBe(DocStatus.Closed);
        settledAmounts[invoices.Keys.Single()].ShouldBe(40m);
    }

    [Fact]
    public async Task ApplyAsync_Should_Track_Settled_Amounts_Across_Multiple_Allocations()
    {
        var document = CreateDocument(unappliedAmount: 100m);
        Dictionary<Guid, FakeInvoice> invoices = CreateInvoices([
            CreateInvoiceEntry(docNo: "INV-1", docTotal: 60m, settledAmount: 10m),
            CreateInvoiceEntry(docNo: "INV-2", docTotal: 40m),
        ]);
        Guid[] invoiceIds = [.. invoices.Keys];
        var spec = CreateSpec(document, invoices);

        var (failure, settledAmounts, allocations) =
            await SettlementAllocationApplicator.ApplyAsync(
                [
                    new SettlementAllocationInput(invoiceIds[0], 20m),
                    new SettlementAllocationInput(invoiceIds[1], 15m),
                ],
                new DateOnly(2026, 4, 8),
                "unit-user",
                UpdatedAt,
                spec
            );

        failure.ShouldBeNull();
        allocations.Count.ShouldBe(2);
        settledAmounts[invoiceIds[0]].ShouldBe(30m);
        settledAmounts[invoiceIds[1]].ShouldBe(15m);
    }

    private static SettlementAllocationApplySpec<FakeAllocation, string> CreateSpec(
        FakeDocument document,
        IReadOnlyDictionary<Guid, FakeInvoice> invoices
    ) =>
        new(
            () => document.Allocations,
            () => document.UnappliedAmount,
            (performedBy, updatedAt) =>
            {
                document.UpdatedBy = performedBy;
                document.UpdatedAt = updatedAt;
            },
            invoiceId =>
            {
                if (!invoices.TryGetValue(invoiceId, out FakeInvoice? invoice))
                {
                    return null;
                }

                return new SettlementAllocationInvoiceAdapter(
                    invoice.Id,
                    invoice.DocNo,
                    () => invoice.SettledAmount,
                    currentSettledAmount => invoice.DocTotal - currentSettledAmount,
                    (performedBy, updatedAt) =>
                    {
                        invoice.UpdatedBy = performedBy;
                        invoice.UpdatedAt = updatedAt;
                    },
                    remainingAfterAllocation =>
                    {
                        if (remainingAfterAllocation == 0m)
                        {
                            invoice.DocStatus = DocStatus.Closed;
                        }
                    }
                );
            },
            (input, allocationDate) =>
                new FakeAllocation
                {
                    DocumentId = document.Id,
                    InvoiceId = input.InvoiceId,
                    AllocationDate = allocationDate,
                    AmountApplied = input.AmountApplied,
                },
            _ => Task.CompletedTask,
            new SettlementAllocationFailures<string>(
                "doc-unapplied",
                "invoice-missing",
                docNo => $"invoice-remaining:{docNo}"
            )
        );

    private static FakeDocument CreateDocument(decimal unappliedAmount) =>
        new() { Id = Guid.NewGuid(), UnappliedAmount = unappliedAmount };

    private static KeyValuePair<Guid, FakeInvoice> CreateInvoiceEntry(
        string docNo = "INV-1",
        decimal docTotal = 100m,
        decimal settledAmount = 0m
    )
    {
        var invoice = new FakeInvoice
        {
            Id = Guid.NewGuid(),
            DocNo = docNo,
            DocTotal = docTotal,
            SettledAmount = settledAmount,
            DocStatus = DocStatus.Posted,
        };

        return new KeyValuePair<Guid, FakeInvoice>(invoice.Id, invoice);
    }

    private static Dictionary<Guid, FakeInvoice> CreateInvoices(
        params KeyValuePair<Guid, FakeInvoice>[] invoices
    ) => invoices.ToDictionary(x => x.Key, x => x.Value);

    private sealed class FakeDocument
    {
        public Guid Id { get; init; }

        public decimal UnappliedAmount { get; init; }

        public List<FakeAllocation> Allocations { get; } = [];

        public string? UpdatedBy { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class FakeInvoice
    {
        public Guid Id { get; init; }

        public string DocNo { get; init; } = string.Empty;

        public decimal DocTotal { get; init; }

        public decimal SettledAmount { get; init; }

        public DocStatus DocStatus { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class FakeAllocation
    {
        public Guid DocumentId { get; init; }

        public Guid InvoiceId { get; init; }

        public DateOnly AllocationDate { get; init; }

        public decimal AmountApplied { get; init; }
    }
}
