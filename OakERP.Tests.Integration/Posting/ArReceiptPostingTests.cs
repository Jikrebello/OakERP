using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;
using Shouldly;

namespace OakERP.Tests.Integration.Posting;

[TestFixture]
public sealed class ArReceiptPostingTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task PostAsync_Should_Post_Unapplied_ArReceipt_To_Bank_And_ArControl()
    {
        var receiptId = await SeedReceiptScenarioAsync();

        PostResult result = await PostReceiptAsync(receiptId);

        result.DocKind.ShouldBe(DocKind.ArReceipt);
        result.GlEntryCount.ShouldBe(2);
        result.InventoryEntryCount.ShouldBe(0);

        await WithDbAsync(async db =>
        {
            var receipt = await db.ArReceipts.SingleAsync(x => x.Id == receiptId);
            receipt.DocStatus.ShouldBe(DocStatus.Posted);
            receipt.PostingDate.ShouldBe(new DateOnly(2026, 4, 5));

            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == receiptId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.Count.ShouldBe(2);
            glEntries.Sum(x => x.Debit).ShouldBe(glEntries.Sum(x => x.Credit));
            glEntries.ShouldContain(x => x.AccountNo == "1000" && x.Debit == 125m);
            glEntries.ShouldContain(x => x.AccountNo == "1100" && x.Credit == 125m);
            glEntries
                .All(x => x.SourceType == OakERP.Domain.Posting.PostingSourceTypes.ArReceipt)
                .ShouldBeTrue();
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == receiptId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == receiptId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Post_Partially_Allocated_Receipt_Using_Full_Receipt_Amount()
    {
        var receiptId = await SeedReceiptScenarioAsync(amount: 150m, allocatedAmount: 60m);

        PostResult result = await PostReceiptAsync(receiptId);

        result.GlEntryCount.ShouldBe(2);
        result.InventoryEntryCount.ShouldBe(0);

        await WithDbAsync(async db =>
        {
            var receipt = await db.ArReceipts.SingleAsync(x => x.Id == receiptId);
            receipt.DocStatus.ShouldBe(DocStatus.Posted);

            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == receiptId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.ShouldContain(x => x.AccountNo == "1000" && x.Debit == 150m);
            glEntries.ShouldContain(x => x.AccountNo == "1100" && x.Credit == 150m);
            (await db.ArReceiptAllocations.CountAsync(x => x.ArReceiptId == receiptId)).ShouldBe(1);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_Second_Post_Attempt_Without_Duplicating_Gl()
    {
        var receiptId = await SeedReceiptScenarioAsync();

        await PostReceiptAsync(receiptId);

        await Should.ThrowAsync<PostingInvariantViolationException>(() => PostReceiptAsync(receiptId));

        await WithDbAsync(async db =>
        {
            var receipt = await db.ArReceipts.SingleAsync(x => x.Id == receiptId);
            receipt.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == receiptId)).ShouldBe(2);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == receiptId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Leave_Only_One_Committed_Posting_During_Concurrent_Attempts()
    {
        var receiptId = await SeedReceiptScenarioAsync();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Exception?> AttemptAsync() =>
            Task.Run(async () =>
            {
                await gate.Task;
                try
                {
                    await PostReceiptAsync(receiptId);
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
            var receipt = await db.ArReceipts.SingleAsync(x => x.Id == receiptId);
            receipt.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == receiptId)).ShouldBe(2);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == receiptId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_When_No_Open_Period_Exists()
    {
        var receiptId = await SeedReceiptScenarioAsync(includeOpenPeriod: false);

        await Should.ThrowAsync<PostingInvariantViolationException>(() => PostReceiptAsync(receiptId));

        await AssertNoPostingWrittenAsync(receiptId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_Non_Base_Currency_Receipts()
    {
        var receiptId = await SeedReceiptScenarioAsync(currencyCode: "USD");

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostReceiptAsync(receiptId)
        );

        ex.Message.ShouldContain("base currency");

        await AssertNoPostingWrittenAsync(receiptId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_When_Allocations_Exceed_Receipt_Amount()
    {
        var receiptId = await SeedReceiptScenarioAsync(amount: 100m, allocatedAmount: 120m);

        var ex = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            PostReceiptAsync(receiptId)
        );

        ex.Message.ShouldContain("exceed the receipt amount");

        await AssertNoPostingWrittenAsync(receiptId);
    }

    private async Task<PostResult> PostReceiptAsync(Guid receiptId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPostingService>();

        return await service.PostAsync(
            new PostCommand(DocKind.ArReceipt, receiptId, "integration-user")
        );
    }

    private async Task AssertNoPostingWrittenAsync(Guid receiptId)
    {
        await WithDbAsync(async db =>
        {
            var receipt = await db.ArReceipts.SingleAsync(x => x.Id == receiptId);
            receipt.DocStatus.ShouldBe(DocStatus.Draft);
            receipt.PostingDate.ShouldBeNull();
            (await db.GlEntries.CountAsync(x => x.SourceId == receiptId)).ShouldBe(0);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == receiptId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == receiptId)).ShouldBe(0);
        });
    }

    private async Task<Guid> SeedReceiptScenarioAsync(
        decimal amount = 125m,
        decimal allocatedAmount = 0m,
        bool includeOpenPeriod = true,
        string currencyCode = "ZAR"
    )
    {
        var receiptId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

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
                new GlAccount
                {
                    AccountNo = "1000",
                    Name = "Bank",
                    Type = GlAccountType.Asset,
                    IsActive = true,
                },
                new GlAccount
                {
                    AccountNo = "1100",
                    Name = "Accounts Receivable",
                    Type = GlAccountType.Asset,
                    IsActive = true,
                    IsControl = true,
                }
            );

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

            if (includeOpenPeriod)
            {
                db.FiscalPeriods.Add(
                    new FiscalPeriod
                    {
                        Id = Guid.NewGuid(),
                        FiscalYear = 2026,
                        PeriodNo = 4,
                        PeriodStart = new DateOnly(2026, 4, 1),
                        PeriodEnd = new DateOnly(2026, 4, 30),
                        Status = FiscalPeriodStatuses.Open,
                    }
                );
            }

            db.Customers.Add(
                new Customer
                {
                    Id = customerId,
                    CustomerCode = "CUST-001",
                    Name = "Acme Customer",
                    IsActive = true,
                }
            );

            db.BankAccounts.Add(
                new BankAccount
                {
                    Id = bankAccountId,
                    Name = "Main Bank",
                    GlAccountNo = "1000",
                    OpeningBalance = 0m,
                    CurrencyCode = currencyCode,
                    IsActive = true,
                }
            );

            db.ArReceipts.Add(
                new ArReceipt
                {
                    Id = receiptId,
                    DocNo = "RCPT-POST-1001",
                    CustomerId = customerId,
                    BankAccountId = bankAccountId,
                    ReceiptDate = new DateOnly(2026, 4, 5),
                    Amount = amount,
                    CurrencyCode = currencyCode,
                    DocStatus = DocStatus.Draft,
                }
            );

            if (allocatedAmount > 0m)
            {
                db.ArInvoices.Add(
                    new ArInvoice
                    {
                        Id = invoiceId,
                        DocNo = "ARINV-POST-1001",
                        CustomerId = customerId,
                        InvoiceDate = new DateOnly(2026, 4, 1),
                        DueDate = new DateOnly(2026, 5, 1),
                        PostingDate = new DateOnly(2026, 4, 1),
                        CurrencyCode = currencyCode,
                        TaxTotal = 0m,
                        DocTotal = allocatedAmount,
                        DocStatus = DocStatus.Posted,
                    }
                );

                db.ArReceiptAllocations.Add(
                    new ArReceiptAllocation
                    {
                        ArReceiptId = receiptId,
                        ArInvoiceId = invoiceId,
                        AllocationDate = new DateOnly(2026, 4, 5),
                        AmountApplied = allocatedAmount,
                    }
                );
            }

            await db.SaveChangesAsync();
        });

        return receiptId;
    }
}
