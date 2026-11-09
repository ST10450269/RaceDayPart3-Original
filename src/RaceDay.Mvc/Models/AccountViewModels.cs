using System.ComponentModel.DataAnnotations;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Models;

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterViewModel
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StrongPassword, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Participant";

    [Required, Phone]
    public string ContactNumber { get; set; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;
}
