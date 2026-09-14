using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ManoMana.Application.Contracts;
using ManoMana.Domain.Enums;
using Xunit;

namespace ManoMana.IntegrationTests;

public sealed class BirthPrivacyFlowTests
{
    [Fact]
    public async Task Birth_stays_private_until_explicit_publication()
    {
        await using var factory = new ManoManaApiFactory();
        using var client = factory.CreateClient();

        var swagger = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("ManoMana API", swagger, StringComparison.Ordinal);
        Assert.Contains("Bearer", swagger, StringComparison.Ordinal);

        var created = await CreatePrediction(client);
        var stats = await client.GetFromJsonAsync<PredictionStatsResponse>("/api/predictions/stats");
        Assert.NotNull(stats);
        Assert.Equal(1, stats.Total);
        Assert.Equal(50, stats.AveragePredictedHeightCentimeters);

        var token = await Login(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/admin/event/close", null)).StatusCode);

        var closedCreate = await client.PostAsJsonAsync("/api/predictions", PredictionRequest("Late entry"));
        Assert.Equal(HttpStatusCode.Conflict, closedCreate.StatusCode);
        using (var update = new HttpRequestMessage(HttpMethod.Put, $"/api/predictions/{created.Id}"))
        {
            update.Headers.Authorization = new AuthenticationHeaderValue("Prediction", created.EditToken);
            update.Content = JsonContent.Create(new UpdatePredictionRequest(
                "Joana updated", Gender.Girl, new DateOnly(2026, 10, 4), new TimeOnly(7, 40), 3275, 50, "Secret Baby"));
            Assert.Equal(HttpStatusCode.Conflict, (await client.SendAsync(update)).StatusCode);
        }

        var birth = new UpsertBirthRequest(
            Gender.Girl, new DateOnly(2026, 10, 4), new TimeOnly(7, 42), 3250, 50, "Secret Baby", "https://example.test/secret-photo.jpg");
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/admin/birth", birth)).StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var beforePublication = await client.GetStringAsync("/api/event");
        Assert.DoesNotContain("Secret Baby", beforePublication, StringComparison.Ordinal);
        Assert.DoesNotContain("3250", beforePublication, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-photo", beforePublication, StringComparison.Ordinal);
        using (var document = JsonDocument.Parse(beforePublication))
        {
            Assert.Equal((int)EventStatus.Born, document.RootElement.GetProperty("status").GetInt32());
            Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("birth").ValueKind);
        }
        Assert.Equal(HttpStatusCode.Conflict, (await client.GetAsync("/api/ranking")).StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/admin/birth/publish", null)).StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var published = await client.GetFromJsonAsync<EventResponse>("/api/event");
        Assert.NotNull(published?.Birth);
        Assert.Equal("Secret Baby", published.Birth.Name);
        Assert.Equal(50, published.Birth.HeightCentimeters);
        var ranking = await client.GetFromJsonAsync<RankingEntryResponse[]>("/api/ranking");
        Assert.Single(ranking!);
        Assert.Equal(created.Id, ranking![0].PredictionId);
        Assert.Equal(0, ranking[0].HeightDifferenceCentimeters);
    }

    private static async Task<CreatePredictionResponse> CreatePrediction(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/predictions", PredictionRequest("Joana"));
        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<CreatePredictionResponse>())!;
    }

    private static CreatePredictionRequest PredictionRequest(string participant) => new(
        participant, Gender.Girl, new DateOnly(2026, 10, 4), new TimeOnly(7, 40), 3275, 50, "Secret Baby");

    private static async Task<string> Login(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/admin/login", new LoginRequest("admin", "A-strong-test-password!"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
    }
}
