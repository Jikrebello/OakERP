using Moq;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.Posting.Inventory;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ArReceiptPostingServiceTests
{
    private readonly PostingServiceTestFactory _factory = new();

    [Fact]
    public async Task PostAsync_Should_Post_ArReceipt_And_Persist_Gl_Entries_Only()
    {
        var receipt = PostingServiceTestFactory.CreateReceipt(amount: 150m, allocatedAmount: 75m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateReceiptRule();
        var postCommand = new PostCommand(DocKind.ArReceipt, receipt.Id, "unit-tester");
        var context = PostingServiceTestFactory.CreateReceiptPostingContext(receipt, rule);
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    receipt.ReceiptDate,
                    period.Id,
                    "1000",
                    150m,
                    0m,
                    PostingSourceTypes.ArReceipt,
                    receipt.Id,
                    receipt.DocNo,
                    "Bank"
                ),
                new GlEntryModel(
                    receipt.ReceiptDate,
                    period.Id,
                    "1100",
                    0m,
                    150m,
                    PostingSourceTypes.ArReceipt,
                    receipt.Id,
                    receipt.DocNo,
                    "AR control"
                ),
            ],
            []
        );

        _factory
            .ArReceiptRepository.Setup(x =>
                x.GetTrackedForPostingAsync(receipt.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(receipt);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(receipt.ReceiptDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(period);
        _factory
            .PostingRuleProvider.Setup(x =>
                x.GetActiveRuleAsync(DocKind.ArReceipt, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .ReceiptPostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    receipt,
                    receipt.ReceiptDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(context);
        _factory.PostingEngine.Setup(x => x.PostArReceipt(context)).Returns(postingResult);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("1000", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "1000",
                    Name = "Bank",
                    Type = GlAccountType.Asset,
                    IsActive = true,
                }
            );
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("1100", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "1100",
                    Name = "AR",
                    Type = GlAccountType.Asset,
                    IsActive = true,
                }
            );
        _factory
            .GlEntryRepository.Setup(x => x.AddAsync(It.IsAny<GlEntry>()))
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = _factory.CreateService();

        var result = await service.PostAsync(postCommand);

        result.DocKind.ShouldBe(DocKind.ArReceipt);
        result.GlEntryCount.ShouldBe(2);
        result.InventoryEntryCount.ShouldBe(0);
        receipt.DocStatus.ShouldBe(DocStatus.Posted);
        receipt.PostingDate.ShouldBe(receipt.ReceiptDate);
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.GlEntryRepository.Verify(x => x.AddAsync(It.IsAny<GlEntry>()), Times.Exactly(2));
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_ArReceipt_When_Allocations_Exceed_Receipt_Amount()
    {
        var receipt = PostingServiceTestFactory.CreateReceipt(amount: 100m, allocatedAmount: 120m);

        _factory
            .ArReceiptRepository.Setup(x =>
                x.GetTrackedForPostingAsync(receipt.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(receipt);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(PostingServiceTestFactory.CreateSettings());

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArReceipt, receipt.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("exceed the receipt amount");
        _factory.GlEntryRepository.Verify(x => x.AddAsync(It.IsAny<GlEntry>()), Times.Never);
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_ArReceipt_When_Posting_Output_Contains_Inventory_Movements()
    {
        var receipt = PostingServiceTestFactory.CreateReceipt(amount: 100m, allocatedAmount: 0m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateReceiptRule();
        var context = PostingServiceTestFactory.CreateReceiptPostingContext(receipt, rule);
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    receipt.ReceiptDate,
                    period.Id,
                    "1000",
                    100m,
                    0m,
                    PostingSourceTypes.ArReceipt,
                    receipt.Id,
                    receipt.DocNo,
                    "Bank"
                ),
                new GlEntryModel(
                    receipt.ReceiptDate,
                    period.Id,
                    "1100",
                    0m,
                    100m,
                    PostingSourceTypes.ArReceipt,
                    receipt.Id,
                    receipt.DocNo,
                    "AR control"
                ),
            ],
            [
                new InventoryMovementModel(
                    receipt.ReceiptDate,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    InventoryTransactionType.SalesCogs,
                    -1m,
                    1m,
                    -1m,
                    PostingSourceTypes.ArReceipt,
                    receipt.Id,
                    "unexpected"
                ),
            ]
        );

        _factory
            .ArReceiptRepository.Setup(x =>
                x.GetTrackedForPostingAsync(receipt.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(receipt);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(receipt.ReceiptDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(period);
        _factory
            .PostingRuleProvider.Setup(x =>
                x.GetActiveRuleAsync(DocKind.ArReceipt, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .ReceiptPostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    receipt,
                    receipt.ReceiptDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(context);
        _factory.PostingEngine.Setup(x => x.PostArReceipt(context)).Returns(postingResult);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArReceipt, receipt.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("unexpected inventory movements");
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }
}
