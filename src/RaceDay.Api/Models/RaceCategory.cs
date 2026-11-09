namespace RaceDay.Api.Models;

public sealed class RaceCategory
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public required string Name { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public decimal Fee { get; set; }
}
