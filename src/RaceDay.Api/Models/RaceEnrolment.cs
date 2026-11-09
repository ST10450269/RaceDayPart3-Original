namespace RaceDay.Api.Models;

public sealed class RaceEnrolment
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnteredAtUtc { get; set; }
    public string Status { get; set; } = "Confirmed";
}
