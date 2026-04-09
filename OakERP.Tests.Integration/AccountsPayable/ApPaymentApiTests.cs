using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OakERP.API.Contracts.Posting;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Tests.Integration.Runtime;
using Shouldly;

namespace OakERP.Tests.Integration.AccountsPayable;

[TestFixture]
public sealed class ApPaymentApiTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task Post_Endpoint_Should_Post_Draft_Ap_Payment()
    {
        await AuthenticateAsync();

        var paymentId = await CreateDraftPaymentAsync();

        var response = await PostPaymentAsync(paymentId);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PostResult>();
        result.ShouldNotBeNull();
        result!.DocKind.ShouldBe(DocKind.ApPayment);
        result.SourceId.ShouldBe(paymentId);
        result.GlEntryCount.ShouldBeGreaterThan(0);

        await WithDbAsync(async db =>
        {
            var payment = await db.ApPayments.SingleAsync(x => x.Id == paymentId);
            payment.DocStatus.ShouldBe(DocStatus.Posted);
            payment.PostingDate.ShouldBe(result.PostingDate);
            var bankTransaction = await db.BankTransactions.SingleAsync(x =>
                x.SourceId == paymentId
            );
            bankTransaction.BankAccountId.ShouldBe(payment.BankAccountId);
            bankTransaction.TxnDate.ShouldBe(result.PostingDate);
            bankTransaction.Amount.ShouldBe(-payment.Amount);
            bankTransaction.DrAccountNo.ShouldBe("2000");
            bankTransaction.CrAccountNo.ShouldBe("1000");
            bankTransaction.SourceType.ShouldBe(PostingSourceTypes.ApPayment);
            bankTransaction.SourceId.ShouldBe(paymentId);
            bankTransaction.Description.ShouldBe($"AP payment {payment.DocNo}");
            bankTransaction.ExternalRef.ShouldBeNull();
            bankTransaction.IsReconciled.ShouldBeFalse();
        });
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_Conflict_ProblemDetails_When_Payment_Is_Already_Posted()
    {
        await AuthenticateAsync();

        var paymentId = await CreateDraftPaymentAsync();
        (await PostPaymentAsync(paymentId)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var response = await PostPaymentAsync(paymentId);

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Conflict,
            "Posting invariant was violated."
        );
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_Conflict_ProblemDetails_When_No_Open_Period_Exists()
    {
        await AuthenticateAsync();

        var paymentId = await CreateDraftPaymentAsync();

        var response = await PostPaymentAsync(
            paymentId,
            new PostDocumentRequestDto { PostingDate = DaysFromToday(40) }
        );

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Conflict,
            "Posting invariant was violated."
        );
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_BadRequest_ProblemDetails_For_NonBase_Currency()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        string docNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId, bankCurrencyCode: "USD");
        var paymentId = await SeedDraftPaymentAsync(vendorId, bankAccountId, docNo, 75m);

        var response = await PostPaymentAsync(paymentId);

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.BadRequest,
            "Posting invariant was violated."
        );
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_NotFound_ProblemDetails_For_Unknown_Payment()
    {
        await AuthenticateAsync();

        var response = await PostPaymentAsync(Guid.NewGuid());

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "AP payment was not found."
        );
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_BadRequest_ProblemDetails_When_Force_Is_Not_Supported()
    {
        await AuthenticateAsync();

        var paymentId = await CreateDraftPaymentAsync();

        var response = await PostPaymentAsync(
            paymentId,
            new PostDocumentRequestDto { Force = true }
        );

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.BadRequest,
            "The requested operation is not supported."
        );
    }

    [Test]
    public async Task Create_Endpoint_Should_Create_Unapplied_Draft_Payment()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        string docNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId);

        var command = new CreateApPaymentCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = DaysFromToday(-4),
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
        string docNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId);

        var invoiceA = await SeedPostedInvoiceAsync(vendorId, $"APINV-{Guid.NewGuid():N}", 100m);
        var invoiceB = await SeedPostedInvoiceAsync(vendorId, $"APINV-{Guid.NewGuid():N}", 50m);

        var command = new CreateApPaymentCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = DaysFromToday(-4),
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
        string seededInvoiceDocNo = $"APINV-{Guid.NewGuid():N}";
        string paymentDocNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId);

        var invoiceId = await SeedPostedInvoiceAsync(vendorId, seededInvoiceDocNo, 100m);
        var paymentId = await SeedDraftPaymentAsync(vendorId, bankAccountId, paymentDocNo, 90m);

        var command = new AllocateApPaymentCommand
        {
            AllocationDate = DaysFromToday(-3),
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
        string docNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(paymentVendorId, bankAccountId);
        await SeedVendorAsync(invoiceVendorId, $"VEND-{Guid.NewGuid():N}", "Other Vendor");
        var invoiceId = await SeedPostedInvoiceAsync(
            invoiceVendorId,
            $"APINV-{Guid.NewGuid():N}",
            80m
        );

        var command = new CreateApPaymentCommand
        {
            DocNo = docNo,
            VendorId = paymentVendorId,
            BankAccountId = bankAccountId,
            PaymentDate = DaysFromToday(-4),
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
            (
                await db
                    .ApPayments.Where(x => x.DocNo == command.DocNo)
                    .SelectMany(x => x.Allocations)
                    .CountAsync()
            ).ShouldBe(0);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_When_Allocations_Exceed_Payment_Amount_Transactionally()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        string docNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId);
        var invoiceId = await SeedPostedInvoiceAsync(vendorId, $"APINV-{Guid.NewGuid():N}", 100m);

        var command = new CreateApPaymentCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = DaysFromToday(-4),
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
            (
                await db
                    .ApPayments.Where(x => x.DocNo == command.DocNo)
                    .SelectMany(x => x.Allocations)
                    .CountAsync()
            ).ShouldBe(0);
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
        string invoiceDocNo = $"APINV-{Guid.NewGuid():N}";
        string paymentDocNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId);
        var invoiceId = await SeedPostedInvoiceAsync(vendorId, invoiceDocNo, 100m);
        var paymentId = await SeedDraftPaymentAsync(vendorId, bankAccountId, paymentDocNo, 120m);

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
        string docNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId, bankCurrencyCode: "USD");

        var command = new CreateApPaymentCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            BankAccountId = bankAccountId,
            PaymentDate = DaysFromToday(-4),
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
        string authId = Guid.NewGuid().ToString("N")[..8];

        var registerDto = new RegisterDto
        {
            Email = $"ap_payment_api_{TestId}_{authId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "AP",
            LastName = "Tester",
            PhoneNumber = "123456789",
            TenantName = $"ApPaymentTenant_{TestId}_{authId}",
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
        string vendorCode = $"VEND-{vendorId.ToString("N")[..8].ToUpperInvariant()}";
        string bankName = $"Main Bank {bankAccountId.ToString("N")[..8].ToUpperInvariant()}";

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

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "1000"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "1000",
                        Name = "Bank",
                        Type = GlAccountType.Asset,
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

            db.Vendors.Add(
                new Vendor
                {
                    Id = vendorId,
                    VendorCode = vendorCode,
                    Name = "Acme Vendor",
                    TermsDays = 30,
                    IsActive = vendorActive,
                }
            );

            db.BankAccounts.Add(
                new BankAccount
                {
                    Id = bankAccountId,
                    Name = bankName,
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
                    InvoiceNo = $"SUP-{invoiceId:N}",
                    InvoiceDate = DaysFromToday(-8),
                    DueDate = DaysFromToday(-8).AddDays(30),
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
                    PaymentDate = DaysFromToday(-4),
                    Amount = amount,
                    DocStatus = DocStatus.Draft,
                }
            );

            await db.SaveChangesAsync();
        });

        return paymentId;
    }

    private async Task<Guid> CreateDraftPaymentAsync()
    {
        var vendorId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        string docNo = $"APPAY-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, bankAccountId);

        var result = await PostAsync<CreateApPaymentCommand, ApPaymentCommandResultDto>(
            ApiRoutes.ApPayments.Create,
            new CreateApPaymentCommand
            {
                DocNo = docNo,
                VendorId = vendorId,
                BankAccountId = bankAccountId,
                PaymentDate = DaysFromToday(-4),
                Amount = 125m,
                Memo = "Posting transport test payment",
            }
        );

        result.Success.ShouldBeTrue();
        result.Payment.ShouldNotBeNull();
        return result.Payment!.PaymentId;
    }

    private Task<HttpResponseMessage> PostPaymentAsync(
        Guid paymentId,
        PostDocumentRequestDto? request = null
    ) => Client.PostAsJsonAsync(ApiRoutes.ApPayments.Post(paymentId), request ?? new());

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle
    )
    {
        response.StatusCode.ShouldBe(expectedStatusCode);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await RuntimeSupportTestJson.ReadJsonAsync(response);
        body.GetProperty("status").GetInt32().ShouldBe((int)expectedStatusCode);
        body.GetProperty("title").GetString().ShouldBe(expectedTitle);
        body.GetProperty("type")
            .GetString()
            .ShouldBe($"https://httpstatuses.com/{(int)expectedStatusCode}");
        body.GetProperty("correlationId").GetString().ShouldNotBeNullOrWhiteSpace();
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
    }
}
