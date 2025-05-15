using Microsoft.AspNetCore.Components;
using OakERP.Shared.Services.Api;

namespace OakERP.Shared.ViewModels.Base;

public abstract class BaseViewModel(IApiClient api, NavigationManager navigation)
{
    protected readonly NavigationManager Navigation = navigation;
    protected readonly IApiClient Api = api;

    /// <summary>
    /// Indicates whether the UI should show a loading spinner or disable inputs.
    /// </summary>
    public bool IsBusy { get; set; }

    /// <summary>
    /// A user-friendly error message to display in the UI.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Optional: a generic success message (e.g. "Registration successful").
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Optional: use to trigger validation manually in certain cases.
    /// </summary>
    public bool TriggerValidation { get; set; }
}
