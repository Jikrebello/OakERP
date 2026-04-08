using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.GeneralLedger;
using Shouldly;

namespace OakERP.Tests.Integration.Posting;

[TestFixture]
public sealed class ApInvoicePostingTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task PostAsync_Should_Post_ApInvoice_To_ApControl_Expense_And_TaxInput()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(taxTotal: 15m);
        var postingDate = DaysFromToday(-2);

        PostResult result = await PostInvoiceAsync(invoiceId, postingDate);

        result.DocKind.ShouldBe(DocKind.ApInvoice);
        result.GlEntryCount.ShouldBe(4);
        result.InventoryEntryCount.ShouldBe(0);
        result.PostingDate.ShouldBe(postingDate);

        await WithDbAsync(async db =>
        {
            var invoice = await db.ApInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);

            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == invoiceId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.Count.ShouldBe(4);
            glEntries.Sum(x => x.Debit).ShouldBe(glEntries.Sum(x => x.Credit));
            glEntries.All(x => x.EntryDate == postingDate).ShouldBeTrue();
            glEntries.All(x => x.SourceType == PostingSourceTypes.ApInvoice).ShouldBeTrue();
            glEntries.ShouldContain(x => x.AccountNo == "2000" && x.Credit == 115m);
            glEntries.ShouldContain(x => x.AccountNo == "2200" && x.Debit == 15m);
            glEntries.ShouldContain(x => x.AccountNo == "5000" && x.Debit == 60m);
            glEntries.ShouldContain(x => x.AccountNo == "5100" && x.Debit == 40m);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_Second_Post_Attempt_Without_Duplicating_Gl()
    {
        var invoiceId = await SeedInvoiceScenarioAsync();

        await PostInvoiceAsync(invoiceId);

        await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        await WithDbAsync(async db =>
        {
            var invoice = await db.ApInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(4);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
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
            var invoice = await db.ApInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(4);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
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

    [Test]
    public async Task PostAsync_Should_Reject_When_Item_Lines_Are_Persisted()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(includeItemLine: true);

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        ex.Message.ShouldContain("ItemId");

        await AssertNoPostingWrittenAsync(invoiceId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_When_TaxRate_Lines_Are_Persisted()
    {
        var invoiceId = await SeedInvoiceScenarioAsync(includeTaxRateLine: true);

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostInvoiceAsync(invoiceId)
        );

        ex.Message.ShouldContain("TaxRateId");

        await AssertNoPostingWrittenAsync(invoiceId);
    }

    private async Task<PostResult> PostInvoiceAsync(Guid invoiceId, DateOnly? postingDate = null)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPostingService>();

        return await service.PostAsync(
            new PostCommand(DocKind.ApInvoice, invoiceId, "integration-user", postingDate)
        );
    }

    private async Task AssertNoPostingWrittenAsync(Guid invoiceId)
    {
        await WithDbAsync(async db =>
        {
            var invoice = await db.ApInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            (await db.GlEntries.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == invoiceId)).ShouldBe(0);
        });
    }

    private async Task<Guid> SeedInvoiceScenarioAsync(
        decimal taxTotal = 15m,
        bool includeOpenPeriod = true,
        string currencyCode = "ZAR",
        bool includeItemLine = false,
        bool includeTaxRateLine = false,
        DateOnly? invoiceDate = null
    )
    {
        var invoiceId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();
        var effectiveInvoiceDate = invoiceDate ?? DaysFromToday(-4);
        string vendorCode = $"VEND-{vendorId.ToString("N")[..8].ToUpperInvariant()}";
        string itemSku = $"ITEM-{itemId.ToString("N")[..8].ToUpperInvariant()}";
        string taxRateName = $"Input VAT 15% {taxRateId.ToString("N")[..8].ToUpperInvariant()}";
        string docNo = $"APINV-{invoiceId.ToString("N")[..8].ToUpperInvariant()}";
        string invoiceNo = $"SUP-{invoiceId.ToString("N")[..8].ToUpperInvariant()}";

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

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "2000"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "2000",
                        Name = "Accounts Payable",
                        Type = GlAccountType.Liability,
                        IsActive = true,
                        IsControl = true,
                    }
                );
            }

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "2200"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "2200",
                        Name = "Input VAT",
                        Type = GlAccountType.Asset,
                        IsActive = true,
                    }
                );
            }

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "5000"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "5000",
                        Name = "Office Expense",
                        Type = GlAccountType.Expense,
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
                        Name = "Travel Expense",
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

            db.Vendors.Add(
                new Vendor
                {
                    Id = vendorId,
                    VendorCode = vendorCode,
                    Name = "Acme Supplier",
                    IsActive = true,
                }
            );

            if (includeItemLine)
            {
                db.Items.Add(
                    new Item
                    {
                        Id = itemId,
                        Sku = itemSku,
                        Name = "Deferred Item",
                        Type = ItemType.Stock,
                        DefaultExpenseAccountNo = "5000",
                        IsActive = true,
                    }
                );
            }

            if (includeTaxRateLine)
            {
                db.TaxRates.Add(
                    new TaxRate
                    {
                        Id = taxRateId,
                        Name = taxRateName,
                        RatePercent = 15m,
                        IsInput = true,
                        EffectiveFrom = DaysFromToday(-100),
                        IsActive = true,
                    }
                );
            }

            db.ApInvoices.Add(
                new ApInvoice
                {
                    Id = invoiceId,
                    DocNo = docNo,
                    VendorId = vendorId,
                    InvoiceNo = invoiceNo,
                    InvoiceDate = effectiveInvoiceDate,
                    DueDate = effectiveInvoiceDate.AddDays(30),
                    CurrencyCode = currencyCode,
                    TaxTotal = taxTotal,
                    DocTotal = 100m + taxTotal,
                    DocStatus = DocStatus.Draft,
                    Lines =
                    [
                        new ApInvoiceLine
                        {
                            LineNo = 1,
                            AccountNo = "5000",
                            ItemId = includeItemLine ? itemId : null,
                            Qty = 1m,
                            UnitPrice = 60m,
                            LineTotal = 60m,
                        },
                        new ApInvoiceLine
                        {
                            LineNo = 2,
                            AccountNo = "5100",
                            TaxRateId = includeTaxRateLine ? taxRateId : null,
                            Qty = 1m,
                            UnitPrice = 40m,
                            LineTotal = 40m,
                        },
                    ],
                }
            );

            await db.SaveChangesAsync();
        });

        return invoiceId;
    }
}
