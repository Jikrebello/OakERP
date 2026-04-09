using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.API.Contracts.Posting;
using OakERP.Application.AccountsReceivable.Invoices.Contracts;
using OakERP.Application.Posting.Contracts;
using OakERP.Common.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ar-invoices")]
[Produces("application/json")]
public sealed class ArInvoicesController(
    IArInvoiceService arInvoiceService,
    IPostingService postingService
) : BaseApiController
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

    [HttpPost("{invoiceId:guid}/post")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Post an accounts receivable invoice.",
        Description = "Posts an existing AR invoice and returns the posting summary."
    )]
    [ProducesResponseType(typeof(PostResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Post(
        Guid invoiceId,
        PostDocumentRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var result = await postingService.PostAsync(
            new PostCommand(
                DocKind.ArInvoice,
                invoiceId,
                ResolvePerformedBy(),
                request.PostingDate,
                request.Force
            ),
            cancellationToken
        );

        return Ok(result);
    }
}
