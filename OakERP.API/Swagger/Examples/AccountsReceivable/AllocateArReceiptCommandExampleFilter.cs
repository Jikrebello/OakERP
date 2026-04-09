using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;

namespace OakERP.API.Swagger.Examples.AccountsReceivable;

internal sealed class AllocateArReceiptCommandExampleFilter
    : OpenApiExampleSchemaFilter<AllocateArReceiptCommand>
{
    protected override IOpenApiAny BuildExample()
    {
        return new OpenApiObject
        {
            ["allocationDate"] = new OpenApiString("2026-04-08"),
            ["allocations"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["arInvoiceId"] = new OpenApiString("77777777-8888-9999-aaaa-bbbbbbbbbbbb"),
                    ["amountApplied"] = new OpenApiDouble(5000),
                },
            },
        };
    }
}
