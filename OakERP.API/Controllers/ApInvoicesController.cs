using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OakERP.Application.AccountsPayable;

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

    private string ResolvePerformedBy() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? "api-user";
}
