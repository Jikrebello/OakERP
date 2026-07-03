using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OakERP.Client.Services.Errors;

namespace OakERP.Shared.Components;

public sealed class OakErrorBoundary : ErrorBoundary
{
    [Inject]
    public IClientErrorHandler ErrorHandler { get; set; } = default!;

    protected override async Task OnErrorAsync(Exception exception)
    {
        await ErrorHandler.HandleAsync(exception, new ClientErrorContext("Render"));
    }
}
