using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/profile")]
[SessionAuthorize]
public sealed class ProfileController(RaceDayDbContext database) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileSummary>> Get(CancellationToken cancellationToken)
    {
        var userId = SessionIdentity.UserId(HttpContext);
        if (userId is null) return Unauthorized();
        var user = await database.Users.Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return Unauthorized();
        return Ok(new ProfileSummary(user.Id, user.FullName, user.Email, user.Phone, user.City, user.Role.Name));
    }

    [HttpPut]
    public async Task<IActionResult> Update(ProfileUpdateRequest request, CancellationToken cancellationToken)
    {
        var userId = SessionIdentity.UserId(HttpContext);
        if (userId is null) return Unauthorized();
        var user = await database.Users.SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return Unauthorized();
        user.FullName = request.FullName.Trim();
        user.Phone = request.ContactNumber.Trim();
        user.City = request.City.Trim();
        await database.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
