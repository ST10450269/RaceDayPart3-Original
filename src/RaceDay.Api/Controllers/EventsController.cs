using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(RaceDayDbContext database) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventSummary>>> Browse(CancellationToken cancellationToken)
    {
        var events = await database.Events
            .AsNoTracking()
            .Where(item => item.Status == "Published")
            .OrderBy(item => item.EventDateUtc)
            .Select(item => new EventSummary(item.Id, item.Name, item.EventDateUtc, item.Location, item.DistanceKm, item.EventType))
            .ToListAsync(cancellationToken);

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventSummary>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await database.Events.AsNoTracking()
            .Where(raceEvent => raceEvent.Id == id && raceEvent.Status == "Published")
            .Select(raceEvent => new EventSummary(raceEvent.Id, raceEvent.Name, raceEvent.EventDateUtc,
                raceEvent.Location, raceEvent.DistanceKm, raceEvent.EventType))
            .SingleOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<ActionResult<EventSummary>> Create(EventWriteRequest request, CancellationToken cancellationToken)
    {
        var eventType = request.EventType.Trim();
        if (eventType is not ("Running" or "Walking" or "Cycling"))
            return BadRequest(new { message = "Event type must be Running, Walking, or Cycling." });
        if (request.EventDateUtc <= DateTime.UtcNow)
            return BadRequest(new { message = "An event must be scheduled in the future." });

        var organiserId = SessionIdentity.UserId(HttpContext);
        if (organiserId is null) return Unauthorized();

        var entity = new RaceEvent
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            EventDateUtc = request.EventDateUtc,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = eventType,
            MaxParticipants = request.MaxParticipants,
            Status = "Published",
            OrganiserId = organiserId.Value
        };
        database.Events.Add(entity);
        await database.SaveChangesAsync(cancellationToken);

        var response = new EventSummary(entity.Id, entity.Name, entity.EventDateUtc, entity.Location, entity.DistanceKm, entity.EventType);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }
}
