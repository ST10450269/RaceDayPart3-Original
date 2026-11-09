namespace RaceDay.Api.Models;

public sealed class RaceRole
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<RaceDayUser> Users { get; set; } = new List<RaceDayUser>();
}

public sealed class RaceDayUser
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Phone { get; set; }
    public required string City { get; set; }
    public int RoleId { get; set; }
    public RaceRole Role { get; set; } = null!;
}
