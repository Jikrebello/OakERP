using Microsoft.AspNetCore.Components;
using OakERP.Shared.Services.Api;

namespace OakERP.Shared.ViewModels.Base;

/// <summary>
/// Serves as a base class for view models, providing common properties and functionality  for managing UI state, such
/// as loading indicators, error messages, and success messages.
/// </summary>
/// <remarks>This class is designed to be inherited by specific view models in applications that  require
/// interaction with an API client and navigation management. It provides properties  to handle common UI scenarios,
/// such as displaying loading states, error messages, and  success messages, as well as triggering
/// validation.</remarks>
/// <param name="api"></param>
/// <param name="navigation"></param>
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