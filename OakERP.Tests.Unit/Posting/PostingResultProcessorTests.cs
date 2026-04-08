using Moq;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Posting.Inventory;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingResultProcessorTests
{
    private readonly PostingServiceTestFactory _factory = new();

    [Fact]
    public async Task ProcessAsync_Should_Persist_Gl_And_Inventory_Rows()
    {
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        Guid sourceId = Guid.NewGuid();
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    new DateOnly(2026, 4, 8),
                    period.Id,
                    "1100",
                    115m,
                    0m,
                    PostingSourceTypes.ArInvoice,
                    sourceId,
                    "AR-1001",
                    "AR control"
                ),
                new GlEntryModel(
                    new DateOnly(2026, 4, 8),
                    period.Id,
                    "4000",
                    0m,
                    115m,
                    PostingSourceTypes.ArInvoice,
                    sourceId,
                    "AR-1001",
                    "Revenue"
                ),
            ],
            [
                new InventoryMovementModel(
                    new DateOnly(2026, 4, 8),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    InventoryTransactionType.SalesCogs,
                    -1m,
                    12.35m,
                    -12.35m,
                    PostingSourceTypes.ArInvoice,
                    sourceId,
                    "AR-1001"
                ),
            ]
        );

        var capturedEntries = new List<GlEntry>();
        var capturedMovements = new List<InventoryLedger>();

        _factory.GlAccountRepository
            .Setup(x => x.FindNoTrackingAsync("1100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlAccount { AccountNo = "1100", IsActive = true });
        _factory.GlAccountRepository
            .Setup(x => x.FindNoTrackingAsync("4000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlAccount { AccountNo = "4000", IsActive = true });
        _factory.GlEntryRepository
            .Setup(x => x.AddAsync(It.IsAny<GlEntry>()))
            .Callback<GlEntry>(capturedEntries.Add)
            .Returns(Task.CompletedTask);
        _factory.InventoryLedgerRepository
            .Setup(x => x.AddAsync(It.IsAny<InventoryLedger>()))
            .Callback<InventoryLedger>(capturedMovements.Add)
            .Returns(Task.CompletedTask);

        var processor = CreateProcessor();

        await processor.ProcessAsync(
            postingResult,
            PostingSourceTypes.ArInvoice,
            inventoryRowsAllowed: true,
            "unit-tester",
            CancellationToken.None
        );

        capturedEntries.Count.ShouldBe(2);
        capturedEntries.All(x => x.CreatedBy == "unit-tester").ShouldBeTrue();
        capturedMovements.Count.ShouldBe(1);
        capturedMovements.Single().CreatedBy.ShouldBe("unit-tester");
        capturedMovements.Single().TransactionType.ShouldBe(InventoryTransactionType.SalesCogs);
    }

    [Fact]
    public async Task ProcessAsync_Should_Reject_Unexpected_Inventory_Movements_When_Disallowed()
    {
        Guid sourceId = Guid.NewGuid();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    new DateOnly(2026, 4, 8),
                    period.Id,
                    "1100",
                    10m,
                    0m,
                    PostingSourceTypes.ApInvoice,
                    sourceId,
                    "AP-1001",
                    "Expense"
                ),
                new GlEntryModel(
                    new DateOnly(2026, 4, 8),
                    period.Id,
                    "2000",
                    0m,
                    10m,
                    PostingSourceTypes.ApInvoice,
                    sourceId,
                    "AP-1001",
                    "AP control"
                ),
            ],
            [
                new InventoryMovementModel(
                    new DateOnly(2026, 4, 8),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    InventoryTransactionType.SalesCogs,
                    -1m,
                    12.35m,
                    -12.35m,
                    PostingSourceTypes.ApInvoice,
                    sourceId,
                    "AP-1001"
                ),
            ]
        );

        var processor = CreateProcessor();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            processor.ProcessAsync(
                postingResult,
                PostingSourceTypes.ApInvoice,
                inventoryRowsAllowed: false,
                "unit-tester",
                CancellationToken.None
            )
        );

        ex.Message.ShouldBe("Posting produced unexpected inventory movements.");
    }

    private PostingResultProcessor CreateProcessor() =>
        new(
            new PostingPersistenceDependencies(
                _factory.FiscalPeriodRepository.Object,
                _factory.GlAccountRepository.Object,
                _factory.GlEntryRepository.Object,
                _factory.InventoryLedgerRepository.Object,
                _factory.UnitOfWork.Object,
                _factory.PersistenceFailureClassifier.Object
            )
        );
}
