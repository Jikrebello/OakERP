namespace OakERP.Common.DTOs.Auth;

/// <summary>
/// Represents the data required to register a new tenant account.
/// </summary>
/// <remarks>This class is used to encapsulate the information needed for a registration process, including tenant
/// details and user credentials. Ensure that all properties are properly populated before submitting the registration
/// request.</remarks>
public class RegisterDTO
{
    /// <summary>
    /// Gets or sets the name of the tenant associated with the current context.
    /// </summary>
    public string TenantName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the email address associated with the user.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the password associated with the user or system.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// Gets or sets the confirmation password entered by the user.
    /// </summary>
    public string ConfirmPassword { get; set; } = default!;
}