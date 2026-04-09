using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.API.Contracts.Posting;
using OakERP.Application.AccountsPayable.Payments.Contracts;
using OakERP.Application.Posting.Contracts;
using OakERP.Common.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ap-payments")]
[Produces("application/json")]
public sealed class ApPaymentsController(
    IApPaymentService apPaymentService,
    IPostingService postingService
) : BaseApiController
{
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create an accounts payable payment.",
        Description = "Creates a draft AP payment and optionally applies the supplied invoice allocations."
    )]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApPaymentCommandResultDto),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        command.PerformedBy = ResolvePerformedBy();
        var result = await apPaymentService.CreateAsync(command, cancellationToken);
        return ApiResult(result);
    }

    [HttpPost("{paymentId:guid}/allocations")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Allocate an AP payment to invoices.",
        Description = "Applies allocation amounts from an existing AP payment to one or more posted AP invoices."
    )]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApPaymentCommandResultDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApPaymentCommandResultDto),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Allocate(
        Guid paymentId,
        AllocateApPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        command.PaymentId = paymentId;
        command.PerformedBy = ResolvePerformedBy();
        var result = await apPaymentService.AllocateAsync(command, cancellationToken);
        return ApiResult(result);
    }

    [HttpPost("{paymentId:guid}/post")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Post an accounts payable payment.",
        Description = "Posts an existing AP payment and returns the posting summary."
    )]
    [ProducesResponseType(typeof(PostResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Post(
        Guid paymentId,
        PostDocumentRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var result = await postingService.PostAsync(
            new PostCommand(
                DocKind.ApPayment,
                paymentId,
                ResolvePerformedBy(),
                request.PostingDate,
                request.Force
            ),
            cancellationToken
        );

        return Ok(result);
    }
}
