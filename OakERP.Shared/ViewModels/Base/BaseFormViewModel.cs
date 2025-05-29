using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OakERP.Shared.Services.Api;

namespace OakERP.Shared.ViewModels.Base;

/// <summary>
/// Serves as a base class for form-based view models, providing a strongly-typed model for form data and an associated
/// <see cref="EditContext"/> for managing validation and state.
/// </summary>
/// <remarks>This class is designed to simplify the creation of form-based view models by providing a
/// pre-initialized <see cref="EditContext"/> and a default instance of the form model. Derived classes can use the <see
/// cref="Form"/> property to bind form data to UI components and the <see cref="EditContext"/> property to manage
/// validation logic.</remarks>
/// <typeparam name="TModel">The type of the form model. Must be a reference type with a parameterless constructor.</typeparam>
public abstract class BaseFormViewModel<TModel> : BaseViewModel
    where TModel : class, new()
{
    /// <summary>
    /// The model that represents the form data. This is typically bound to the UI components.
    /// </summary>
    public TModel Form { get; }

    /// <summary>
    /// Gets the <see cref="EditContext"/> associated with the current form.
    /// </summary>
    /// <remarks>The <see cref="EditContext"/> provides access to the form's validation state,  field-level
    /// validation messages, and mechanisms for notifying the form of changes  to its fields. It is typically used to
    /// interact with form validation logic.</remarks>
    public EditContext EditContext { get; }

    protected BaseFormViewModel(IApiClient api, NavigationManager? navigation = null)
        : base(api, navigation)
    {
        Form = new TModel();
        EditContext = new EditContext(Form);
    }
}