namespace RaceDay.Api.Models;

public sealed class RaceEvent
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime EventDateUtc { get; set; }
    public required string Location { get; set; }
    public decimal DistanceKm { get; set; }
    public required string EventType { get; set; }
    public int MaxParticipants { get; set; }
    public string Status { get; set; } = "Published";
    public int OrganiserId { get; set; }
}
