using System.ComponentModel.DataAnnotations;
using OakERP.Common.Dtos.Auth;

namespace OakERP.UI.Models.Auth;

public class LoginFormModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public static implicit operator LoginDto(LoginFormModel form)
    {
        return new LoginDto { Email = form.Email, Password = form.Password };
    }
}
