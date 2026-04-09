using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OakERP.API.Contracts.Posting;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Tests.Integration.Runtime;
using Shouldly;

namespace OakERP.Tests.Integration.AccountsReceivable;

[TestFixture]
public sealed class ArInvoiceApiTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task Post_Endpoint_Should_Post_Draft_Ar_Invoice()
    {
        await AuthenticateAsync();

        var invoiceId = await CreateDraftInvoiceAsync();

        var response = await PostInvoiceAsync(invoiceId);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PostResult>();
        result.ShouldNotBeNull();
        result!.DocKind.ShouldBe(DocKind.ArInvoice);
        result.SourceId.ShouldBe(invoiceId);
        result.GlEntryCount.ShouldBeGreaterThan(0);

        await WithDbAsync(async db =>
        {
            var invoice = await db.ArInvoices.SingleAsync(x => x.Id == invoiceId);
            invoice.DocStatus.ShouldBe(DocStatus.Posted);
        });
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_Conflict_ProblemDetails_When_Invoice_Is_Already_Posted()
    {
        await AuthenticateAsync();

        var invoiceId = await CreateDraftInvoiceAsync();
        (await PostInvoiceAsync(invoiceId)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var response = await PostInvoiceAsync(invoiceId);

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

        var invoiceId = await CreateDraftInvoiceAsync();

        var response = await PostInvoiceAsync(
            invoiceId,
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

        var invoiceId = await CreateDraftInvoiceAsync(currencyCode: "USD");

        var response = await PostInvoiceAsync(invoiceId);

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.BadRequest,
            "Posting invariant was violated."
        );
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_NotFound_ProblemDetails_For_Unknown_Invoice()
    {
        await AuthenticateAsync();

        var response = await PostInvoiceAsync(Guid.NewGuid());

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "AR invoice was not found."
        );
    }

    [Test]
    public async Task Post_Endpoint_Should_Return_BadRequest_ProblemDetails_When_Force_Is_Not_Supported()
    {
        await AuthenticateAsync();

        var invoiceId = await CreateDraftInvoiceAsync();

        var response = await PostInvoiceAsync(
            invoiceId,
            new PostDocumentRequestDto { Force = true }
        );

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.BadRequest,
            "The requested operation is not supported."
        );
    }

    [Test]
    public async Task Create_Endpoint_Should_Create_Draft_Ar_Invoice_With_Mixed_Lines_And_Default_Due_Date()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var outputTaxRateId = Guid.NewGuid();
        string docNo = $"ARINV-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        await SeedReferenceDataAsync(
            customerId,
            itemId,
            locationId,
            outputTaxRateId,
            Guid.NewGuid()
        );

        var command = new CreateArInvoiceCommand
        {
            DocNo = docNo,
            CustomerId = customerId,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            ShipTo = "Warehouse 4",
            Memo = "Mixed invoice",
            TaxTotal = 15m,
            DocTotal = 165m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "Consulting services",
                    RevenueAccount = "4000",
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
                new ArInvoiceLineInputDto
                {
                    Description = "Stock item",
                    ItemId = itemId,
                    LocationId = locationId,
                    TaxRateId = outputTaxRateId,
                    Qty = 1m,
                    UnitPrice = 100m,
                    LineTotal = 100m,
                },
            ],
        };

        var result = await PostAsync<CreateArInvoiceCommand, ArInvoiceCommandResultDto>(
            ApiRoutes.ArInvoices.Create,
            command
        );

        result.Success.ShouldBeTrue();
        result.Invoice.ShouldNotBeNull();
        result.Invoice!.DocStatus.ShouldBe(DocStatus.Draft);
        result.Invoice.DueDate.ShouldBe(command.InvoiceDate.AddDays(30));
        result.Invoice.Lines.Count.ShouldBe(2);
        result.Invoice.Lines[0].RevenueAccount.ShouldBe("4000");
        result.Invoice.Lines[1].ItemId.ShouldBe(itemId);
        result.Invoice.Lines[1].LocationId.ShouldBe(locationId);
        result.Invoice.Lines[1].TaxRateId.ShouldBe(outputTaxRateId);

        await WithDbAsync(async db =>
        {
            var invoice = await db
                .ArInvoices.Include(x => x.Lines)
                .SingleAsync(x => x.DocNo == command.DocNo);

            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            invoice.DueDate.ShouldBe(command.InvoiceDate.AddDays(30));
            invoice.ShipTo.ShouldBe("Warehouse 4");
            invoice.Lines.Count.ShouldBe(2);

            var lines = invoice.Lines.OrderBy(x => x.LineNo).ToList();
            lines[0].RevenueAccount.ShouldBe("4000");
            lines[1].ItemId.ShouldBe(itemId);
            lines[1].LocationId.ShouldBe(locationId);
            lines[1].TaxRateId.ShouldBe(outputTaxRateId);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Allow_Stock_Line_Without_Location_In_Draft_State()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var outputTaxRateId = Guid.NewGuid();
        string docNo = $"ARINV-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        await SeedReferenceDataAsync(
            customerId,
            itemId,
            Guid.NewGuid(),
            outputTaxRateId,
            Guid.NewGuid()
        );

        var command = new CreateArInvoiceCommand
        {
            DocNo = docNo,
            CustomerId = customerId,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            TaxTotal = 15m,
            DocTotal = 115m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "Stock line without location",
                    ItemId = itemId,
                    TaxRateId = outputTaxRateId,
                    Qty = 1m,
                    UnitPrice = 100m,
                    LineTotal = 100m,
                },
            ],
        };

        var result = await PostAsync<CreateArInvoiceCommand, ArInvoiceCommandResultDto>(
            ApiRoutes.ArInvoices.Create,
            command
        );

        result.Success.ShouldBeTrue();
        result.Invoice.ShouldNotBeNull();
        result.Invoice!.DocStatus.ShouldBe(DocStatus.Draft);
        result.Invoice.Lines.Single().LocationId.ShouldBeNull();

        await WithDbAsync(async db =>
        {
            var invoice = await db
                .ArInvoices.Include(x => x.Lines)
                .SingleAsync(x => x.DocNo == command.DocNo);

            invoice.DocStatus.ShouldBe(DocStatus.Draft);
            invoice.Lines.Single().LocationId.ShouldBeNull();
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Duplicate_DocNo_Without_Partial_Writes()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        string docNo = $"ARINV-DUP-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        await SeedReferenceDataAsync(
            customerId,
            itemId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        await SeedExistingInvoiceAsync(customerId, docNo);

        var command = new CreateArInvoiceCommand
        {
            DocNo = docNo,
            CustomerId = customerId,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            TaxTotal = 0m,
            DocTotal = 50m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "Service line",
                    RevenueAccount = "4000",
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ArInvoices.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ArInvoiceCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("already exists");

        await WithDbAsync(async db =>
        {
            (await db.ArInvoices.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(1);
            (
                await db
                    .ArInvoices.Where(x => x.DocNo == command.DocNo)
                    .SelectMany(x => x.Lines)
                    .CountAsync()
            ).ShouldBe(0);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Inactive_Item_Transactionally()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        string docNo = $"ARINV-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        await SeedReferenceDataAsync(
            customerId,
            itemId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            itemActive: false
        );

        var command = new CreateArInvoiceCommand
        {
            DocNo = docNo,
            CustomerId = customerId,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            TaxTotal = 0m,
            DocTotal = 100m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "Inactive item",
                    ItemId = itemId,
                    Qty = 1m,
                    UnitPrice = 100m,
                    LineTotal = 100m,
                },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ArInvoices.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ArInvoiceCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("inactive");

        await WithDbAsync(async db =>
        {
            (await db.ArInvoices.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (
                await db
                    .ArInvoices.Where(x => x.DocNo == command.DocNo)
                    .SelectMany(x => x.Lines)
                    .CountAsync()
            ).ShouldBe(0);
        });
    }

    [Test]
    public async Task Create_Endpoint_Should_Reject_Input_Tax_Rate_Without_Partial_Writes()
    {
        await AuthenticateAsync();

        var customerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var inputTaxRateId = Guid.NewGuid();
        string docNo = $"ARINV-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        await SeedReferenceDataAsync(
            customerId,
            itemId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            inputTaxRateId
        );

        var command = new CreateArInvoiceCommand
        {
            DocNo = docNo,
            CustomerId = customerId,
            InvoiceDate = DaysFromToday(-4),
            CurrencyCode = "ZAR",
            TaxTotal = 15m,
            DocTotal = 65m,
            Lines =
            [
                new ArInvoiceLineInputDto
                {
                    Description = "Service with invalid tax rate",
                    RevenueAccount = "4000",
                    TaxRateId = inputTaxRateId,
                    Qty = 1m,
                    UnitPrice = 50m,
                    LineTotal = 50m,
                },
            ],
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ArInvoices.Create, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ArInvoiceCommandResultDto>();
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("input tax");

        await WithDbAsync(async db =>
        {
            (await db.ArInvoices.CountAsync(x => x.DocNo == command.DocNo)).ShouldBe(0);
            (
                await db
                    .ArInvoices.Where(x => x.DocNo == command.DocNo)
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
            Email = $"ar_invoice_api_{TestId}_{authId}@oak.test",
            Password = "TestPass123!",
            ConfirmPassword = "TestPass123!",
            FirstName = "AR",
            LastName = "Tester",
            PhoneNumber = "123456789",
            TenantName = $"ArInvoiceTenant_{TestId}_{authId}",
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
        Guid itemId,
        Guid locationId,
        Guid outputTaxRateId,
        Guid inputTaxRateId,
        bool customerActive = true,
        bool itemActive = true,
        bool locationActive = true,
        bool includeUsdCurrency = false
    )
    {
        string customerCode = $"CUST-{customerId.ToString("N")[..8].ToUpperInvariant()}";
        string itemSku = $"SKU-{itemId.ToString("N")[..8].ToUpperInvariant()}";
        string locationCode = $"LOC-{locationId.ToString("N")[..8].ToUpperInvariant()}";
        string outputTaxName = $"VAT 15% {outputTaxRateId.ToString("N")[..8].ToUpperInvariant()}";
        string inputTaxName =
            $"Input VAT 15% {inputTaxRateId.ToString("N")[..8].ToUpperInvariant()}";

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

            if (includeUsdCurrency && !await db.Currencies.AnyAsync(x => x.Code == "USD"))
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

            if (!await db.GlAccounts.AnyAsync(x => x.AccountNo == "4000"))
            {
                db.GlAccounts.Add(
                    new GlAccount
                    {
                        AccountNo = "4000",
                        Name = "Revenue",
                        Type = GlAccountType.Revenue,
                        IsActive = true,
                    }
                );
            }

            db.Customers.Add(
                new Customer
                {
                    Id = customerId,
                    CustomerCode = customerCode,
                    Name = "Acme Customer",
                    TermsDays = 30,
                    IsActive = customerActive,
                }
            );

            db.Items.Add(
                new Item
                {
                    Id = itemId,
                    Sku = itemSku,
                    Name = "Stock Item",
                    Type = ItemType.Stock,
                    DefaultPrice = 100m,
                    IsActive = itemActive,
                }
            );

            db.Locations.Add(
                new Location
                {
                    Id = locationId,
                    Code = locationCode,
                    Name = "Main Warehouse",
                    IsActive = locationActive,
                }
            );

            db.TaxRates.AddRange(
                new TaxRate
                {
                    Id = outputTaxRateId,
                    Name = outputTaxName,
                    RatePercent = 15m,
                    IsInput = false,
                    IsActive = true,
                    EffectiveFrom = DaysFromToday(-100),
                },
                new TaxRate
                {
                    Id = inputTaxRateId,
                    Name = inputTaxName,
                    RatePercent = 15m,
                    IsInput = true,
                    IsActive = true,
                    EffectiveFrom = DaysFromToday(-100),
                }
            );

            await db.SaveChangesAsync();
        });
    }

    private async Task SeedExistingInvoiceAsync(Guid customerId, string docNo)
    {
        await WithDbAsync(async db =>
        {
            db.ArInvoices.Add(
                new ArInvoice
                {
                    Id = Guid.NewGuid(),
                    DocNo = docNo,
                    CustomerId = customerId,
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

    private async Task<Guid> CreateDraftInvoiceAsync(string currencyCode = "ZAR")
    {
        var customerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var outputTaxRateId = Guid.NewGuid();
        var inputTaxRateId = Guid.NewGuid();
        string docNo = $"ARINV-{Guid.NewGuid():N}";
        await SeedReferenceDataAsync(
            customerId,
            itemId,
            locationId,
            outputTaxRateId,
            inputTaxRateId,
            includeUsdCurrency: !string.Equals(
                currencyCode,
                "ZAR",
                StringComparison.OrdinalIgnoreCase
            )
        );

        var result = await PostAsync<CreateArInvoiceCommand, ArInvoiceCommandResultDto>(
            ApiRoutes.ArInvoices.Create,
            new CreateArInvoiceCommand
            {
                DocNo = docNo,
                CustomerId = customerId,
                InvoiceDate = DaysFromToday(-4),
                CurrencyCode = currencyCode,
                ShipTo = "Posting transport test ship-to",
                Memo = "Posting transport test invoice",
                TaxTotal = 15m,
                DocTotal = 115m,
                Lines =
                [
                    new ArInvoiceLineInputDto
                    {
                        Description = "Services",
                        RevenueAccount = "4000",
                        Qty = 1m,
                        UnitPrice = 100m,
                        LineTotal = 100m,
                    },
                ],
            }
        );

        result.Success.ShouldBeTrue();
        result.Invoice.ShouldNotBeNull();
        return result.Invoice!.InvoiceId;
    }

    private Task<HttpResponseMessage> PostInvoiceAsync(
        Guid invoiceId,
        PostDocumentRequestDto? request = null
    ) => Client.PostAsJsonAsync(ApiRoutes.ArInvoices.Post(invoiceId), request ?? new());

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
