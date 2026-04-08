using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;
using OakERP.Application.AccountsReceivable.Receipts.Commands;

namespace OakERP.API.Swagger.Examples.AccountsReceivable;

internal sealed class CreateArReceiptCommandExampleFilter
    : OpenApiExampleSchemaFilter<CreateArReceiptCommand>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["docNo"] = new OpenApiString("ARREC-2026-0007"),
            ["customerId"] = new OpenApiString("3cc71f33-27ad-47d9-9f3e-251e4553d16b"),
            ["bankAccountId"] = new OpenApiString("0a10a117-5471-4557-9b58-440a0b56ee51"),
            ["receiptDate"] = new OpenApiString("2026-04-08"),
            ["allocationDate"] = new OpenApiString("2026-04-08"),
            ["amount"] = new OpenApiDouble(14500),
            ["currencyCode"] = new OpenApiString("ZAR"),
            ["memo"] = new OpenApiString("Customer receipt against open invoices"),
            ["allocations"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["arInvoiceId"] = new OpenApiString("77777777-8888-9999-aaaa-bbbbbbbbbbbb"),
                    ["amountApplied"] = new OpenApiDouble(14500),
                },
            },
        };
    }
}
