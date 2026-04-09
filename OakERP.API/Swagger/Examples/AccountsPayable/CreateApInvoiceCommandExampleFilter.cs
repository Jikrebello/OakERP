using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;

namespace OakERP.API.Swagger.Examples.AccountsPayable;

internal sealed class CreateApInvoiceCommandExampleFilter
    : OpenApiExampleSchemaFilter<CreateApInvoiceCommand>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["docNo"] = new OpenApiString("APINV-2026-0001"),
            ["vendorId"] = new OpenApiString("9f6ee86f-32d7-4cb7-8d83-e8d1a25fd004"),
            ["invoiceNo"] = new OpenApiString("SUP-44821"),
            ["invoiceDate"] = new OpenApiString("2026-04-08"),
            ["dueDate"] = new OpenApiString("2026-05-08"),
            ["currencyCode"] = new OpenApiString("ZAR"),
            ["memo"] = new OpenApiString("April component shipment"),
            ["taxTotal"] = new OpenApiDouble(2250),
            ["docTotal"] = new OpenApiDouble(17250),
            ["lines"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["description"] = new OpenApiString("Industrial bearings"),
                    ["accountNo"] = new OpenApiString("5100"),
                    ["itemId"] = new OpenApiString("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                    ["qty"] = new OpenApiDouble(10),
                    ["unitPrice"] = new OpenApiDouble(1500),
                    ["taxRateId"] = new OpenApiString("11111111-2222-3333-4444-555555555555"),
                    ["lineTotal"] = new OpenApiDouble(15000),
                },
            },
        };
    }
}
