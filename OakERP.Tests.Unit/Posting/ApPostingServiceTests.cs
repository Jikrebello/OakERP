using Moq;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Posting.Inventory;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class ApPostingServiceTests
{
    private readonly PostingServiceTestFactory _factory = new();

    [Fact]
    public async Task PostAsync_Should_Post_ApInvoice_And_Persist_Gl_Entries_Only()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 15m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateApInvoiceRule();
        var context = PostingServiceTestFactory.CreateApInvoicePostingContext(invoice, rule);
        var postCommand = new PostCommand(DocKind.ApInvoice, invoice.Id, "unit-tester");
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "2000",
                    0m,
                    115m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "AP control"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "5000",
                    60m,
                    0m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Expense 1"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "5100",
                    40m,
                    0m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Expense 2"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "2200",
                    15m,
                    0m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Tax"
                ),
            ],
            []
        );

        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForPostingAsync(invoice.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(invoice);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(invoice.InvoiceDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(period);
        _factory
            .PostingRuleProvider.Setup(x =>
                x.GetActiveRuleAsync(DocKind.ApInvoice, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .ApInvoicePostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    invoice,
                    invoice.InvoiceDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(context);
        _factory.PostingEngine.Setup(x => x.PostApInvoice(context)).Returns(postingResult);
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
                x.FindNoTrackingAsync("5000", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "5000",
                    Name = "Expense",
                    Type = GlAccountType.Expense,
                    IsActive = true,
                }
            );
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("5100", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "5100",
                    Name = "Expense 2",
                    Type = GlAccountType.Expense,
                    IsActive = true,
                }
            );
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("2200", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "2200",
                    Name = "Input VAT",
                    Type = GlAccountType.Asset,
                    IsActive = true,
                }
            );
        _factory
            .GlEntryRepository.Setup(x =>
                x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var service = _factory.CreateService();

        var result = await service.PostAsync(postCommand);

        result.DocKind.ShouldBe(DocKind.ApInvoice);
        result.GlEntryCount.ShouldBe(4);
        result.InventoryEntryCount.ShouldBe(0);
        invoice.DocStatus.ShouldBe(DocStatus.Posted);
        invoice.UpdatedBy.ShouldBe("unit-tester");
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Exactly(4)
        );
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_ApInvoice_When_Totals_Are_Inconsistent()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 15m);
        invoice.DocTotal = 200m;

        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForPostingAsync(invoice.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(invoice);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(PostingServiceTestFactory.CreateSettings());
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(invoice.InvoiceDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(PostingServiceTestFactory.CreateOpenPeriod());

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApInvoice, invoice.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("totals are inconsistent");
        _factory.ApInvoicePostingContextBuilder.Verify(
            x =>
                x.BuildAsync(
                    It.IsAny<OakERP.Domain.Entities.Accounts_Payable.ApInvoice>(),
                    It.IsAny<DateOnly>(),
                    It.IsAny<OakERP.Domain.Entities.General_Ledger.FiscalPeriod>(),
                    It.IsAny<GlPostingSettings>(),
                    It.IsAny<PostingRule>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_ApInvoice_When_Posting_Output_Contains_Inventory_Movements()
    {
        var invoice = PostingServiceTestFactory.CreateApInvoice(taxTotal: 0m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateApInvoiceRule();
        var context = PostingServiceTestFactory.CreateApInvoicePostingContext(invoice, rule);
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "2000",
                    0m,
                    100m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "AP control"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "5000",
                    60m,
                    0m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Expense 1"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "5100",
                    40m,
                    0m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Expense 2"
                ),
            ],
            [
                new InventoryMovementModel(
                    invoice.InvoiceDate,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    InventoryTransactionType.SalesCogs,
                    -1m,
                    1m,
                    -1m,
                    PostingSourceTypes.ApInvoice,
                    invoice.Id,
                    "unexpected"
                ),
            ]
        );

        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForPostingAsync(invoice.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(invoice);
        _factory
            .GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(invoice.InvoiceDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(period);
        _factory
            .PostingRuleProvider.Setup(x =>
                x.GetActiveRuleAsync(DocKind.ApInvoice, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .ApInvoicePostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    invoice,
                    invoice.InvoiceDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(context);
        _factory.PostingEngine.Setup(x => x.PostApInvoice(context)).Returns(postingResult);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApInvoice, invoice.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("unexpected inventory movements");
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Post_ApPayment_And_Persist_Gl_Entries_Only()
    {
        var payment = PostingServiceTestFactory.CreateApPayment(amount: 150m, allocatedAmount: 60m);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateApPaymentRule();
        var context = PostingServiceTestFactory.CreateApPaymentPostingContext(payment, rule);
        var postCommand = new PostCommand(DocKind.ApPayment, payment.Id, "unit-tester");
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "1000",
                    150m,
                    0m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "Bank"
                ),
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "2000",
                    0m,
                    150m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "AP control"
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
            .GlEntryRepository.Setup(x =>
                x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = _factory.CreateService();

        var result = await service.PostAsync(postCommand);

        result.DocKind.ShouldBe(DocKind.ApPayment);
        result.GlEntryCount.ShouldBe(2);
        result.InventoryEntryCount.ShouldBe(0);
        payment.DocStatus.ShouldBe(DocStatus.Posted);
        payment.PostingDate.ShouldBe(payment.PaymentDate);
        payment.UpdatedBy.ShouldBe("unit-tester");
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Exactly(2)
        );
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
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

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApPayment, payment.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("exceed the payment amount");
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Never
        );
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
                    "1000",
                    100m,
                    0m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "Bank"
                ),
                new GlEntryModel(
                    payment.PaymentDate,
                    period.Id,
                    "2000",
                    0m,
                    100m,
                    PostingSourceTypes.ApPayment,
                    payment.Id,
                    payment.DocNo,
                    "AP control"
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

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApPayment, payment.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("unexpected inventory movements");
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }
}
