using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Bank;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;
using Shouldly;

namespace OakERP.Tests.Integration.AccountsReceivable;

[TestFixture]
public sealed class ArReceiptApiTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task Create_Endpoint_Should_Create_Unapplied_Draft_Receipt()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(customerId, bankAccountId);

        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-3001",
            CustomerId = customerId,
            BankAccountId = bankAccountId,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 125m,
            CurrencyCode = "ZAR",
            Memo = "Unapplied cash",
        };

        var result = await PostAsync<CreateArReceiptCommand, ArReceiptCommandResultDto>(
            ApiRoutes.ArReceipts.Create,
            command
        );

        result.Success.ShouldBeTrue();
        result.Receipt.ShouldNotBeNull();
        result.Receipt!.DocStatus.ShouldBe(DocStatus.Draft);
        result.Receipt.AllocatedAmount.ShouldBe(0m);
        result.Receipt.UnappliedAmount.ShouldBe(125m);

        await WithDbAsync(async db =>
        {
            var receipt = await db.ArReceipts.SingleAsync(x => x.DocNo == command.DocNo);
            receipt.DocStatus.ShouldBe(DocStatus.Draft);
            receipt.PostingDate.ShouldBeNull();
            (await db.ArReceiptAllocations.CountAsync(x => x.ArReceiptId == receipt.Id)).ShouldBe(
                0
            );
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Create_Receipt_With_Multiple_Allocations_And_Close_Invoices()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(customerId, bankAccountId);

        var invoiceA = await SeedPostedInvoiceAsync(customerId, "ARINV-3001", 100m);
        var invoiceB = await SeedPostedInvoiceAsync(customerId, "ARINV-3002", 50m);

        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-3002",
            CustomerId = customerId,
            BankAccountId = bankAccountId,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 150m,
            CurrencyCode = "ZAR",
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceA, AmountApplied = 100m },
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceB, AmountApplied = 50m },
            ],
        };

        var result = await PostAsync<CreateArReceiptCommand, ArReceiptCommandResultDto>(
            ApiRoutes.ArReceipts.Create,
            command
        );

        result.Success.ShouldBeTrue();
        result.Receipt.ShouldNotBeNull();
        result.Receipt!.AllocatedAmount.ShouldBe(150m);
        result.Receipt.UnappliedAmount.ShouldBe(0m);
        result.Invoices.Count.ShouldBe(2);
        result.Invoices.All(x => x.DocStatus == DocStatus.Closed).ShouldBeTrue();

        await WithDbAsync(async db =>
        {
            var receipt = await db.ArReceipts.SingleAsync(x => x.DocNo == command.DocNo);
            receipt.DocStatus.ShouldBe(DocStatus.Draft);

            var allocations = await db
                .ArReceiptAllocations.Where(x => x.ArReceiptId == receipt.Id)
                .OrderBy(x => x.AmountApplied)
                .ToListAsync();

            allocations.Count.ShouldBe(2);
            allocations.Sum(x => x.AmountApplied).ShouldBe(150m);

            var invoices = await db
                .ArInvoices.Where(x => x.Id == invoiceA || x.Id == invoiceB)
                .OrderBy(x => x.DocNo)
                .ToListAsync();

            invoices.All(x => x.DocStatus == DocStatus.Closed).ShouldBeTrue();
        });
    }

    [Test]
    public async Task Allocate_Endpoint_Should_Apply_To_Existing_Draft_Receipt_And_Leave_Invoice_Posted_When_Partial()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(customerId, bankAccountId);

        var invoiceId = await SeedPostedInvoiceAsync(customerId, "ARINV-3003", 100m);
        var receiptId = await SeedDraftReceiptAsync(customerId, bankAccountId, "RCPT-3003", 90m);

        var command = new AllocateArReceiptCommand
        {
            AllocationDate = new DateOnly(2026, 4, 6),
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceId, AmountApplied = 60m },
            ],
        };

        var result = await PostAsync<AllocateArReceiptCommand, ArReceiptCommandResultDto>(
            ApiRoutes.ArReceipts.Allocate(receiptId),
            command
        );

        result.Success.ShouldBeTrue();
        result.Receipt.ShouldNotBeNull();
        result.Receipt!.AllocatedAmount.ShouldBe(60m);
        result.Receipt.UnappliedAmount.ShouldBe(30m);
        result.Invoices.Single().DocStatus.ShouldBe(DocStatus.Posted);
        result.Invoices.Single().RemainingAmount.ShouldBe(40m);

        await WithDbAsync(async db =>
        {
            var receipt = await db.ArReceipts.SingleAsync(x => x.Id == receiptId);
            receipt.DocStatus.ShouldBe(DocStatus.Draft);

            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);

            var allocations = await db
                .ArReceiptAllocations.Where(x => x.ArReceiptId == receiptId)
                .ToListAsync();
            allocations.Count.ShouldBe(1);
            allocations.Single().AmountApplied.ShouldBe(60m);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Customer_Mismatch_Without_Partial_Writes()
    {
        await AuthenticateAsync();

        var receiptCustomerId = Guid.NewGuid();
        var invoiceCustomerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(receiptCustomerId, bankAccountId);
        await SeedCustomerAsync(invoiceCustomerId, "CUST-002", "Other Customer");
        var invoiceId = await SeedPostedInvoiceAsync(invoiceCustomerId, "ARINV-3004", 80m);

        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-3004",
            CustomerId = receiptCustomerId,
            BankAccountId = bankAccountId,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 80m,
            CurrencyCode = "ZAR",
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceId, AmountApplied = 50m },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ArReceipts.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ArReceiptCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("same customer");

        await WithDbAsync(async db =>
        {
            (await db.ArReceipts.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (await db.ArReceiptAllocations.CountAsync()).ShouldBe(0);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_When_Allocations_Exceed_Receipt_Amount_Transactionally()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(customerId, bankAccountId);
        var invoiceId = await SeedPostedInvoiceAsync(customerId, "ARINV-3005", 100m);

        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-3005",
            CustomerId = customerId,
            BankAccountId = bankAccountId,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 80m,
            CurrencyCode = "ZAR",
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceId, AmountApplied = 90m },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ArReceipts.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ArReceiptCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("unapplied amount");

        await WithDbAsync(async db =>
        {
            (await db.ArReceipts.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (await db.ArReceiptAllocations.CountAsync()).ShouldBe(0);
            (await db.ArInvoices.SingleAsync(x => x.Id == invoiceId)).DocStatus.ShouldBe(
                DocStatus.Posted
            );
        });
    }

    [Test]
    public async Task Allocate_Endpoint_Should_Reject_When_Allocation_Exceeds_Invoice_Remaining_Amount()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(customerId, bankAccountId);
        var invoiceId = await SeedPostedInvoiceAsync(customerId, "ARINV-3006", 100m);
        var receiptId = await SeedDraftReceiptAsync(customerId, bankAccountId, "RCPT-3006", 120m);

        var command = new AllocateArReceiptCommand
        {
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceId, AmountApplied = 110m },
            ],
        };

        var response = await Client.PostAsJsonAsync(
            ApiRoutes.ArReceipts.Allocate(receiptId),
            command
        );
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ArReceiptCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("remaining balance");

        await WithDbAsync(async db =>
        {
            (await db.ArReceiptAllocations.CountAsync(x => x.ArReceiptId == receiptId)).ShouldBe(0);
            (await db.ArInvoices.SingleAsync(x => x.Id == invoiceId)).DocStatus.ShouldBe(
                DocStatus.Posted
            );
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Non_Base_Currency_Receipts()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        await SeedReferenceDataAsync(customerId, bankAccountId, bankCurrencyCode: "USD");

        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-3007",
            CustomerId = customerId,
            BankAccountId = bankAccountId,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 50m,
            CurrencyCode = "USD",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ArReceipts.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ArReceiptCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("base currency");

        await WithDbAsync(async db =>
        {
            (await db.ArReceipts.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
        });
    }

    private async Task AuthenticateAsync()
    {
        var registerDto = new RegisterDto
        {
            Email = $"receipt_api_{TestId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "Receipt",
            LastName = "Tester",
            PhoneNumber = "123456789",
            TenantName = $"ReceiptTenant_{TestId}",
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
        Guid customerId,
        Guid bankAccountId,
        string bankCurrencyCode = "ZAR",
        bool customerActive = true,
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

            db.Customers.Add(
                new Customer
                {
                    Id = customerId,
                    CustomerCode = "CUST-001",
                    Name = "Acme Customer",
                    IsActive = customerActive,
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

    private async Task SeedCustomerAsync(Guid customerId, string customerCode, string name)
    {
        await WithDbAsync(async db =>
        {
            db.Customers.Add(
                new Customer
                {
                    Id = customerId,
                    CustomerCode = customerCode,
                    Name = name,
                    IsActive = true,
                }
            );

            await db.SaveChangesAsync();
        });
    }

    private async Task<Guid> SeedPostedInvoiceAsync(Guid customerId, string docNo, decimal docTotal)
    {
        var invoiceId = Guid.NewGuid();

        await WithDbAsync(async db =>
        {
            db.ArInvoices.Add(
                new ArInvoice
                {
                    Id = invoiceId,
                    DocNo = docNo,
                    CustomerId = customerId,
                    InvoiceDate = new DateOnly(2026, 4, 1),
                    DueDate = new DateOnly(2026, 5, 1),
                    PostingDate = new DateOnly(2026, 4, 1),
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

    private async Task<Guid> SeedDraftReceiptAsync(
        Guid customerId,
        Guid bankAccountId,
        string docNo,
        decimal amount
    )
    {
        var receiptId = Guid.NewGuid();

        await WithDbAsync(async db =>
        {
            db.ArReceipts.Add(
                new ArReceipt
                {
                    Id = receiptId,
                    DocNo = docNo,
                    CustomerId = customerId,
                    BankAccountId = bankAccountId,
                    ReceiptDate = new DateOnly(2026, 4, 5),
                    Amount = amount,
                    CurrencyCode = "ZAR",
                    DocStatus = DocStatus.Draft,
                }
            );

            await db.SaveChangesAsync();
        });

        return receiptId;
    }
}
