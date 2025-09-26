namespace OakERP.Common.DTOs.Auth;

/// <summary>
/// Represents the data required to register a new tenant account.
/// </summary>
/// <remarks>This class is used to encapsulate the information needed for a registration process, including tenant
/// details and user credentials. Ensure that all properties are properly populated before submitting the registration
/// request.</remarks>
public class RegisterDTO
{
    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public string PhoneNumber { get; set; } = string.Empty;

    public string TenantName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string ConfirmPassword { get; set; } = default!;
}
