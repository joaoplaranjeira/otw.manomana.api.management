using ManoMana.Application.Contracts;
using ManoMana.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManoMana.Api.Controllers;

[ApiController]
[Route("api/ranking")]
public sealed class RankingController(IRankingService rankingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<RankingEntryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyCollection<RankingEntryResponse>>> Get(CancellationToken cancellationToken) =>
        Ok(await rankingService.GetAsync(cancellationToken));
}
