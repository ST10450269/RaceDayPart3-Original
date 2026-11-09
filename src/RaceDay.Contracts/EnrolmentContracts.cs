using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

public sealed record EnrolmentRequest([Range(1, int.MaxValue)] int EventId, [Range(1, int.MaxValue)] int CategoryId);

public sealed record EnrolmentSummary(
    int Id,
    int EventId,
    string EventName,
    int CategoryId,
    string CategoryName,
    string Status,
    DateTime EnteredAtUtc);

public sealed record CategorySummary(int Id, int EventId, string Name, int? MinimumAge, int? MaximumAge, decimal Fee);
