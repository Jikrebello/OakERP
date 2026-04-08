using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ap-payments")]
public sealed class ApPaymentsController(IApPaymentService apPaymentService) : BaseApiController
{
    [HttpPost]
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
}
