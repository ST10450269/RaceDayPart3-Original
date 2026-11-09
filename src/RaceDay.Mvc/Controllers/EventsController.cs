using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public sealed class EventsController(ApiGateway api) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var reply = await api.GetAsync<List<EventSummary>>("api/events", cancellationToken);
        if (!reply.Success)
        {
            ViewData["ApiError"] = reply.Message;
            return View(new List<EventSummary>());
        }
        return View(reply.Value ?? new List<EventSummary>());
    }
}
