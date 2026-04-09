using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;

namespace OakERP.API.Swagger.Examples.AccountsPayable;

internal sealed class CreateApPaymentCommandExampleFilter
    : OpenApiExampleSchemaFilter<CreateApPaymentCommand>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["docNo"] = new OpenApiString("APPAY-2026-0004"),
            ["vendorId"] = new OpenApiString("9f6ee86f-32d7-4cb7-8d83-e8d1a25fd004"),
            ["bankAccountId"] = new OpenApiString("0a10a117-5471-4557-9b58-440a0b56ee51"),
            ["paymentDate"] = new OpenApiString("2026-04-08"),
            ["allocationDate"] = new OpenApiString("2026-04-08"),
            ["amount"] = new OpenApiDouble(8500),
            ["memo"] = new OpenApiString("Partial settlement for April invoices"),
            ["allocations"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["apInvoiceId"] = new OpenApiString("22222222-3333-4444-5555-666666666666"),
                    ["amountApplied"] = new OpenApiDouble(8500),
                },
            },
        };
    }
}
