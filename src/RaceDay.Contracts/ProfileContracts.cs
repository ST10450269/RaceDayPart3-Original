using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

public sealed record ProfileSummary(int Id, string FullName, string Email, string ContactNumber, string City, string Role);

public sealed record ProfileUpdateRequest(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, Phone] string ContactNumber,
    [Required, StringLength(80, MinimumLength = 2)] string City);
