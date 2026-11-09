using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public static class RaceDaySeedData
{
    public static async Task InitializeAsync(RaceDayDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (!await db.Roles.AnyAsync(cancellationToken))
        {
            db.Roles.AddRange(new RaceRole { Name = "Organiser" }, new RaceRole { Name = "Participant" });
            await db.SaveChangesAsync(cancellationToken);
        }

        var organiserRole = await db.Roles.SingleAsync(role => role.Name == "Organiser", cancellationToken);
        var participantRole = await db.Roles.SingleAsync(role => role.Name == "Participant", cancellationToken);
        var hasher = new PasswordHasher<RaceDayUser>();

        if (!await db.Users.AnyAsync(cancellationToken))
        {
            var organiser = NewUser("Demo Organiser", "organiser@raceday.test", "Durban", "DemoOrganiser!2026", organiserRole, hasher);
            var participant = NewUser("Demo Participant", "runner@raceday.test", "Durban", "DemoRunner!2026", participantRole, hasher);
            db.Users.AddRange(organiser, participant);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Events.AnyAsync(cancellationToken))
        {
            var organiserId = await db.Users.Where(user => user.RoleId == organiserRole.Id)
                .Select(user => user.Id).FirstAsync(cancellationToken);
            db.Events.AddRange(
                new RaceEvent
                {
                    Name = "Durban Community Run",
                    Description = "A friendly road-running event for local athletes.",
                    EventDateUtc = DateTime.UtcNow.Date.AddDays(30),
                    Location = "Durban, KwaZulu-Natal",
                    DistanceKm = 10m,
                    EventType = "Running",
                    MaxParticipants = 200,
                    Status = "Published",
                    OrganiserId = organiserId
                },
                new RaceEvent
                {
                    Name = "Hillcrest Autumn Race",
                    Description = "A completed local race used for result demonstrations.",
                    EventDateUtc = DateTime.UtcNow.Date.AddDays(-30),
                    Location = "Hillcrest, KwaZulu-Natal",
                    DistanceKm = 5m,
                    EventType = "Running",
                    MaxParticipants = 100,
                    Status = "Completed",
                    OrganiserId = organiserId
                });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            var events = await db.Events.OrderBy(item => item.Id).ToListAsync(cancellationToken);
            db.Categories.AddRange(
                new RaceCategory { EventId = events[0].Id, Name = "Open 10K", MinimumAge = 16, MaximumAge = 99, Fee = 0m },
                new RaceCategory { EventId = events[1].Id, Name = "Open 5K", MinimumAge = 14, MaximumAge = 99, Fee = 0m });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Enrolments.AnyAsync(cancellationToken))
        {
            var participantId = await db.Users.Where(user => user.RoleId == participantRole.Id)
                .Select(user => user.Id).FirstAsync(cancellationToken);
            var pastEvent = await db.Events.SingleAsync(item => item.Name == "Hillcrest Autumn Race", cancellationToken);
            var category = await db.Categories.SingleAsync(item => item.EventId == pastEvent.Id, cancellationToken);
            db.Enrolments.Add(new RaceEnrolment
            {
                ParticipantId = participantId,
                EventId = pastEvent.Id,
                CategoryId = category.Id,
                EnteredAtUtc = DateTime.UtcNow.Date.AddDays(-45),
                Status = "Confirmed"
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static RaceDayUser NewUser(string name, string email, string city, string password, RaceRole role, IPasswordHasher<RaceDayUser> hasher)
    {
        var user = new RaceDayUser
        {
            FullName = name,
            Email = email,
            PasswordHash = string.Empty,
            Phone = "0821234567",
            City = city,
            RoleId = role.Id,
            Role = role
        };
        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }
}
