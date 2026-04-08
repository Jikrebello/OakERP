using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OakERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/ap-invoices")]
public sealed class ApInvoicesController(IApInvoiceService apInvoiceService) : BaseApiController
{
    [HttpPost]
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
