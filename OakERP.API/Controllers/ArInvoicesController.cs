using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.Application.AccountsReceivable.Invoices.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ar-invoices")]
[Produces("application/json")]
public sealed class ArInvoicesController(IArInvoiceService arInvoiceService) : BaseApiController
{
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create an accounts receivable invoice.",
        Description = "Creates a draft AR invoice and returns the saved invoice snapshot."
    )]
    [ProducesResponseType(typeof(ArInvoiceCommandResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ArInvoiceCommandResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ArInvoiceCommandResultDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ArInvoiceCommandResultDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ArInvoiceCommandResultDto),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateArInvoiceCommand command,
        CancellationToken cancellationToken
    )
    {
        command.PerformedBy = ResolvePerformedBy();
        var result = await arInvoiceService.CreateAsync(command, cancellationToken);
        return ApiResult(result);
    }
}
