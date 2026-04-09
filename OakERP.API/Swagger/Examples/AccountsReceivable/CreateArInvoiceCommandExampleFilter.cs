using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;

namespace OakERP.API.Swagger.Examples.AccountsReceivable;

internal sealed class CreateArInvoiceCommandExampleFilter
    : OpenApiExampleSchemaFilter<CreateArInvoiceCommand>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["docNo"] = new OpenApiString("ARINV-2026-0012"),
            ["customerId"] = new OpenApiString("3cc71f33-27ad-47d9-9f3e-251e4553d16b"),
            ["invoiceDate"] = new OpenApiString("2026-04-08"),
            ["dueDate"] = new OpenApiString("2026-05-08"),
            ["currencyCode"] = new OpenApiString("ZAR"),
            ["shipTo"] = new OpenApiString("Warehouse 4, Johannesburg"),
            ["memo"] = new OpenApiString("Mixed draft invoice with service and stock lines"),
            ["taxTotal"] = new OpenApiDouble(2250),
            ["docTotal"] = new OpenApiDouble(17250),
            ["lines"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["description"] = new OpenApiString("Implementation services"),
                    ["revenueAccount"] = new OpenApiString("4000"),
                    ["qty"] = new OpenApiDouble(1),
                    ["unitPrice"] = new OpenApiDouble(5000),
                    ["lineTotal"] = new OpenApiDouble(5000),
                },
                new OpenApiObject
                {
                    ["description"] = new OpenApiString("Stock item shipment"),
                    ["itemId"] = new OpenApiString("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                    ["qty"] = new OpenApiDouble(5),
                    ["unitPrice"] = new OpenApiDouble(2000),
                    ["taxRateId"] = new OpenApiString("11111111-2222-3333-4444-555555555555"),
                    ["locationId"] = new OpenApiString("99999999-8888-7777-6666-555555555555"),
                    ["lineTotal"] = new OpenApiDouble(10000),
                },
            },
        };
    }
}
