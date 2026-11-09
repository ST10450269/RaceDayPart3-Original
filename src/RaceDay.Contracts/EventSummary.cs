namespace RaceDay.Contracts;

public sealed record EventSummary(
    int Id,
    string Name,
    DateTime EventDate,
    string Location,
    decimal DistanceKm,
    string EventType);
