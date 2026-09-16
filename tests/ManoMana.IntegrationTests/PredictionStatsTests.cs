using System.Net;
using System.Net.Http.Json;
using ManoMana.Application.Contracts;
using ManoMana.Domain.Enums;
using Xunit;

namespace ManoMana.IntegrationTests;

public sealed class PredictionStatsTests
{
    [Fact]
    public async Task Birth_date_average_ignores_dates_before_15_september_2026()
    {
        await using var factory = new ManoManaApiFactory();
        using var client = factory.CreateClient();

        await CreatePrediction(client, "Before cutoff", new DateOnly(2026, 9, 14));
        await CreatePrediction(client, "On cutoff", new DateOnly(2026, 9, 15));
        await CreatePrediction(client, "After cutoff", new DateOnly(2026, 9, 17));

        var stats = await client.GetFromJsonAsync<PredictionStatsResponse>("/api/predictions/stats");

        Assert.NotNull(stats);
        Assert.Equal(new DateOnly(2026, 9, 16), stats.AveragePredictedBirthDate);
    }

    private static async Task CreatePrediction(HttpClient client, string participant, DateOnly predictedBirthDate)
    {
        var request = new CreatePredictionRequest(
            participant,
            Gender.Girl,
            predictedBirthDate,
            new TimeOnly(12, 0),
            3200,
            50,
            "Baby");

        var response = await client.PostAsJsonAsync("/api/predictions", request);

        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
    }
}
