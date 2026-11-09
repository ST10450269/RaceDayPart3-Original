using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public sealed class AccountController(ApiGateway api) : Controller
{
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var (reply, cookie) = await api.LoginAsync<AuthResponse>(
            new LoginRequest(model.Email, model.Password), cancellationToken);
        if (!reply.Success || reply.Value is null || string.IsNullOrWhiteSpace(cookie))
        {
            ModelState.AddModelError(string.Empty, reply.Success ? "The API did not return a session cookie." : reply.Message);
            return View(model);
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SetString("RaceDay.Api.Cookie", cookie);
        HttpContext.Session.SetString("RaceDay.UserName", reply.Value.FullName);
        HttpContext.Session.SetString("RaceDay.UserRole", reply.Value.Role);
        return RedirectToAction("Index", "Events");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (model.Role is not ("Organiser" or "Participant"))
            ModelState.AddModelError(nameof(model.Role), "Choose Organiser or Participant.");
        if (!ModelState.IsValid) return View(model);

        var reply = await api.SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/register",
            new RegisterRequest(model.FullName, model.Email, model.Password, model.Role, model.ContactNumber, model.City),
            cancellationToken);
        if (!reply.Success)
        {
            ModelState.AddModelError(string.Empty, reply.Message);
            return View(model);
        }

        TempData["Notice"] = "Account created. Sign in to continue.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await api.SendAsync<object>(HttpMethod.Post, "api/auth/logout", null, cancellationToken);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
