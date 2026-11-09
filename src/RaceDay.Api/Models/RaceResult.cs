namespace RaceDay.Api.Models;

public sealed class RaceResult
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public int CapturedByOrganiserId { get; set; }
    public int FinishTimeSeconds { get; set; }
    public int Position { get; set; }
    public int TotalFinishers { get; set; }
    public DateTime PublishedAtUtc { get; set; }
}
