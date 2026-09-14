using ManoMana.Application.Contracts;
using ManoMana.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ManoMana.Api.Controllers;

[ApiController]
[Route("api/predictions")]
public sealed class PredictionsController(IPredictionService predictionService) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("prediction-create")]
    [ProducesResponseType<CreatePredictionResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatePredictionResponse>> Create(
        CreatePredictionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await predictionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PredictionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PredictionResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await predictionService.GetAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, UpdatePredictionRequest request, CancellationToken cancellationToken)
    {
        var authorization = Request.Headers.Authorization.ToString();
        var token = authorization.StartsWith("Prediction ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Prediction ".Length..].Trim()
            : string.Empty;
        await predictionService.UpdateAsync(id, token, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("stats")]
    [ProducesResponseType<PredictionStatsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PredictionStatsResponse>> Stats(CancellationToken cancellationToken) =>
        Ok(await predictionService.GetStatsAsync(cancellationToken));
}
