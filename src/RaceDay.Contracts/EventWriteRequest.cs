using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

public sealed record EventWriteRequest(
    [Required, StringLength(150, MinimumLength = 4)] string Name,
    [Required, StringLength(500, MinimumLength = 10)] string Description,
    [Required] DateTime EventDateUtc,
    [Required, StringLength(150, MinimumLength = 3)] string Location,
    [Range(0.1, 1000)] decimal DistanceKm,
    [Required] string EventType,
    [Range(1, 100000)] int MaxParticipants);
