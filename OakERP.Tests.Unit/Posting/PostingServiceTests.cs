using Microsoft.EntityFrameworkCore;
using Moq;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Posting.Inventory;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingServiceTests
{
    private readonly PostingServiceTestFactory _factory = new();

    [Fact]
    public async Task PostAsync_Should_Post_ArInvoice_And_Persist_Gl_And_Inventory_Entries()
    {
        var invoice = PostingServiceTestFactory.CreateStockInvoice();
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateRule();
        var postCommand = new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester");
        var context = PostingServiceTestFactory.CreatePostingContext(invoice, rule);
        var stockLine = invoice.Lines.Single();
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "1100",
                    115m,
                    0m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "AR"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "4000",
                    0m,
                    100m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Revenue"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "2100",
                    0m,
                    15m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Tax"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "5100",
                    12.35m,
                    0m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "COGS"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "1300",
                    0m,
                    12.35m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Inventory"
                ),
            ],
            [
                new InventoryMovementModel(
                    invoice.InvoiceDate,
                    stockLine.ItemId!.Value,
                    stockLine.LocationId!.Value,
                    InventoryTransactionType.SalesCogs,
                    -1m,
                    12.3456m,
                    -12.35m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    "AR inventory"
                ),
            ]
        );

        _factory
            .ArInvoiceRepository.Setup(x =>
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
                x.GetActiveRuleAsync(DocKind.ArInvoice, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .PostingContextBuilder.Setup(x =>
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
        _factory.PostingEngine.Setup(x => x.PostArInvoice(context)).Returns(postingResult);
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
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("4000", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "4000",
                    Name = "Revenue",
                    Type = GlAccountType.Revenue,
                    IsActive = true,
                }
            );
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("2100", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "2100",
                    Name = "VAT",
                    Type = GlAccountType.Liability,
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
                    Name = "COGS",
                    Type = GlAccountType.Expense,
                    IsActive = true,
                }
            );
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync("1300", It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new GlAccount
                {
                    AccountNo = "1300",
                    Name = "Inventory",
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
            .InventoryLedgerRepository.Setup(x =>
                x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(6);

        var service = _factory.CreateService();

        var result = await service.PostAsync(postCommand);

        result.GlEntryCount.ShouldBe(5);
        result.InventoryEntryCount.ShouldBe(1);
        invoice.DocStatus.ShouldBe(DocStatus.Posted);
        invoice.PostingDate.ShouldBe(invoice.InvoiceDate);
        invoice.UpdatedBy.ShouldBe("unit-tester");
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Exactly(5)
        );
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Once
        );
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Rollback_When_Context_Builder_Fails()
    {
        var invoice = PostingServiceTestFactory.CreateStockInvoice(includeLocation: false);
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateRule();

        _factory
            .ArInvoiceRepository.Setup(x =>
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
                x.GetActiveRuleAsync(DocKind.ArInvoice, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .PostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    invoice,
                    invoice.InvoiceDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(
                new InvalidOperationException("Stock AR invoice line 1 requires a location.")
            );

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("requires a location");
        _factory.PostingEngine.Verify(
            x => x.PostArInvoice(It.IsAny<ArInvoicePostingContext>()),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Rollback_When_SaveChanges_Fails()
    {
        var invoice = PostingServiceTestFactory.CreateInvoice();
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateRule();
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "1100",
                    115m,
                    0m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "AR"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "4000",
                    0m,
                    100m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Revenue"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "2100",
                    0m,
                    15m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Tax"
                ),
            ],
            []
        );

        _factory
            .ArInvoiceRepository.Setup(x =>
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
                x.GetActiveRuleAsync(DocKind.ArInvoice, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .PostingContextBuilder.Setup(x =>
                x.BuildAsync(
                    invoice,
                    invoice.InvoiceDate,
                    period,
                    settings,
                    rule,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(PostingServiceTestFactory.CreatePostingContext(invoice, rule));
        _factory
            .PostingEngine.Setup(x => x.PostArInvoice(It.IsAny<ArInvoicePostingContext>()))
            .Returns(postingResult);
        _factory
            .GlAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                (string accountNo, CancellationToken _) =>
                    new GlAccount
                    {
                        AccountNo = accountNo,
                        Name = accountNo,
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
            .ThrowsAsync(new DbUpdateException("save failed", new Exception()));

        var service = _factory.CreateService();

        await Should.ThrowAsync<DbUpdateException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester"))
        );

        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task PostAsync_Should_Rollback_When_Inventory_Value_Does_Not_Match_Qty_And_UnitCost()
    {
        var invoice = PostingServiceTestFactory.CreateStockInvoice();
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateRule();
        var context = PostingServiceTestFactory.CreatePostingContext(invoice, rule);
        var stockLine = invoice.Lines.Single();
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "1100",
                    115m,
                    0m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "AR"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "4000",
                    0m,
                    100m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Revenue"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "2100",
                    0m,
                    15m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Tax"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "5100",
                    12.35m,
                    0m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "COGS"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "1300",
                    0m,
                    12.35m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Inventory"
                ),
            ],
            [
                new InventoryMovementModel(
                    invoice.InvoiceDate,
                    stockLine.ItemId!.Value,
                    stockLine.LocationId!.Value,
                    InventoryTransactionType.SalesCogs,
                    -1m,
                    12.3456m,
                    -12.34m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    "AR inventory"
                ),
            ]
        );

        _factory
            .ArInvoiceRepository.Setup(x =>
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
                x.GetActiveRuleAsync(DocKind.ArInvoice, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .PostingContextBuilder.Setup(x =>
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
        _factory.PostingEngine.Setup(x => x.PostArInvoice(context)).Returns(postingResult);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("does not match quantity and unit cost");
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Never
        );
        _factory.InventoryLedgerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Inventory.InventoryLedger>()),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Rollback_When_Posting_Output_Misses_Traceability()
    {
        var invoice = PostingServiceTestFactory.CreateInvoice();
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateRule();
        var context = PostingServiceTestFactory.CreatePostingContext(invoice, rule);
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "1100",
                    115m,
                    0m,
                    PostingSourceTypes.ArInvoice,
                    Guid.Empty,
                    invoice.DocNo,
                    "AR"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "4000",
                    0m,
                    100m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Revenue"
                ),
                new GlEntryModel(
                    invoice.InvoiceDate,
                    period.Id,
                    "2100",
                    0m,
                    15m,
                    PostingSourceTypes.ArInvoice,
                    invoice.Id,
                    invoice.DocNo,
                    "Tax"
                ),
            ],
            []
        );

        _factory
            .ArInvoiceRepository.Setup(x =>
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
                x.GetActiveRuleAsync(DocKind.ArInvoice, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(rule);
        _factory
            .PostingContextBuilder.Setup(x =>
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
        _factory.PostingEngine.Setup(x => x.PostArInvoice(context)).Returns(postingResult);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("without a source id");
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Never
        );
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_Unsupported_DocKind()
    {
        var service = _factory.CreateService();

        await Should.ThrowAsync<NotSupportedException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApCreditNote, Guid.NewGuid(), "unit-tester"))
        );

        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

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
            .GlEntryRepository.Setup(x =>
                x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>())
            )
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
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Exactly(2)
        );
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

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArReceipt, receipt.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("exceed the receipt amount");
        _factory.GlEntryRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()),
            Times.Never
        );
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

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
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
