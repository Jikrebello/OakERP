using System.ComponentModel.DataAnnotations;
using OakERP.Common.DTOs.Auth;

namespace OakERP.Shared.Models.Auth;

internal class RegisterFormModel
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(
        @"^\+?[1-9]\d{7,14}$",
        ErrorMessage = "Enter a valid international phone number."
    )]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Company name is required.")]
    public string TenantName { get; set; } = default!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = default!;

    public static implicit operator RegisterDTO(RegisterFormModel form)
    {
        return new RegisterDTO
        {
            FirstName = form.FirstName,
            LastName = form.LastName,
            PhoneNumber = form.PhoneNumber,
            TenantName = form.TenantName,
            Email = form.Email,
            Password = form.Password,
            ConfirmPassword = form.ConfirmPassword,
        };
    }
}