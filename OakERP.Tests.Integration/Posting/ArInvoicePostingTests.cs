using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting.GeneralLedger;
using Shouldly;

namespace OakERP.Tests.Integration.Posting;

[TestFixture]
public sealed class ArInvoicePostingTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task PostAsync_Should_Keep_NonStock_Posting_Behavior()
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
            invoice.PostingDate.ShouldBe(invoice.InvoiceDate);

            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == invoiceId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.Count.ShouldBe(3);
            glEntries.Sum(x => x.Debit).ShouldBe(glEntries.Sum(x => x.Credit));
            glEntries.ShouldContain(x => x.AccountNo == "1100" && x.Debit == 115m);
            glEntries.ShouldContain(x => x.AccountNo == "4000" && x.Credit == 100m);
            glEntries.ShouldContain(x => x.AccountNo == "2100" && x.Credit == 15m);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Write_Stock_Gl_And_Inventory_Effects()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeStockLine: true,
            includeServiceLine: false,
            includeLocation: true,
            includeCostHistory: true
        );

        PostResult result = await PostInvoiceAsync(invoiceId);

        result.GlEntryCount.ShouldBe(5);
        result.InventoryEntryCount.ShouldBe(1);

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);

            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == invoiceId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.Count.ShouldBe(5);
            glEntries.ShouldContain(x => x.AccountNo == "1100" && x.Debit == 115m);
            glEntries.ShouldContain(x => x.AccountNo == "4000" && x.Credit == 100m);
            glEntries.ShouldContain(x => x.AccountNo == "2100" && x.Credit == 15m);
            glEntries.ShouldContain(x => x.AccountNo == "5100" && x.Debit == 10m);
            glEntries.ShouldContain(x => x.AccountNo == "1300" && x.Credit == 10m);

            var inventoryRows = await db
                .InventoryLedgers.Where(x => x.SourceId == invoiceId)
                .ToListAsync();
            inventoryRows.Count.ShouldBe(1);
            inventoryRows.Single().TransactionType.ShouldBe(InventoryTransactionType.SalesCogs);
            inventoryRows.Single().Qty.ShouldBe(-1m);
            inventoryRows.Single().UnitCost.ShouldBe(10m);
            inventoryRows.Single().ValueChange.ShouldBe(-10m);
        });
    }

    [Test]
    public async Task PostAsync_Should_Post_Mixed_Invoice_With_Inventory_Effects_Only_For_Stock_Lines()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeStockLine: true,
            includeServiceLine: true,
            includeLocation: true,
            includeCostHistory: true,
            stockLineTotal: 50m
        );

        PostResult result = await PostInvoiceAsync(invoiceId);

        result.GlEntryCount.ShouldBe(6);
        result.InventoryEntryCount.ShouldBe(1);

        await WithDbAsync(async db =>
        {
            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == invoiceId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.ShouldContain(x => x.AccountNo == "4000" && x.Credit == 100m);
            glEntries.ShouldContain(x => x.AccountNo == "4000" && x.Credit == 50m);
            glEntries.ShouldContain(x => x.AccountNo == "5100" && x.Debit == 10m);
            glEntries.ShouldContain(x => x.AccountNo == "1300" && x.Credit == 10m);

            var inventoryRows = await db
                .InventoryLedgers.Where(x => x.SourceId == invoiceId)
                .ToListAsync();
            inventoryRows.Count.ShouldBe(1);
            inventoryRows.Single().Qty.ShouldBe(-1m);
        });
    }

    [Test]
    public async Task PostAsync_Should_Allow_Negative_Stock_When_Prior_Cost_Basis_Exists()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeStockLine: true,
            includeServiceLine: false,
            includeLocation: true,
            includeCostHistory: true,
            stockQty: 10m
        );

        PostResult result = await PostInvoiceAsync(invoiceId);

        result.GlEntryCount.ShouldBe(5);
        result.InventoryEntryCount.ShouldBe(1);

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);

            var inventoryRow = await db.InventoryLedgers.SingleAsync(x => x.SourceId == invoiceId);
            inventoryRow.Qty.ShouldBe(-10m);
            inventoryRow.UnitCost.ShouldBe(10m);
            inventoryRow.ValueChange.ShouldBe(-100m);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_Stock_Invoice_Without_Location_Transactionally()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeStockLine: true,
            includeServiceLine: false,
            includeLocation: false,
            includeCostHistory: false
        );

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        ex.Message.ShouldContain("requires a location");

        await AssertNoPostingWrittenAsync(invoiceId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_Stock_Invoice_Without_Prior_Cost_Basis_Transactionally()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeStockLine: true,
            includeServiceLine: false,
            includeLocation: true,
            includeCostHistory: false
        );

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        ex.Message.ShouldContain("prior cost basis");

        await AssertNoPostingWrittenAsync(invoiceId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_Second_Post_Attempt_Without_Duplicating_Gl_Or_Inventory()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeStockLine: true,
            includeServiceLine: false,
            includeLocation: true,
            includeCostHistory: true
        );

        await PostInvoiceAsync(invoiceId);

        await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(5);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(1);
        });
    }

    [Test]
    public async Task PostAsync_Should_Leave_Only_One_Committed_Posting_During_Concurrent_Stock_Attempts()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeStockLine: true,
            includeServiceLine: false,
            includeLocation: true,
            includeCostHistory: true
        );
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
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(5);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(1);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_When_No_Open_Period_Exists()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(
            includeOpenPeriod: false,
            invoiceDate: DaysFromToday(40)
        );

        await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        await AssertNoPostingWrittenAsync(invoiceId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_Non_Base_Currency_Invoices()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(currencyCode: "USD");

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        ex.Message.ShouldContain("base currency");

        await AssertNoPostingWrittenAsync(invoiceId);
    }

    private async Task<PostResult> PostInvoiceAsync(Guid invoiceId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPostingService>();

        return await service.PostAsync(
            new PostCommand(DocKind.ArInvoice, invoiceId, "integration-user")
        );
    }

    private async Task AssertNoPostingWrittenAsync(Guid invoiceId)
    {
        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            invoice.PostingDate.ShouldBeNull();
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
        });
    }

    private async Task<Guid> SeedInvoiceScenarioAsync(
        bool includeStockLine = false,
        bool includeServiceLine = true,
        bool includeOpenPeriod = true,
        string currencyCode = "ZAR",
        bool includeLocation = true,
        bool includeCostHistory = false,
        decimal stockQty = 1m,
        decimal serviceLineTotal = 100m,
        decimal stockLineTotal = 100m,
        DateOnly? invoiceDate = null
    )
    {
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var stockItemId = Guid.NewGuid();
        var stockCategoryId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var effectiveInvoiceDate = invoiceDate ?? DaysFromToday(-10);
        string customerCode = $"CUST-{customerId.ToString("N")[..8].ToUpperInvariant()}";
        string stockCategoryCode = $"STK-{stockCategoryId.ToString("N")[..8].ToUpperInvariant()}";
        string stockCategoryName = $"Stock {stockCategoryId.ToString("N")[..8].ToUpperInvariant()}";
        string stockItemSku = $"SKU-{stockItemId.ToString("N")[..8].ToUpperInvariant()}";
        string locationCode = $"LOC-{locationId.ToString("N")[..8].ToUpperInvariant()}";
        string docNo = $"AR-{invoiceId.ToString("N")[..8].ToUpperInvariant()}";

        await WithDbAsync(async db =>
        {
            if (!await db.Currencies.AnyAsync(x => x.Code == "ZAR"))
            {
                db.Currencies.Add(
                    new Currency
                    {
                        Code = "ZAR",
                        NumericCode = 710,
                        Name = "Rand",
                        Symbol = "R",
                        Decimals = 2,
                        IsActive = true,
                    }
                );
            }

            if (!await db.Currencies.AnyAsync(x => x.Code == "USD"))
            {
                db.Currencies.Add(
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
            }

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "1100"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "1100",
                        Name = "Accounts Receivable",
                        Type = GlAccountType.Asset,
                        IsActive = true,
                        IsControl = true,
                    }
                );
            }

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "1300"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "1300",
                        Name = "Inventory",
                        Type = GlAccountType.Asset,
                        IsActive = true,
                    }
                );
            }

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "2100"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "2100",
                        Name = "Output VAT",
                        Type = GlAccountType.Liability,
                        IsActive = true,
                    }
                );
            }

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "4000"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "4000",
                        Name = "Sales",
                        Type = GlAccountType.Revenue,
                        IsActive = true,
                    }
                );
            }

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "5100"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "5100",
                        Name = "COGS",
                        Type = GlAccountType.Expense,
                        IsActive = true,
                    }
                );
            }

            if (!await db.AppSettings.AnyAsync(x => x.Key == GlPostingSettingsKeys.Posting))
            {
                db.AppSettings.Add(
                    new AppSetting
                    {
                        Key = GlPostingSettingsKeys.Posting,
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
            }

            if (includeOpenPeriod)
            {
                if (
                    !await db.FiscalPeriods.AnyAsync(x =>
                        x.FiscalYear == effectiveInvoiceDate.Year
                        && x.PeriodNo == effectiveInvoiceDate.Month
                    )
                )
                {
                    db.FiscalPeriods.Add(
                        new FiscalPeriod
                        {
                            Id = Guid.NewGuid(),
                            FiscalYear = effectiveInvoiceDate.Year,
                            PeriodNo = effectiveInvoiceDate.Month,
                            PeriodStart = StartOfMonth(effectiveInvoiceDate),
                            PeriodEnd = EndOfMonth(effectiveInvoiceDate),
                            Status = FiscalPeriodStatuses.Open,
                        }
                    );
                }
            }

            db.Customers.Add(
                new Customer
                {
                    Id = customerId,
                    CustomerCode = customerCode,
                    Name = "Acme",
                    IsActive = true,
                }
            );

            if (includeStockLine)
            {
                db.ItemCategories.Add(
                    new ItemCategory
                    {
                        Id = stockCategoryId,
                        Code = stockCategoryCode,
                        Name = stockCategoryName,
                        RevenueAccount = "4000",
                        CogsAccount = "5100",
                        InventoryAccount = "1300",
                    }
                );

                db.Items.Add(
                    new Item
                    {
                        Id = stockItemId,
                        Sku = stockItemSku,
                        Name = "Stock Item",
                        Type = ItemType.Stock,
                        CategoryId = stockCategoryId,
                        Uom = "EA",
                        DefaultPrice = 100m,
                        DefaultRevenueAccountNo = "4000",
                    }
                );

                if (includeLocation)
                {
                    db.Locations.Add(
                        new Location
                        {
                            Id = locationId,
                            Code = locationCode,
                            Name = "Main Warehouse",
                            IsActive = true,
                        }
                    );
                }

                if (includeCostHistory && includeLocation)
                {
                    db.InventoryLedgers.Add(
                        new InventoryLedger
                        {
                            Id = Guid.NewGuid(),
                            TrxDate = effectiveInvoiceDate.AddDays(-5),
                            ItemId = stockItemId,
                            LocationId = locationId,
                            TransactionType = InventoryTransactionType.Receipt,
                            Qty = 5m,
                            UnitCost = 10m,
                            ValueChange = 50m,
                            SourceType = "TEST",
                            SourceId = Guid.NewGuid(),
                            Note = "Seed receipt",
                            CreatedBy = "integration-seed",
                        }
                    );
                }
            }

            var lines = new List<ArInvoiceLine>();
            int lineNo = 1;
            decimal netTotal = 0m;
            decimal taxTotal = 15m;

            if (includeServiceLine)
            {
                lines.Add(
                    new ArInvoiceLine
                    {
                        Id = Guid.NewGuid(),
                        LineNo = lineNo++,
                        Description = "Service line",
                        Qty = 1m,
                        UnitPrice = serviceLineTotal,
                        RevenueAccount = "4000",
                        LineTotal = serviceLineTotal,
                    }
                );
                netTotal += serviceLineTotal;
            }

            if (includeStockLine)
            {
                lines.Add(
                    new ArInvoiceLine
                    {
                        Id = Guid.NewGuid(),
                        LineNo = lineNo++,
                        ItemId = stockItemId,
                        LocationId = includeLocation ? locationId : null,
                        Description = "Stock line",
                        Qty = stockQty,
                        UnitPrice = stockLineTotal / stockQty,
                        RevenueAccount = "4000",
                        LineTotal = stockLineTotal,
                    }
                );
                netTotal += stockLineTotal;
            }

            db.ArInvoices.Add(
                new ArInvoice
                {
                    Id = invoiceId,
                    DocNo = docNo,
                    CustomerId = customerId,
                    InvoiceDate = effectiveInvoiceDate,
                    DueDate = effectiveInvoiceDate.AddDays(30),
                    CurrencyCode = currencyCode,
                    TaxTotal = taxTotal,
                    DocTotal = netTotal + taxTotal,
                    DocStatus = DocStatus.Draft,
                    Lines = lines,
                }
            );

            await db.SaveChangesAsync();
        });

        return invoiceId;
    }
}
