using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
public sealed class CategoriesController(RaceDayDbContext database) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategorySummary>>> Browse(int eventId, CancellationToken cancellationToken)
    {
        var eventExists = await database.Events.AnyAsync(item => item.Id == eventId && item.Status == "Published", cancellationToken);
        if (!eventExists) return NotFound();

        var categories = await database.Categories.AsNoTracking()
            .Where(category => category.EventId == eventId)
            .OrderBy(category => category.Name)
            .Select(category => new CategorySummary(category.Id, category.EventId, category.Name,
                category.MinimumAge, category.MaximumAge, category.Fee))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<ActionResult<CategorySummary>> Create(int eventId, CategoryWriteRequest request, CancellationToken cancellationToken)
    {
        var organiserId = SessionIdentity.UserId(HttpContext);
        if (organiserId is null) return Unauthorized();

        var raceEvent = await database.Events.SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
        if (raceEvent is null) return NotFound();
        if (raceEvent.OrganiserId != organiserId.Value)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only manage categories for your own events." });
        if (request.MinimumAge is not null && request.MaximumAge is not null && request.MinimumAge > request.MaximumAge)
            return BadRequest(new { message = "Minimum age cannot exceed maximum age." });

        var name = request.Name.Trim();
        if (await database.Categories.AnyAsync(item => item.EventId == eventId && item.Name == name, cancellationToken))
            return Conflict(new { message = "A category with that name already exists for this event." });

        var category = new RaceCategory
        {
            EventId = eventId,
            Name = name,
            MinimumAge = request.MinimumAge,
            MaximumAge = request.MaximumAge,
            Fee = request.Fee
        };
        database.Categories.Add(category);
        await database.SaveChangesAsync(cancellationToken);
        var response = new CategorySummary(category.Id, category.EventId, category.Name,
            category.MinimumAge, category.MaximumAge, category.Fee);
        return CreatedAtAction(nameof(Browse), new { eventId }, response);
    }
}
