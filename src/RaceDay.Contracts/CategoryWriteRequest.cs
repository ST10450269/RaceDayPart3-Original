using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

public sealed record CategoryWriteRequest(
    [Required, StringLength(50, MinimumLength = 2)] string Name,
    [Range(0, 120)] int? MinimumAge,
    [Range(0, 120)] int? MaximumAge,
    [Range(typeof(decimal), "0", "100000")] decimal Fee);
