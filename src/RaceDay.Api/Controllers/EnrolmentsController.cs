using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
public sealed class EnrolmentsController(RaceDayDbContext database) : ControllerBase
{
    [HttpPost]
    [SessionAuthorize("Participant")]
    public async Task<ActionResult<EnrolmentSummary>> Enrol(EnrolmentRequest request, CancellationToken cancellationToken)
    {
        var participantId = SessionIdentity.UserId(HttpContext);
        if (participantId is null) return Unauthorized();

        var raceEvent = await database.Events.SingleOrDefaultAsync(item =>
            item.Id == request.EventId && item.Status == "Published", cancellationToken);
        if (raceEvent is null) return NotFound(new { message = "Published event not found." });

        var category = await database.Categories.SingleOrDefaultAsync(item =>
            item.Id == request.CategoryId && item.EventId == request.EventId, cancellationToken);
        if (category is null) return BadRequest(new { message = "Choose a category belonging to this event." });

        if (await database.Enrolments.AnyAsync(item =>
            item.ParticipantId == participantId && item.EventId == request.EventId, cancellationToken))
            return Conflict(new { message = "You already have an entry for this event." });

        var currentEntries = await database.Enrolments.CountAsync(item =>
            item.EventId == request.EventId && item.Status == "Confirmed", cancellationToken);
        if (currentEntries >= raceEvent.MaxParticipants)
            return Conflict(new { message = "This event has reached its participant limit." });

        var enrolment = new RaceEnrolment
        {
            ParticipantId = participantId.Value,
            EventId = raceEvent.Id,
            CategoryId = category.Id,
            EnteredAtUtc = DateTime.UtcNow,
            Status = "Confirmed"
        };
        database.Enrolments.Add(enrolment);
        await database.SaveChangesAsync(cancellationToken);

        var response = new EnrolmentSummary(enrolment.Id, raceEvent.Id, raceEvent.Name,
            category.Id, category.Name, enrolment.Status, enrolment.EnteredAtUtc);
        return Created("/api/enrolments/mine", response);
    }

    [HttpGet("mine")]
    [SessionAuthorize("Participant")]
    public async Task<ActionResult<IReadOnlyList<EnrolmentSummary>>> Mine(CancellationToken cancellationToken)
    {
        var participantId = SessionIdentity.UserId(HttpContext);
        if (participantId is null) return Unauthorized();

        var entries = await (
            from enrolment in database.Enrolments.AsNoTracking()
            join raceEvent in database.Events on enrolment.EventId equals raceEvent.Id
            join category in database.Categories on enrolment.CategoryId equals category.Id
            where enrolment.ParticipantId == participantId.Value
            orderby raceEvent.EventDateUtc
            select new EnrolmentSummary(enrolment.Id, raceEvent.Id, raceEvent.Name,
                category.Id, category.Name, enrolment.Status, enrolment.EnteredAtUtc)
        ).ToListAsync(cancellationToken);

        return Ok(entries);
    }
}
