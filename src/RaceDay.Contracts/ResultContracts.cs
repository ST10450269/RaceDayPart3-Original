using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

public sealed record ResultWriteRequest(
    [Range(1, int.MaxValue)] int EnrolmentId,
    [Range(1, 864000)] int FinishTimeSeconds,
    [Range(1, 1000000)] int Position,
    [Range(1, 1000000)] int TotalFinishers);

public sealed record ResultSummary(
    int Id,
    int EnrolmentId,
    int EventId,
    string EventName,
    string CategoryName,
    string ParticipantName,
    int FinishTimeSeconds,
    int Position,
    int TotalFinishers,
    DateTime PublishedAtUtc);
