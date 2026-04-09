using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.API.Contracts.Posting;
using OakERP.Application.AccountsPayable.Invoices.Contracts;
using OakERP.Application.Posting.Contracts;
using OakERP.Common.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ap-invoices")]
[Produces("application/json")]
public sealed class ApInvoicesController(
    IApInvoiceService apInvoiceService,
    IPostingService postingService
) : BaseApiController
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

    [HttpPost("{invoiceId:guid}/post")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Post an accounts payable invoice.",
        Description = "Posts an existing AP invoice and returns the posting summary."
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
                DocKind.ApInvoice,
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
