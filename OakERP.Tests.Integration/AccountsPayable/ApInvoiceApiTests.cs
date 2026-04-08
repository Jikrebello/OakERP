using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using Shouldly;

namespace OakERP.Tests.Integration.AccountsPayable;

[TestFixture]
public sealed class ApInvoiceApiTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task Create_Endpoint_Should_Create_Draft_Ap_Invoice_And_Default_Due_Date()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        string docNo = $"APINV-{Guid.NewGuid():N}";
        string invoiceNo = $"VEN-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId);

        var command = new CreateApInvoiceCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            InvoiceNo = invoiceNo,
            InvoiceDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-4)),
            CurrencyCode = "ZAR",
            TaxTotal = 15m,
            DocTotal = 115m,
            Memo = "April supplier invoice",
            Lines =
            [
                new ApInvoiceLineInputDto
                {
                    Description = "Rent",
                    AccountNo = "5000",
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
                new ApInvoiceLineInputDto
                {
                    Description = "Utilities",
                    AccountNo = "5000",
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
            ],
        };

        var result = await PostAsync<CreateApInvoiceCommand, ApInvoiceCommandResultDto>(
            ApiRoutes.ApInvoices.Create,
            command
        );

        result.Success.ShouldBeTrue();
        result.Invoice.ShouldNotBeNull();
        result.Invoice!.DocStatus.ShouldBe(DocStatus.Draft);
        result.Invoice.DueDate.ShouldBe(command.InvoiceDate.AddDays(30));
        result.Invoice.Lines.Select(x => x.LineNo).ShouldBe([1, 2]);

        await WithDbAsync(async db =>
        {
            var invoice = await db
                .ApInvoices.Include(x => x.Lines)
                .SingleAsync(x => x.DocNo == command.DocNo);

            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            invoice.DueDate.ShouldBe(command.InvoiceDate.AddDays(30));
            invoice.Lines.Count.ShouldBe(2);
            invoice.Lines.OrderBy(x => x.LineNo).Select(x => x.LineNo).ShouldBe([1, 2]);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Duplicate_Vendor_Invoice_Number_Without_Partial_Writes()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        string existingDocNo = $"APINV-{Guid.NewGuid():N}";
        string duplicateInvoiceNo = $"VEN-{Guid.NewGuid():N}";
        string docNo = $"APINV-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId);
        await SeedExistingInvoiceAsync(vendorId, existingDocNo, duplicateInvoiceNo);

        var command = new CreateApInvoiceCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            InvoiceNo = duplicateInvoiceNo,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            TaxTotal = 0m,
            DocTotal = 40m,
            Lines =
            [
                new ApInvoiceLineInputDto
                {
                    Description = "Services",
                    AccountNo = "5000",
                    Qty = 1m,
                    UnitPrice = 40m,
                    LineTotal = 40m,
                },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ApInvoices.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ApInvoiceCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("vendor");

        await WithDbAsync(async db =>
        {
            (await db.ApInvoices.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (
                await db
                    .ApInvoices.Where(x => x.DocNo == command.DocNo)
                    .SelectMany(x => x.Lines)
                    .CountAsync()
            ).ShouldBe(0);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Item_Based_Lines_Without_Partial_Writes()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        string docNo = $"APINV-{Guid.NewGuid():N}";
        string invoiceNo = $"VEN-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId);

        var command = new CreateApInvoiceCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            InvoiceNo = invoiceNo,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            TaxTotal = 0m,
            DocTotal = 25m,
            Lines =
            [
                new ApInvoiceLineInputDto
                {
                    AccountNo = "5000",
                    ItemId = Guid.NewGuid(),
                    Qty = 1m,
                    UnitPrice = 25m,
                    LineTotal = 25m,
                },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ApInvoices.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApInvoiceCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("Item-based");

        await WithDbAsync(async db =>
        {
            (await db.ApInvoices.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (
                await db
                    .ApInvoices.Where(x => x.DocNo == command.DocNo)
                    .SelectMany(x => x.Lines)
                    .CountAsync()
            ).ShouldBe(0);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Inactive_Account_Transactionally()
    {
        await AuthenticateAsync();

        var vendorId = Guid.NewGuid();
        string docNo = $"APINV-{Guid.NewGuid():N}";
        string invoiceNo = $"VEN-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(vendorId, accountActive: false);

        var command = new CreateApInvoiceCommand
        {
            DocNo = docNo,
            VendorId = vendorId,
            InvoiceNo = invoiceNo,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            TaxTotal = 0m,
            DocTotal = 40m,
            Lines =
            [
                new ApInvoiceLineInputDto
                {
                    Description = "Services",
                    AccountNo = "5000",
                    Qty = 1m,
                    UnitPrice = 40m,
                    LineTotal = 40m,
                },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ApInvoices.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApInvoiceCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("inactive");

        await WithDbAsync(async db =>
        {
            (await db.ApInvoices.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (
                await db
                    .ApInvoices.Where(x => x.DocNo == command.DocNo)
                    .SelectMany(x => x.Lines)
                    .CountAsync()
            ).ShouldBe(0);
        });
    }

    private async Task AuthenticateAsync()
    {
        string authId = Guid.NewGuid().ToString("N")[..8];

        var registerDto = new RegisterDto
        {
            Email = $"ap_invoice_api_{TestId}_{authId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "AP",
            LastName = "Tester",
            PhoneNumber = "123456789",
            TenantName = $"ApInvoiceTenant_{TestId}_{authId}",
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
        bool vendorActive = true,
        bool accountActive = true
    )
    {
        string vendorCode = $"VEND-{vendorId.ToString("N")[..8].ToUpperInvariant()}";

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

            var account = await db.GlAccounts.SingleOrDefaultAsync(x => x.AccountNo == "5000");
            if (account is null)
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "5000",
                        Name = "Expense",
                        Type = GlAccountType.Expense,
                        IsActive = accountActive,
                    }
                );
            }
            else
            {
                account.IsActive = accountActive;
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

            await db.SaveChangesAsync();
        });
    }

    private async Task SeedExistingInvoiceAsync(Guid vendorId, string docNo, string invoiceNo)
    {
        await WithDbAsync(async db =>
        {
            db.ApInvoices.Add(
                new ApInvoice
                {
                    Id = Guid.NewGuid(),
                    DocNo = docNo,
                    VendorId = vendorId,
                    InvoiceNo = invoiceNo,
                    InvoiceDate = DaysFromToday(-8),
                    DueDate = DaysFromToday(-8).AddDays(30),
                    CurrencyCode = "ZAR",
                    TaxTotal = 0m,
                    DocTotal = 10m,
                    DocStatus = DocStatus.Draft,
                }
            );

            await db.SaveChangesAsync();
        });
    }
}
