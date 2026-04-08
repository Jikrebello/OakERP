using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.Application.AccountsReceivable.Receipts.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ar-receipts")]
[Produces("application/json")]
public sealed class ArReceiptsController(IArReceiptService arReceiptService) : BaseApiController
{
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create an accounts receivable receipt.",
        Description = "Creates a draft AR receipt and optionally applies the supplied invoice allocations."
    )]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ArReceiptCommandResultDto),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken
    )
    {
        command.PerformedBy = ResolvePerformedBy();
        var result = await arReceiptService.CreateAsync(command, cancellationToken);
        return ApiResult(result);
    }

    [HttpPost("{receiptId:guid}/allocations")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Allocate an AR receipt to invoices.",
        Description = "Applies allocation amounts from an existing AR receipt to one or more posted AR invoices."
    )]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ArReceiptCommandResultDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ArReceiptCommandResultDto),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Allocate(
        Guid receiptId,
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken
    )
    {
        command.ReceiptId = receiptId;
        command.PerformedBy = ResolvePerformedBy();
        var result = await arReceiptService.AllocateAsync(command, cancellationToken);
        return ApiResult(result);
    }
}
