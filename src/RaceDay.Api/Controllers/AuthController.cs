using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(RaceDayDbContext database, IPasswordHasher<RaceDayUser> passwordHasher) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var roleName = request.Role.Trim();
        if (roleName is not ("Organiser" or "Participant"))
            return BadRequest(new { message = "Role must be Organiser or Participant." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await database.Users.AnyAsync(user => user.Email == email, cancellationToken))
            return Conflict(new { message = "That email address is already registered." });

        var role = await database.Roles.SingleOrDefaultAsync(item => item.Name == roleName, cancellationToken);
        if (role is null)
            return Problem("RaceDay roles have not been initialized.", statusCode: StatusCodes.Status503ServiceUnavailable);

        var user = new RaceDayUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = string.Empty,
            Phone = request.ContactNumber.Trim(),
            City = request.City.Trim(),
            RoleId = role.Id,
            Role = role
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        database.Users.Add(user);
        await database.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(user.Id, user.FullName, user.Email, role.Name);
        return CreatedAtAction(nameof(SessionInfo), response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await database.Users.Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Incorrect email or password." });

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("RaceDayUserId", user.Id);
        HttpContext.Session.SetString("RaceDayRole", user.Role.Name);
        return Ok(new AuthResponse(user.Id, user.FullName, user.Email, user.Role.Name));
    }

    [HttpGet("session")]
    [SessionAuthorize]
    public async Task<ActionResult<AuthResponse>> SessionInfo(CancellationToken cancellationToken)
    {
        var userId = SessionIdentity.UserId(HttpContext);
        var user = await database.Users.Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        return user is null
            ? Unauthorized()
            : Ok(new AuthResponse(user.Id, user.FullName, user.Email, user.Role.Name));
    }

    [HttpPost("logout")]
    [SessionAuthorize]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return NoContent();
    }
}
