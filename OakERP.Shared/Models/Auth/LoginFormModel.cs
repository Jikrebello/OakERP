using System.ComponentModel.DataAnnotations;
using OakERP.Common.DTOs.Auth;

namespace OakERP.Shared.Models.Auth;

/// <summary>
/// Represents the data model for a login form, containing user credentials such as email and password.
/// </summary>
/// <remarks>This model is typically used to capture user input for authentication purposes.  Validation
/// attributes are applied to ensure that the email and password fields meet the required criteria: <list type="bullet">
/// <item><description>The <see cref="Email"/> property must be a valid email address and cannot be
/// empty.</description></item> <item><description>The <see cref="Password"/> property cannot be
/// empty.</description></item> </list></remarks>
public class LoginFormModel
{
    /// <summary>
    /// Gets or sets the email address associated with the user.
    /// </summary>
    /// <remarks>The email address must be in a valid format as defined by standard email address conventions.
    /// An error will be raised if the value is invalid or not provided.</remarks>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password associated with the user.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public static implicit operator LoginDTO(LoginFormModel form)
    {
        return new LoginDTO { Email = form.Email, Password = form.Password };
    }
}