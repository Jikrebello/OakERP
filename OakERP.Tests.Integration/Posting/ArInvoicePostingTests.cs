using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Infrastructure.Persistence;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;

namespace OakERP.Tests.Integration.Posting;

[TestFixture]
public sealed class ArInvoicePostingTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task PostAsync_Should_Write_GlEntries_And_Mark_Invoice_Posted()
    {
        var invoiceId = await SeedInvoiceScenarioAsync();

        PostResult result = await PostInvoiceAsync(invoiceId);

        result.DocKind.ShouldBe(DocKind.ArInvoice);
        result.GlEntryCount.ShouldBe(3);
        result.InventoryEntryCount.ShouldBe(0);

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
            invoice.PostingDate.ShouldBe(new DateOnly(2026, 3, 15));

            var glEntries = await db.GlEntries.Where(x => x.SourceId == invoiceId).OrderBy(x => x.AccountNo).ToListAsync();
            glEntries.Count.ShouldBe(3);
            glEntries.Sum(x => x.Debit).ShouldBe(glEntries.Sum(x => x.Credit));
            glEntries.ShouldContain(x => x.AccountNo == "1100" && x.Debit == 115m);
            glEntries.ShouldContain(x => x.AccountNo == "4000" && x.Credit == 100m);
            glEntries.ShouldContain(x => x.AccountNo == "2100" && x.Credit == 15m);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_Second_Post_Attempt_Without_Duplicating_Gl()
    {
        var invoiceId = await SeedInvoiceScenarioAsync();

        await PostInvoiceAsync(invoiceId);

        await Should.ThrowAsync<InvalidOperationException>(() => PostInvoiceAsync(invoiceId));

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(3);
        });
    }

    [Test]
    public async Task PostAsync_Should_Leave_Only_One_Committed_Posting_During_Concurrent_Attempts()
    {
        var invoiceId = await SeedInvoiceScenarioAsync();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Exception?> AttemptAsync() =>
            Task.Run(async () =>
            {
                await gate.Task;
                try
                {
                    await PostInvoiceAsync(invoiceId);
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            });

        var first = AttemptAsync();
        var second = AttemptAsync();
        gate.SetResult();

        var results = await Task.WhenAll(first, second);

        results.Count(x => x is null).ShouldBe(1);
        results.Count(x => x is not null).ShouldBe(1);

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(3);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_Stock_Line_Invoice_Transactionally()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(includeStockLine: true);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => PostInvoiceAsync(invoiceId));

        ex.Message.ShouldContain("stock lines");

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            invoice.PostingDate.ShouldBeNull();
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_When_No_Open_Period_Exists()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(includeOpenPeriod: false);

        await Should.ThrowAsync<InvalidOperationException>(() => PostInvoiceAsync(invoiceId));

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_Non_Base_Currency_Invoices()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(currencyCode: "USD");

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => PostInvoiceAsync(invoiceId));

        ex.Message.ShouldContain("base currency");

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
        });
    }

    private async Task<PostResult> PostInvoiceAsync(Guid invoiceId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPostingService>();

        return await service.PostAsync(
            new PostCommand(DocKind.ArInvoice, invoiceId, "integration-user")
        );
    }

    private async Task<Guid> SeedInvoiceScenarioAsync(
        bool includeStockLine = false,
        bool includeOpenPeriod = true,
        string currencyCode = "ZAR"
    )
    {
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        await WithDbAsync(async db =>
        {
            db.Currencies.AddRange(
                new Currency
                {
                    Code = "ZAR",
                    NumericCode = 710,
                    Name = "Rand",
                    Symbol = "R",
                    Decimals = 2,
                    IsActive = true,
                },
                new Currency
                {
                    Code = "USD",
                    NumericCode = 840,
                    Name = "US Dollar",
                    Symbol = "$",
                    Decimals = 2,
                    IsActive = true,
                }
            );

            db.GlAccounts.AddRange(
                new GlAccount { AccountNo = "1100", Name = "Accounts Receivable", Type = GlAccountType.Asset, IsActive = true, IsControl = true },
                new GlAccount { AccountNo = "4000", Name = "Sales", Type = GlAccountType.Revenue, IsActive = true },
                new GlAccount { AccountNo = "2100", Name = "Output VAT", Type = GlAccountType.Liability, IsActive = true }
            );

            db.AppSettings.Add(
                new AppSetting
                {
                    Key = "gl.posting",
                    ValueJson = JsonSerializer.Serialize(
                        new GlPostingSettings(
                            "ZAR",
                            "1100",
                            "2000",
                            "4000",
                            "5000",
                            "1300",
                            "5100",
                            "2100",
                            "2200"
                        )
                    ),
                }
            );

            if (includeOpenPeriod)
            {
                db.FiscalPeriods.Add(
                    new FiscalPeriod
                    {
                        Id = Guid.NewGuid(),
                        FiscalYear = 2026,
                        PeriodNo = 3,
                        PeriodStart = new DateOnly(2026, 3, 1),
                        PeriodEnd = new DateOnly(2026, 3, 31),
                        Status = "open",
                    }
                );
            }

            db.Customers.Add(
                new Customer
                {
                    Id = customerId,
                    CustomerCode = "CUST-001",
                    Name = "Acme",
                    IsActive = true,
                }
            );

            if (includeStockLine)
            {
                db.ItemCategories.Add(
                    new ItemCategory
                    {
                        Id = categoryId,
                        Code = "STK",
                        Name = "Stock",
                        RevenueAccount = "4000",
                    }
                );

                db.Items.Add(
                    new Item
                    {
                        Id = itemId,
                        Sku = "SKU-001",
                        Name = "Stock Item",
                        Type = ItemType.Stock,
                        CategoryId = categoryId,
                        Uom = "EA",
                        DefaultPrice = 100m,
                        DefaultRevenueAccountNo = "4000",
                    }
                );
            }

            db.ArInvoices.Add(
                new ArInvoice
                {
                    Id = invoiceId,
                    DocNo = "AR-2001",
                    CustomerId = customerId,
                    InvoiceDate = new DateOnly(2026, 3, 15),
                    DueDate = new DateOnly(2026, 4, 14),
                    CurrencyCode = currencyCode,
                    TaxTotal = 15m,
                    DocTotal = 115m,
                    DocStatus = DocStatus.Draft,
                    Lines =
                    [
                        new ArInvoiceLine
                        {
                            Id = lineId,
                            LineNo = 1,
                            ItemId = includeStockLine ? itemId : null,
                            Description = includeStockLine ? "Stock line" : "Service line",
                            Qty = 1m,
                            UnitPrice = 100m,
                            RevenueAccount = "4000",
                            LineTotal = 100m,
                        },
                    ],
                }
            );

            await db.SaveChangesAsync();
        });

        return invoiceId;
    }
}
