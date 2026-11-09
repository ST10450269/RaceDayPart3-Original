using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
public sealed class ResultsController(RaceDayDbContext database) : ControllerBase
{
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<ActionResult<ResultSummary>> Publish(ResultWriteRequest request, CancellationToken cancellationToken)
    {
        if (request.Position > request.TotalFinishers)
            return BadRequest(new { message = "Finishing position cannot exceed the number of finishers." });

        var organiserId = SessionIdentity.UserId(HttpContext);
        if (organiserId is null) return Unauthorized();

        var enrolment = await database.Enrolments.SingleOrDefaultAsync(item => item.Id == request.EnrolmentId, cancellationToken);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });

        var raceEvent = await database.Events.SingleAsync(item => item.Id == enrolment.EventId, cancellationToken);
        if (raceEvent.OrganiserId != organiserId.Value)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only publish results for your own events." });
        if (raceEvent.EventDateUtc > DateTime.UtcNow && raceEvent.Status != "Completed")
            return Conflict(new { message = "Results can only be published after the event has finished." });
        if (await database.Results.AnyAsync(item => item.EnrolmentId == enrolment.Id, cancellationToken))
            return Conflict(new { message = "A result already exists for this enrolment." });

        var result = new RaceResult
        {
            EnrolmentId = enrolment.Id,
            CapturedByOrganiserId = organiserId.Value,
            FinishTimeSeconds = request.FinishTimeSeconds,
            Position = request.Position,
            TotalFinishers = request.TotalFinishers,
            PublishedAtUtc = DateTime.UtcNow
        };
        database.Results.Add(result);
        await database.SaveChangesAsync(cancellationToken);

        var response = await ProjectResults(resultId: result.Id).SingleAsync(cancellationToken);
        return Created($"/api/events/{raceEvent.Id}/results", response);
    }

    [HttpGet("mine")]
    [SessionAuthorize("Participant")]
    public async Task<ActionResult<IReadOnlyList<ResultSummary>>> Mine(CancellationToken cancellationToken)
    {
        var participantId = SessionIdentity.UserId(HttpContext);
        if (participantId is null) return Unauthorized();
        var results = await ProjectResults(participantId: participantId.Value)
            .ToListAsync(cancellationToken);
        return Ok(results);
    }

    [HttpGet("/api/events/{eventId:int}/results")]
    public async Task<ActionResult<IReadOnlyList<ResultSummary>>> ForEvent(int eventId, CancellationToken cancellationToken)
    {
        if (!await database.Events.AnyAsync(item => item.Id == eventId, cancellationToken)) return NotFound();
        var results = await ProjectResults(eventId: eventId).ToListAsync(cancellationToken);
        return Ok(results);
    }

    private IQueryable<ResultSummary> ProjectResults(int? participantId = null, int? resultId = null, int? eventId = null) =>
        from result in database.Results.AsNoTracking()
        join enrolment in database.Enrolments on result.EnrolmentId equals enrolment.Id
        join raceEvent in database.Events on enrolment.EventId equals raceEvent.Id
        join category in database.Categories on enrolment.CategoryId equals category.Id
        join participant in database.Users on enrolment.ParticipantId equals participant.Id
        where (participantId == null || enrolment.ParticipantId == participantId)
            && (resultId == null || result.Id == resultId)
            && (eventId == null || raceEvent.Id == eventId)
        orderby result.PublishedAtUtc descending
        select new ResultSummary(result.Id, enrolment.Id, raceEvent.Id, raceEvent.Name,
            category.Name, participant.FullName, result.FinishTimeSeconds, result.Position,
            result.TotalFinishers, result.PublishedAtUtc);
}
