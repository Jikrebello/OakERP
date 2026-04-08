using OakERP.Common.Enums;
using Shouldly;

namespace OakERP.Tests.Unit.Settlements;

public sealed class SettlementInvoiceLoaderTests
{
    private static readonly Guid ExpectedPartyId = Guid.NewGuid();

    [Fact]
    public async Task LoadAsync_Should_Return_Empty_Dictionary_For_Empty_Request()
    {
        bool called = false;
        var spec = new SettlementInvoiceLoadSpec<FakeInvoice, string>(
            (_, _) =>
            {
                called = true;
                return Task.FromResult<IReadOnlyList<FakeInvoice>>([]);
            },
            DescribeInvoice,
            new SettlementInvoiceLoadExpectations(ExpectedPartyId, "ZAR"),
            CreateFailures()
        );

        var (invoices, failure) = await SettlementInvoiceLoader.LoadAsync([], spec, default);

        called.ShouldBeFalse();
        failure.ShouldBeNull();
        invoices.ShouldNotBeNull();
        invoices.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoadAsync_Should_Return_Failure_When_Invoice_Is_Missing()
    {
        var spec = CreateSpec([
            new FakeInvoice
            {
                Id = Guid.NewGuid(),
                DocNo = "INV-1",
                DocStatus = DocStatus.Posted,
                PartyId = ExpectedPartyId,
                CurrencyCode = "ZAR",
                RemainingAmount = 10m,
            },
        ]);

        var (_, failure) = await SettlementInvoiceLoader.LoadAsync(
            [Guid.NewGuid(), Guid.NewGuid()],
            spec,
            default
        );

        failure.ShouldBe("missing");
    }

    [Fact]
    public async Task LoadAsync_Should_Return_Failure_For_Wrong_Party()
    {
        var spec = CreateSpec([
            new FakeInvoice
            {
                Id = Guid.NewGuid(),
                DocNo = "INV-1",
                DocStatus = DocStatus.Posted,
                PartyId = Guid.NewGuid(),
                CurrencyCode = "ZAR",
                RemainingAmount = 10m,
            },
        ]);

        var (_, failure) = await SettlementInvoiceLoader.LoadAsync([Guid.NewGuid()], spec, default);

        failure.ShouldBe("party");
    }

    [Fact]
    public async Task LoadAsync_Should_Return_Failure_For_Wrong_Currency()
    {
        var spec = CreateSpec([
            new FakeInvoice
            {
                Id = Guid.NewGuid(),
                DocNo = "INV-1",
                DocStatus = DocStatus.Posted,
                PartyId = ExpectedPartyId,
                CurrencyCode = "USD",
                RemainingAmount = 10m,
            },
        ]);

        var (_, failure) = await SettlementInvoiceLoader.LoadAsync([Guid.NewGuid()], spec, default);

        failure.ShouldBe("currency");
    }

    [Fact]
    public async Task LoadAsync_Should_Return_Failure_For_No_Remaining_Balance()
    {
        var spec = CreateSpec([
            new FakeInvoice
            {
                Id = Guid.NewGuid(),
                DocNo = "INV-1",
                DocStatus = DocStatus.Posted,
                PartyId = ExpectedPartyId,
                CurrencyCode = "ZAR",
                RemainingAmount = 0m,
            },
        ]);

        var (_, failure) = await SettlementInvoiceLoader.LoadAsync([Guid.NewGuid()], spec, default);

        failure.ShouldBe("remaining:INV-1");
    }

    private static SettlementInvoiceLoadSpec<FakeInvoice, string> CreateSpec(
        IReadOnlyList<FakeInvoice> invoices
    ) =>
        new(
            (_, _) => Task.FromResult(invoices),
            DescribeInvoice,
            new SettlementInvoiceLoadExpectations(ExpectedPartyId, "ZAR"),
            CreateFailures()
        );

    private static SettlementInvoiceSnapshot DescribeInvoice(FakeInvoice invoice) =>
        new(
            invoice.Id,
            invoice.DocNo,
            invoice.DocStatus,
            invoice.PartyId,
            invoice.CurrencyCode,
            invoice.RemainingAmount
        );

    private static SettlementInvoiceLoadFailures<string> CreateFailures() =>
        new("missing", "status", "party", "currency", docNo => $"remaining:{docNo}");

    private sealed class FakeInvoice
    {
        public Guid Id { get; init; }

        public string DocNo { get; init; } = string.Empty;

        public DocStatus DocStatus { get; init; }

        public Guid PartyId { get; init; }

        public string CurrencyCode { get; init; } = string.Empty;

        public decimal RemainingAmount { get; init; }
    }
}
