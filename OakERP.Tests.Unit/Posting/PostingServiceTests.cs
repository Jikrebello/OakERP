using Microsoft.EntityFrameworkCore;
using Moq;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.General_Ledger;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingServiceTests
{
    private readonly PostingServiceTestFactory _factory = new();

    [Fact]
    public async Task PostAsync_Should_Post_ArInvoice_And_Persist_GlEntries()
    {
        var invoice = PostingServiceTestFactory.CreateInvoice();
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();
        var rule = PostingServiceTestFactory.CreateRule();
        var postCommand = new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester");
        var postingResult = new PostingEngineResult(
            [
                new GlEntryModel(invoice.InvoiceDate, period.Id, "1100", 115m, 0m, "ARINV", invoice.Id, invoice.DocNo, "AR"),
                new GlEntryModel(invoice.InvoiceDate, period.Id, "4000", 0m, 100m, "ARINV", invoice.Id, invoice.DocNo, "Revenue"),
                new GlEntryModel(invoice.InvoiceDate, period.Id, "2100", 0m, 15m, "ARINV", invoice.Id, invoice.DocNo, "Tax"),
            ],
            []
        );

        _factory.ArInvoiceRepository
            .Setup(x => x.GetTrackedForPostingAsync(invoice.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);
        _factory.GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _factory.FiscalPeriodRepository
            .Setup(x => x.GetOpenForDateAsync(invoice.InvoiceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);
        _factory.PostingRuleProvider
            .Setup(x => x.GetActiveRuleAsync(DocKind.ArInvoice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        _factory.PostingEngine
            .Setup(x => x.PostArInvoice(It.IsAny<OakERP.Domain.Posting.Accounts_Receivable.ArInvoicePostingContext>()))
            .Returns(postingResult);
        _factory.GlAccountRepository
            .Setup(x => x.FindNoTrackingAsync("1100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlAccount { AccountNo = "1100", Name = "AR", Type = GlAccountType.Asset, IsActive = true });
        _factory.GlAccountRepository
            .Setup(x => x.FindNoTrackingAsync("4000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlAccount { AccountNo = "4000", Name = "Revenue", Type = GlAccountType.Revenue, IsActive = true });
        _factory.GlAccountRepository
            .Setup(x => x.FindNoTrackingAsync("2100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlAccount { AccountNo = "2100", Name = "VAT", Type = GlAccountType.Liability, IsActive = true });
        _factory.GlEntryRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>())).Returns(Task.CompletedTask);
        _factory.UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(4);

        var service = _factory.CreateService();

        var result = await service.PostAsync(postCommand);

        result.GlEntryCount.ShouldBe(3);
        result.InventoryEntryCount.ShouldBe(0);
        invoice.DocStatus.ShouldBe(DocStatus.Posted);
        invoice.PostingDate.ShouldBe(invoice.InvoiceDate);
        invoice.UpdatedBy.ShouldBe("unit-tester");
        _factory.GlEntryRepository.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>()), Times.Exactly(3));
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_Stock_Lines_Before_Engine_Execution()
    {
        var invoice = PostingServiceTestFactory.CreateInvoice();
        invoice.Lines.Single().Item = new Item { Type = ItemType.Stock };
        var settings = PostingServiceTestFactory.CreateSettings();
        var period = PostingServiceTestFactory.CreateOpenPeriod();

        _factory.ArInvoiceRepository
            .Setup(x => x.GetTrackedForPostingAsync(invoice.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);
        _factory.GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _factory.FiscalPeriodRepository
            .Setup(x => x.GetOpenForDateAsync(invoice.InvoiceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        var service = _factory.CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester"))
        );

        ex.Message.ShouldContain("stock lines");
        _factory.PostingEngine.Verify(
            x => x.PostArInvoice(It.IsAny<OakERP.Domain.Posting.Accounts_Receivable.ArInvoicePostingContext>()),
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
                new GlEntryModel(invoice.InvoiceDate, period.Id, "1100", 115m, 0m, "ARINV", invoice.Id, invoice.DocNo, "AR"),
                new GlEntryModel(invoice.InvoiceDate, period.Id, "4000", 0m, 100m, "ARINV", invoice.Id, invoice.DocNo, "Revenue"),
                new GlEntryModel(invoice.InvoiceDate, period.Id, "2100", 0m, 15m, "ARINV", invoice.Id, invoice.DocNo, "Tax"),
            ],
            []
        );

        _factory.ArInvoiceRepository
            .Setup(x => x.GetTrackedForPostingAsync(invoice.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);
        _factory.GlSettingsProvider.Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _factory.FiscalPeriodRepository
            .Setup(x => x.GetOpenForDateAsync(invoice.InvoiceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);
        _factory.PostingRuleProvider
            .Setup(x => x.GetActiveRuleAsync(DocKind.ArInvoice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        _factory.PostingEngine
            .Setup(x => x.PostArInvoice(It.IsAny<OakERP.Domain.Posting.Accounts_Receivable.ArInvoicePostingContext>()))
            .Returns(postingResult);
        _factory.GlAccountRepository
            .Setup(x => x.FindNoTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string accountNo, CancellationToken _) => new GlAccount { AccountNo = accountNo, Name = accountNo, Type = GlAccountType.Asset, IsActive = true });
        _factory.GlEntryRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.General_Ledger.GlEntry>())).Returns(Task.CompletedTask);
        _factory.UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("save failed", new Exception()));

        var service = _factory.CreateService();

        await Should.ThrowAsync<DbUpdateException>(() =>
            service.PostAsync(new PostCommand(DocKind.ArInvoice, invoice.Id, "unit-tester"))
        );

        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task PostAsync_Should_Reject_Unsupported_DocKind()
    {
        var service = _factory.CreateService();

        await Should.ThrowAsync<NotSupportedException>(() =>
            service.PostAsync(new PostCommand(DocKind.ApInvoice, Guid.NewGuid(), "unit-tester"))
        );

        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }
}
