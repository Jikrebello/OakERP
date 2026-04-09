using Moq;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.Posting.Inventory;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ApPaymentPostingServiceTests
{
    private readonly PostingServiceTestFactory _factory = new();

    [Fact]
    public async Task PostAsync_Should_Post_ApPayment_And_Persist_Gl_Entries_And_Bank_Transaction()
    {
        var payment = PostingServiceTestFactory.CreateApPayment(amount: 150m, allocatedAmount: 60m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateApPaymentRule();
        var context = PostingServiceTestFactory.CreateApPaymentPostingContext(payment, rule);
        var postCommand = new PostCommand(DocKind.ApPayment, payment.Id, "unit-tester");
        BankTransaction? capturedBankTransaction = null;
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "2000",
                    150m,
                    0m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "AP control"
                ),
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "1000",
                    0m,
                    150m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "Bank"
                ),
            ],
            []
        );

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForPostingAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(payment.PaymentDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(period);
        _factory
            .PostingRuleProvider.Setup(x =>
                x.GetActiveRuleAsync(DocKind.ApPayment, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .ApPaymentPostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    payment,
                    payment.PaymentDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(context);
        _factory.PostingEngine.Setup(x => x.PostApPayment(context)).Returns(postingResult);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("2000", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "2000",
                    Name = "AP Control",
                    Type = GlAccountType.Liability,
                    IsActive = true,
                }
            );
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
            .GlEntryRepository.Setup(x => x.AddAsync(It.IsAny<GlEntry>()))
            .Returns(Task.CompletedTask);
        _factory
            .BankTransactionRepository.Setup(x => x.AddAsync(It.IsAny<BankTransaction>()))
            .Callback<BankTransaction>(transaction => capturedBankTransaction = transaction)
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        var service = _factory.CreateService();

        var result = await service.PostAsync(postCommand);

        result.DocKind.ShouldBe(DocKind.ApPayment);
        result.GlEntryCount.ShouldBe(2);
        result.InventoryEntryCount.ShouldBe(0);
        payment.DocStatus.ShouldBe(DocStatus.Posted);
        payment.PostingDate.ShouldBe(payment.PaymentDate);
        payment.UpdatedBy.ShouldBe("unit-tester");
        capturedBankTransaction.ShouldNotBeNull();
        capturedBankTransaction.BankAccountId.ShouldBe(payment.BankAccountId);
        capturedBankTransaction.TxnDate.ShouldBe(payment.PaymentDate);
        capturedBankTransaction.Amount.ShouldBe(-payment.Amount);
        capturedBankTransaction.DrAccountNo.ShouldBe(settings.ApControlAccountNo);
        capturedBankTransaction.CrAccountNo.ShouldBe(payment.BankAccount.GlAccountNo);
        capturedBankTransaction.SourceType.ShouldBe(PostingSourceTypes.ApPayment);
        capturedBankTransaction.SourceId.ShouldBe(payment.Id);
        capturedBankTransaction.Description.ShouldBe($"AP payment {payment.DocNo}");
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
        var payment = PostingServiceTestFactory.CreateApPayment(amount: 100m, allocatedAmount: 20m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateApPaymentRule();
        var context = PostingServiceTestFactory.CreateApPaymentPostingContext(payment, rule);
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "2000",
                    100m,
                    0m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "AP control"
                ),
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "1000",
                    0m,
                    100m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "Bank"
                ),
            ],
            []
        );
        var expected = new InvalidOperationException("bank transaction persistence failed");

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForPostingAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(payment.PaymentDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(period);
        _factory
            .PostingRuleProvider.Setup(x =>
                x.GetActiveRuleAsync(DocKind.ApPayment, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .ApPaymentPostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    payment,
                    payment.PaymentDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(context);
        _factory.PostingEngine.Setup(x => x.PostApPayment(context)).Returns(postingResult);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("2000", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new GlAccount { AccountNo = "2000", IsActive = true });
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("1000", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new GlAccount { AccountNo = "1000", IsActive = true });
        _factory
            .GlEntryRepository.Setup(x => x.AddAsync(It.IsAny<GlEntry>()))
            .Returns(Task.CompletedTask);
        _factory
            .BankTransactionRepository.Setup(x => x.AddAsync(It.IsAny<BankTransaction>()))
            .ThrowsAsync(expected);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApPayment, payment.Id, "unit-tester"))
        );

        ex.ShouldBe(expected);
        payment.DocStatus.ShouldBe(DocStatus.Draft);
        payment.PostingDate.ShouldBeNull();
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_ApPayment_When_Allocations_Exceed_Payment_Amount()
    {
        var payment = PostingServiceTestFactory.CreateApPayment(
            amount: 100m,
            allocatedAmount: 120m
        );

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForPostingAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(PostingServiceTestFactory.CreateSettings());

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApPayment, payment.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("exceed the payment amount");
        _factory.GlEntryRepository.Verify(x => x.AddAsync(It.IsAny<GlEntry>()), Times.Never);
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_ApPayment_When_Posting_Output_Contains_Inventory_Movements()
    {
        var payment = PostingServiceTestFactory.CreateApPayment(amount: 100m, allocatedAmount: 0m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateApPaymentRule();
        var context = PostingServiceTestFactory.CreateApPaymentPostingContext(payment, rule);
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "2000",
                    100m,
                    0m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "AP control"
                ),
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "1000",
                    0m,
                    100m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "Bank"
                ),
            ],
            [
                new InventoryMovementModel(
                    payment.PaymentDate,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    InventoryTransactionType.SalesCogs,
                    -1m,
                    1m,
                    -1m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    "unexpected"
                ),
            ]
        );

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForPostingAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(payment.PaymentDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(period);
        _factory
            .PostingRuleProvider.Setup(x =>
                x.GetActiveRuleAsync(DocKind.ApPayment, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .ApPaymentPostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    payment,
                    payment.PaymentDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(context);
        _factory.PostingEngine.Setup(x => x.PostApPayment(context)).Returns(postingResult);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApPayment, payment.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("unexpected inventory movements");
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.GlEntryRepository.Verify(x => x.AddAsync(It.IsAny<GlEntry>()), Times.Never);
        _factory.BankTransactionRepository.Verify(
            x => x.AddAsync(It.IsAny<BankTransaction>()),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }
}
