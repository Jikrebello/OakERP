using Microsoft.OpenApi.Any;
using OakERP.API.Swagger.Configuration;
using OakERP.Application.AccountsPayable.Payments.Commands;

namespace OakERP.API.Swagger.Examples.AccountsPayable;

internal sealed class AllocateApPaymentCommandExampleFilter
    : OpenApiExampleSchemaFilter<AllocateApPaymentCommand>
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
                    ["apInvoiceId"] = new OpenApiString("22222222-3333-4444-5555-666666666666"),
                    ["amountApplied"] = new OpenApiDouble(2500),
                },
            },
        };
    }
}
