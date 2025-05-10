namespace OakERP.Shared.DTOs.Auth;

public class RegisterDTO
{
    public string TenantName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}