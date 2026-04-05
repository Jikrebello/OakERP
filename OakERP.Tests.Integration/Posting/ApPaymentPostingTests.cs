using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Tests.Integration.TestSetup;
using Shouldly;

namespace OakERP.Tests.Integration.Posting;

[TestFixture]
public sealed class ApPaymentPostingTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task PostAsync_Should_Post_Unapplied_ApPayment_To_Bank_And_ApControl()
    {
        var paymentId = await SeedPaymentScenarioAsync(amount: 125m, allocatedAmount: 0m);
        var postingDate = new DateOnly(2026, 4, 10);

        PostResult result = await PostPaymentAsync(paymentId, postingDate);

        result.DocKind.ShouldBe(DocKind.ApPayment);
        result.GlEntryCount.ShouldBe(2);
        result.InventoryEntryCount.ShouldBe(0);
        result.PostingDate.ShouldBe(postingDate);

        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.Id == paymentId);
            payment.DocStatus.ShouldBe(DocStatus.Posted);
            payment.PostingDate.ShouldBe(postingDate);

            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == paymentId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.Count.ShouldBe(2);
            glEntries.Sum(x => x.Debit).ShouldBe(glEntries.Sum(x => x.Credit));
            glEntries.All(x => x.EntryDate == postingDate).ShouldBeTrue();
            glEntries.All(x => x.SourceType == PostingSourceTypes.ApPayment).ShouldBeTrue();
            glEntries.ShouldContain(x => x.AccountNo == "1000" && x.Debit == 125m);
            glEntries.ShouldContain(x => x.AccountNo == "2000" && x.Credit == 125m);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Post_Partially_Allocated_Payment_Using_Full_Payment_Amount()
    {
        var paymentId = await SeedPaymentScenarioAsync(amount: 150m, allocatedAmount: 60m);

        PostResult result = await PostPaymentAsync(paymentId);

        result.GlEntryCount.ShouldBe(2);
        result.InventoryEntryCount.ShouldBe(0);

        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.Id == paymentId);
            payment.DocStatus.ShouldBe(DocStatus.Posted);
            payment.PostingDate.ShouldBe(payment.PaymentDate);

            var glEntries = await db
                .GlEntries.Where(x => x.SourceId == paymentId)
                .OrderBy(x => x.AccountNo)
                .ToListAsync();

            glEntries.ShouldContain(x => x.AccountNo == "1000" && x.Debit == 150m);
            glEntries.ShouldContain(x => x.AccountNo == "2000" && x.Credit == 150m);
            (await db.ApPaymentAllocations.CountAsync(x => x.ApPaymentId == paymentId)).ShouldBe(1);
            var invoiceId = await db
                .ApPaymentAllocations.Where(x => x.ApPaymentId == paymentId)
                .Select(x => x.ApInvoiceId)
                .SingleAsync();
            var invoice = await db.ApInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_Second_Post_Attempt_Without_Duplicating_Gl()
    {
        var paymentId = await SeedPaymentScenarioAsync(amount: 100m, allocatedAmount: 0m);

        await PostPaymentAsync(paymentId);

        await Should.ThrowAsync<InvalidOperationException>(() => PostPaymentAsync(paymentId));

        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.Id == paymentId);
            payment.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == paymentId)).ShouldBe(2);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Leave_Only_One_Committed_Posting_During_Concurrent_Attempts()
    {
        var paymentId = await SeedPaymentScenarioAsync(amount: 100m, allocatedAmount: 0m);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Exception?> AttemptAsync() =>
            Task.Run(async () =>
            {
                await gate.Task;
                try
                {
                    await PostPaymentAsync(paymentId);
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
            var payment = await db.ApPayments.SingleAsync(x => x.Id == paymentId);
            payment.DocStatus.ShouldBe(DocStatus.Posted);
            (await db.GlEntries.CountAsync(x => x.SourceId == paymentId)).ShouldBe(2);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
        });
    }

    [Test]
    public async Task PostAsync_Should_Reject_When_No_Open_Period_Exists()
    {
        var paymentId = await SeedPaymentScenarioAsync(
            amount: 100m,
            allocatedAmount: 0m,
            includeOpenPeriod: false
        );

        await Should.ThrowAsync<InvalidOperationException>(() => PostPaymentAsync(paymentId));

        await AssertNoPostingWrittenAsync(paymentId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_Non_Base_Currency_Payments()
    {
        var paymentId = await SeedPaymentScenarioAsync(
            amount: 100m,
            allocatedAmount: 0m,
            bankCurrencyCode: "USD"
        );

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            PostPaymentAsync(paymentId)
        );

        ex.Message.ShouldContain("base currency");

        await AssertNoPostingWrittenAsync(paymentId);
    }

    [Test]
    public async Task PostAsync_Should_Reject_When_Allocations_Exceed_Payment_Amount()
    {
        var paymentId = await SeedPaymentScenarioAsync(amount: 100m, allocatedAmount: 120m);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            PostPaymentAsync(paymentId)
        );

        ex.Message.ShouldContain("exceed the payment amount");

        await AssertNoPostingWrittenAsync(paymentId);
    }

    private async Task<PostResult> PostPaymentAsync(Guid paymentId, DateOnly? postingDate = null)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPostingService>();

        return await service.PostAsync(
            new PostCommand(DocKind.ApPayment, paymentId, "integration-user", postingDate)
        );
    }

    private async Task AssertNoPostingWrittenAsync(Guid paymentId)
    {
        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.Id == paymentId);
            payment.DocStatus.ShouldBe(DocStatus.Draft);
            payment.PostingDate.ShouldBeNull();
            (await db.GlEntries.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
            (await db.InventoryLedgers.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
            (await db.BankTransactions.CountAsync(x => x.SourceId == paymentId)).ShouldBe(0);
        });
    }

    private async Task<Guid> SeedPaymentScenarioAsync(
        decimal amount,
        decimal allocatedAmount,
        bool includeOpenPeriod = true,
        string bankCurrencyCode = "ZAR"
    )
    {
        var paymentId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        Guid? invoiceId = allocatedAmount > 0m ? Guid.NewGuid() : null;
        decimal invoiceTotal = allocatedAmount > 0m ? allocatedAmount + 40m : 0m;

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
                    AccountNo = "2000",
                    Name = "Accounts Payable",
                    Type = GlAccountType.Liability,
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

            db.Vendors.Add(
                new Vendor
                {
                    Id = vendorId,
                    VendorCode = "VEND-POST-001",
                    Name = "Posting Vendor",
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
                    CurrencyCode = bankCurrencyCode,
                    IsActive = true,
                }
            );

            if (invoiceId is not null)
            {
                db.ApInvoices.Add(
                    new ApInvoice
                    {
                        Id = invoiceId.Value,
                        DocNo = "APINV-PAY-1001",
                        VendorId = vendorId,
                        InvoiceNo = "SUP-PAY-1001",
                        InvoiceDate = new DateOnly(2026, 4, 1),
                        DueDate = new DateOnly(2026, 5, 1),
                        CurrencyCode = "ZAR",
                        TaxTotal = 0m,
                        DocTotal = invoiceTotal,
                        DocStatus = DocStatus.Posted,
                    }
                );
            }

            var payment = new ApPayment
            {
                Id = paymentId,
                DocNo = "APPAY-POST-1001",
                VendorId = vendorId,
                BankAccountId = bankAccountId,
                PaymentDate = new DateOnly(2026, 4, 5),
                Amount = amount,
                DocStatus = DocStatus.Draft,
            };

            if (invoiceId is not null)
            {
                payment.Allocations.Add(
                    new ApPaymentAllocation
                    {
                        ApInvoiceId = invoiceId.Value,
                        AllocationDate = payment.PaymentDate,
                        AmountApplied = allocatedAmount,
                    }
                );
            }

            db.ApPayments.Add(payment);

            await db.SaveChangesAsync();
        });

        return paymentId;
    }
}
