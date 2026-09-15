using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ManoMana.Application.Contracts;
using ManoMana.Domain.Enums;
using ManoMana.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ManoMana.IntegrationTests;

public sealed class AdminResetFlowTests
{
    [Fact]
    public async Task Reset_removes_the_current_cycle_and_starts_a_new_open_event()
    {
        await using var factory = new ManoManaApiFactory();
        using var client = factory.CreateClient();

        var originalEvent = await client.GetFromJsonAsync<EventResponse>("/api/event");
        Assert.NotNull(originalEvent);

        var created = await client.PostAsJsonAsync("/api/predictions", new CreatePredictionRequest(
            "Joana", Gender.Girl, new DateOnly(2026, 10, 4), new TimeOnly(7, 40), 3275, 50, "Maria"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var prediction = await created.Content.ReadFromJsonAsync<CreatePredictionResponse>();
        Assert.NotNull(prediction);

        var login = await client.PostAsJsonAsync("/api/admin/login", new LoginRequest("admin", "A-strong-test-password!"));
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/admin/event/close", null)).StatusCode);
        var birth = new UpsertBirthRequest(
            Gender.Girl, new DateOnly(2026, 10, 4), new TimeOnly(7, 42), 3250, 50, "Maria", null);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/admin/birth", birth)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/admin/birth/publish", null)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/admin/event/reset", null)).StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var resetEvent = await client.GetFromJsonAsync<EventResponse>("/api/event");
        Assert.NotNull(resetEvent);
        Assert.NotEqual(originalEvent.Id, resetEvent.Id);
        Assert.Equal(originalEvent.Name, resetEvent.Name);
        Assert.Equal(EventStatus.Open, resetEvent.Status);
        Assert.Null(resetEvent.PredictionsCloseAt);
        Assert.Null(resetEvent.Birth);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/predictions/{prediction.Id}")).StatusCode);
        var stats = await client.GetFromJsonAsync<PredictionStatsResponse>("/api/predictions/stats");
        Assert.NotNull(stats);
        Assert.Equal(0, stats.Total);

        var newPrediction = await client.PostAsJsonAsync("/api/predictions", new CreatePredictionRequest(
            "Miguel", Gender.Boy, new DateOnly(2027, 6, 12), new TimeOnly(12, 30), 3400, 51, "Tomás"));
        Assert.Equal(HttpStatusCode.Created, newPrediction.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ManoManaDbContext>();
        Assert.Equal(1, await database.Events.CountAsync());
        Assert.Equal(1, await database.Predictions.CountAsync());
        Assert.Equal(0, await database.Births.CountAsync());
    }
}
