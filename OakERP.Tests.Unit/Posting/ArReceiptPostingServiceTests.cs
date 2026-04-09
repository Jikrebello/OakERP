using Moq;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Bank;
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
    public async Task PostAsync_Should_Post_ArReceipt_And_Persist_Gl_Entries_And_Bank_Transaction()
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
        BankTransaction? capturedBankTransaction = null;

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
            .BankTransactionRepository.Setup(x => x.AddAsync(It.IsAny<BankTransaction>()))
            .Callback<BankTransaction>(transaction => capturedBankTransaction = transaction)
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
        capturedBankTransaction.ShouldNotBeNull();
        capturedBankTransaction.BankAccountId.ShouldBe(receipt.BankAccountId);
        capturedBankTransaction.TxnDate.ShouldBe(receipt.ReceiptDate);
        capturedBankTransaction.Amount.ShouldBe(receipt.Amount);
        capturedBankTransaction.DrAccountNo.ShouldBe(receipt.BankAccount.GlAccountNo);
        capturedBankTransaction.CrAccountNo.ShouldBe(settings.ArControlAccountNo);
        capturedBankTransaction.SourceType.ShouldBe(PostingSourceTypes.ArReceipt);
        capturedBankTransaction.SourceId.ShouldBe(receipt.Id);
        capturedBankTransaction.Description.ShouldBe($"AR receipt {receipt.DocNo}");
        capturedBankTransaction.ExternalRef.ShouldBeNull();
        capturedBankTransaction.IsReconciled.ShouldBeFalse();
        capturedBankTransaction.CreatedBy.ShouldBe("unit-tester");
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.GlEntryRepository.Verify(x => x.AddAsync(It.IsAny<GlEntry>()), Times.Exactly(2));
        _factory.BankTransactionRepository.Verify(
            x => x.AddAsync(It.IsAny<BankTransaction>()),
            Times.Once
        );
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Rollback_When_Bank_Transaction_Persistence_Fails()
    {
        var receipt = PostingServiceTestFactory.CreateReceipt(amount: 100m, allocatedAmount: 20m);
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
            []
        );
        var expected = new InvalidOperationException("bank transaction persistence failed");

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
            .ReturnsAsync(new GlAccount { AccountNo = "1000", IsActive = true });
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("1100", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new GlAccount { AccountNo = "1100", IsActive = true });
        _factory
            .GlEntryRepository.Setup(x => x.AddAsync(It.IsAny<GlEntry>()))
            .Returns(Task.CompletedTask);
        _factory
            .BankTransactionRepository.Setup(x => x.AddAsync(It.IsAny<BankTransaction>()))
            .ThrowsAsync(expected);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArReceipt, receipt.Id, "unit-tester"))
        );

        ex.ShouldBe(expected);
        receipt.DocStatus.ShouldBe(DocStatus.Draft);
        receipt.PostingDate.ShouldBeNull();
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
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
