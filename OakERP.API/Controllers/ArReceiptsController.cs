using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ar-receipts")]
public sealed class ArReceiptsController(IArReceiptService arReceiptService) : BaseApiController
{
    [HttpPost]
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
