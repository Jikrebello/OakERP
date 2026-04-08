using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;
using Shouldly;

namespace OakERP.Tests.Integration.AccountsPayable;

[TestFixture]
public sealed class ApPaymentApiTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task Create_Endpoint_Should_Create_Unapplied_Draft_Payment()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(vendorId, bankAccountId);

        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-3001",
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = new DateOnly(2026, 4, 5),
            Amount = 125m,
            Memo = "Unapplied vendor payment",
        };

        var result = await PostAsync<CreateApPaymentCommand, ApPaymentCommandResultDto>(
            ApiRoutes.ApPayments.Create,
            command
        );

        result.Success.ShouldBeTrue();
        result.Payment.ShouldNotBeNull();
        result.Payment!.DocStatus.ShouldBe(DocStatus.Draft);
        result.Payment.AllocatedAmount.ShouldBe(0m);
        result.Payment.UnappliedAmount.ShouldBe(125m);

        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.DocNo == command.DocNo);
            payment.DocStatus.ShouldBe(DocStatus.Draft);
            payment.PostingDate.ShouldBeNull();
            (await db.ApPaymentAllocations.CountAsync(x => x.ApPaymentId == payment.Id)).ShouldBe(
                0
            );
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Create_Payment_With_Multiple_Allocations_And_Close_Invoices()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(vendorId, bankAccountId);

        var invoiceA = await SeedPostedInvoiceAsync(vendorId, "APINV-3001", 100m);
        var invoiceB = await SeedPostedInvoiceAsync(vendorId, "APINV-3002", 50m);

        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-3002",
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = new DateOnly(2026, 4, 5),
            Amount = 150m,
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceA, AmountApplied = 100m },
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceB, AmountApplied = 50m },
            ],
        };

        var result = await PostAsync<CreateApPaymentCommand, ApPaymentCommandResultDto>(
            ApiRoutes.ApPayments.Create,
            command
        );

        result.Success.ShouldBeTrue();
        result.Payment.ShouldNotBeNull();
        result.Payment!.AllocatedAmount.ShouldBe(150m);
        result.Payment.UnappliedAmount.ShouldBe(0m);
        result.Invoices.Count.ShouldBe(2);
        result.Invoices.All(x => x.DocStatus == DocStatus.Closed).ShouldBeTrue();

        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.DocNo == command.DocNo);
            payment.DocStatus.ShouldBe(DocStatus.Draft);

            var allocations = await db
                .ApPaymentAllocations.Where(x => x.ApPaymentId == payment.Id)
                .OrderBy(x => x.AmountApplied)
                .ToListAsync();

            allocations.Count.ShouldBe(2);
            allocations.Sum(x => x.AmountApplied).ShouldBe(150m);

            var invoices = await db
                .ApInvoices.Where(x => x.Id == invoiceA || x.Id == invoiceB)
                .OrderBy(x => x.DocNo)
                .ToListAsync();

            invoices.All(x => x.DocStatus == DocStatus.Closed).ShouldBeTrue();
        });
    }

    [Test]
    public async Task Allocate_Endpoint_Should_Apply_To_Existing_Draft_Payment_And_Leave_Invoice_Posted_When_Partial()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(vendorId, bankAccountId);

        var invoiceId = await SeedPostedInvoiceAsync(vendorId, "APINV-3003", 100m);
        var paymentId = await SeedDraftPaymentAsync(vendorId, bankAccountId, "APPAY-3003", 90m);

        var command = new AllocateApPaymentCommand
        {
            AllocationDate = new DateOnly(2026, 4, 6),
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceId, AmountApplied = 60m },
            ],
        };

        var result = await PostAsync<AllocateApPaymentCommand, ApPaymentCommandResultDto>(
            ApiRoutes.ApPayments.Allocate(paymentId),
            command
        );

        result.Success.ShouldBeTrue();
        result.Payment.ShouldNotBeNull();
        result.Payment!.AllocatedAmount.ShouldBe(60m);
        result.Payment.UnappliedAmount.ShouldBe(30m);
        result.Invoices.Single().DocStatus.ShouldBe(DocStatus.Posted);
        result.Invoices.Single().RemainingAmount.ShouldBe(40m);

        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.Id == paymentId);
            payment.DocStatus.ShouldBe(DocStatus.Draft);

            var invoice = await db.ApInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);

            var allocations = await db
                .ApPaymentAllocations.Where(x => x.ApPaymentId == paymentId)
                .ToListAsync();
            allocations.Count.ShouldBe(1);
            allocations.Single().AmountApplied.ShouldBe(60m);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Vendor_Mismatch_Without_Partial_Writes()
    {
        await AuthenticateAsync();

        var paymentVendorId = Guid.NewGuid();
        var invoiceVendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(paymentVendorId, bankAccountId);
        await SeedVendorAsync(invoiceVendorId, "VEND-002", "Other Vendor");
        var invoiceId = await SeedPostedInvoiceAsync(invoiceVendorId, "APINV-3004", 80m);

        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-3004",
            VendorId = paymentVendorId,
            BankAccountId = bankAccountId,
            PaymentDate = new DateOnly(2026, 4, 5),
            Amount = 80m,
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceId, AmountApplied = 50m },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ApPayments.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApPaymentCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("same vendor");

        await WithDbAsync(async db =>
        {
            (await db.ApPayments.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (await db.ApPaymentAllocations.CountAsync()).ShouldBe(0);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_When_Allocations_Exceed_Payment_Amount_Transactionally()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(vendorId, bankAccountId);
        var invoiceId = await SeedPostedInvoiceAsync(vendorId, "APINV-3005", 100m);

        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-3005",
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = new DateOnly(2026, 4, 5),
            Amount = 80m,
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceId, AmountApplied = 90m },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ApPayments.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApPaymentCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("unapplied amount");

        await WithDbAsync(async db =>
        {
            (await db.ApPayments.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (await db.ApPaymentAllocations.CountAsync()).ShouldBe(0);
            (await db.ApInvoices.SingleAsync(x => x.Id == invoiceId)).DocStatus.ShouldBe(
                DocStatus.Posted
            );
        });
    }

    [Test]
    public async Task Allocate_Endpoint_Should_Reject_When_Allocation_Exceeds_Invoice_Remaining_Amount()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(vendorId, bankAccountId);
        var invoiceId = await SeedPostedInvoiceAsync(vendorId, "APINV-3006", 100m);
        var paymentId = await SeedDraftPaymentAsync(vendorId, bankAccountId, "APPAY-3006", 120m);

        var command = new AllocateApPaymentCommand
        {
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceId, AmountApplied = 110m },
            ],
        };

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.ApPayments.Allocate(paymentId),
            command
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApPaymentCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("remaining balance");

        await WithDbAsync(async db =>
        {
            (await db.ApPaymentAllocations.CountAsync(x => x.ApPaymentId == paymentId)).ShouldBe(0);
            (await db.ApInvoices.SingleAsync(x => x.Id == invoiceId)).DocStatus.ShouldBe(
                DocStatus.Posted
            );
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Non_Base_Currency_Payments()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(vendorId, bankAccountId, bankCurrencyCode: "USD");

        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-3007",
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = new DateOnly(2026, 4, 5),
            Amount = 50m,
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ApPayments.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApPaymentCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("base currency");

        await WithDbAsync(async db =>
        {
            (await db.ApPayments.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
        });
    }

    private async Task AuthenticateAsync()
    {
        var registerDto = new RegisterDto
        {
            Email = $"ap_payment_api_{TestId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "AP",
            LastName = "Tester",
            PhoneNumber = "123456789",
            TenantName = $"ApPaymentTenant_{TestId}",
        };

        var registerResult = await PostAsync<RegisterDto, AuthResultDto>(
            ApiRoutes.Auth.Register,
            registerDto
        );
        registerResult.Success.ShouldBeTrue();

        var loginResult = await PostAsync<LoginDto, AuthResultDto>(
            ApiRoutes.Auth.Login,
            new LoginDto { Email = registerDto.Email, Password = registerDto.Password }
        );

        loginResult.Success.ShouldBeTrue();
        loginResult.Token.ShouldNotBeNullOrEmpty();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResult.Token
        );
    }

    private async Task SeedReferenceDataAsync(
        Guid vendorId,
        Guid bankAccountId,
        string bankCurrencyCode = "ZAR",
        bool vendorActive = true,
        bool bankActive = true
    )
    {
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

            db.GlAccounts.Add(
                new GlAccount
                {
                    AccountNo = "1000",
                    Name = "Bank",
                    Type = GlAccountType.Asset,
                    IsActive = true,
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

            db.Vendors.Add(
                new Vendor
                {
                    Id = vendorId,
                    VendorCode = "VEND-001",
                    Name = "Acme Vendor",
                    TermsDays = 30,
                    IsActive = vendorActive,
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
                    IsActive = bankActive,
                }
            );

            await db.SaveChangesAsync();
        });
    }

    private async Task SeedVendorAsync(Guid vendorId, string vendorCode, string name)
    {
        await WithDbAsync(async db =>
        {
            db.Vendors.Add(
                new Vendor
                {
                    Id = vendorId,
                    VendorCode = vendorCode,
                    Name = name,
                    TermsDays = 30,
                    IsActive = true,
                }
            );

            await db.SaveChangesAsync();
        });
    }

    private async Task<Guid> SeedPostedInvoiceAsync(Guid vendorId, string docNo, decimal docTotal)
    {
        var invoiceId = Guid.NewGuid();

        await WithDbAsync(async db =>
        {
            db.ApInvoices.Add(
                new ApInvoice
                {
                    Id = invoiceId,
                    DocNo = docNo,
                    VendorId = vendorId,
                    InvoiceNo = $"{docNo}-SUP",
                    InvoiceDate = new DateOnly(2026, 4, 1),
                    DueDate = new DateOnly(2026, 5, 1),
                    CurrencyCode = "ZAR",
                    TaxTotal = 0m,
                    DocTotal = docTotal,
                    DocStatus = DocStatus.Posted,
                }
            );

            await db.SaveChangesAsync();
        });

        return invoiceId;
    }

    private async Task<Guid> SeedDraftPaymentAsync(
        Guid vendorId,
        Guid bankAccountId,
        string docNo,
        decimal amount
    )
    {
        var paymentId = Guid.NewGuid();

        await WithDbAsync(async db =>
        {
            db.ApPayments.Add(
                new ApPayment
                {
                    Id = paymentId,
                    DocNo = docNo,
                    VendorId = vendorId,
                    BankAccountId = bankAccountId,
                    PaymentDate = new DateOnly(2026, 4, 5),
                    Amount = amount,
                    DocStatus = DocStatus.Draft,
                }
            );

            await db.SaveChangesAsync();
        });

        return paymentId;
    }
}
