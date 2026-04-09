using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.API.Contracts.Posting;
using OakERP.Application.AccountsReceivable.Receipts.Contracts;
using OakERP.Application.Posting.Contracts;
using OakERP.Common.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ar-receipts")]
[Produces("application/json")]
public sealed class ArReceiptsController(
    IArReceiptService arReceiptService,
    IPostingService postingService
) : BaseApiController
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

    [HttpPost("{receiptId:guid}/post")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Post an accounts receivable receipt.",
        Description = "Posts an existing AR receipt and returns the posting summary."
    )]
    [ProducesResponseType(typeof(PostResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Post(
        Guid receiptId,
        PostDocumentRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var result = await postingService.PostAsync(
            new PostCommand(
                DocKind.ArReceipt,
                receiptId,
                ResolvePerformedBy(),
                request.PostingDate,
                request.Force
            ),
            cancellationToken
        );

        return Ok(result);
    }
}
