using ManoMana.Application.Contracts;
using ManoMana.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManoMana.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin")]
public sealed class AdminController(IAuthenticationService authentication, IAdminService adminService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken) =>
        Ok(await authentication.LoginAsync(request, cancellationToken));

    [HttpGet("predictions")]
    public async Task<ActionResult<IReadOnlyCollection<AdminPredictionResponse>>> Predictions(CancellationToken cancellationToken) =>
        Ok(await adminService.GetPredictionsAsync(cancellationToken));

    [HttpGet("predictions/export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken) =>
        File(await adminService.ExportPredictionsCsvAsync(cancellationToken), "text/csv; charset=utf-8", "predictions.csv");

    [HttpDelete("predictions/{id:guid}")]
    public async Task<IActionResult> DeletePrediction(Guid id, CancellationToken cancellationToken)
    {
        await adminService.DeletePredictionAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("event/open")]
    public async Task<IActionResult> OpenEvent(CancellationToken cancellationToken)
    {
        await adminService.OpenEventAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("event/close")]
    public async Task<IActionResult> CloseEvent(CancellationToken cancellationToken)
    {
        await adminService.CloseEventAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("birth")]
    public async Task<IActionResult> CreateBirth(UpsertBirthRequest request, CancellationToken cancellationToken)
    {
        await adminService.CreateBirthAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("birth")]
    public async Task<IActionResult> UpdateBirth(UpsertBirthRequest request, CancellationToken cancellationToken)
    {
        await adminService.UpdateBirthAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("birth/publish")]
    public async Task<IActionResult> PublishBirth(CancellationToken cancellationToken)
    {
        await adminService.PublishBirthAsync(cancellationToken);
        return NoContent();
    }
}
