using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

public sealed record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, StrongPassword] string Password,
    [Required] string Role,
    [Required, Phone] string ContactNumber,
    [Required, StringLength(80, MinimumLength = 2)] string City);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record AuthResponse(int UserId, string FullName, string Email, string Role);
