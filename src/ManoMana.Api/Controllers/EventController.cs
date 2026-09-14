using ManoMana.Application.Contracts;
using ManoMana.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManoMana.Api.Controllers;

[ApiController]
[Route("api/event")]
public sealed class EventController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EventResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await eventService.GetCurrentAsync(cancellationToken));
}
