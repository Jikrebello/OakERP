using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.Application.AccountsPayable.Invoices.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ap-invoices")]
[Produces("application/json")]
public sealed class ApInvoicesController(IApInvoiceService apInvoiceService) : BaseApiController
{
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create an accounts payable invoice.",
        Description = "Creates a draft AP invoice and returns the saved invoice snapshot."
    )]
    [ProducesResponseType(typeof(ApInvoiceCommandResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApInvoiceCommandResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApInvoiceCommandResultDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApInvoiceCommandResultDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApInvoiceCommandResultDto),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken
    )
    {
        command.PerformedBy = ResolvePerformedBy();
        var result = await apInvoiceService.CreateAsync(command, cancellationToken);
        return ApiResult(result);
    }
}
