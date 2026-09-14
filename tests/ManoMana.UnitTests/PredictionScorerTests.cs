using ManoMana.Application.Scoring;
using ManoMana.Domain.Entities;
using ManoMana.Domain.Enums;
using Xunit;

namespace ManoMana.UnitTests;

public sealed class PredictionScorerTests
{
    private readonly PredictionScorer _scorer = new();
    private readonly Birth _birth = new()
    {
        Gender = Gender.Girl,
        BirthDate = new DateOnly(2026, 10, 4),
        BirthTime = new TimeOnly(0, 5),
        WeightGrams = 3250,
        HeightCentimeters = 50,
        Name = "Maria"
    };

    [Fact]
    public void Exact_prediction_receives_maximum_score()
    {
        var prediction = Prediction(Gender.Girl, new DateOnly(2026, 10, 4), new TimeOnly(0, 5), 3250, 50, "maria");

        var result = _scorer.Score(prediction, _birth);

        Assert.Equal(230, result.Total);
        Assert.True(result.GenderCorrect);
        Assert.True(result.NameCorrect);
    }

    [Fact]
    public void Time_difference_wraps_around_midnight()
    {
        var prediction = Prediction(Gender.Boy, null, new TimeOnly(23, 55), null, null, null);

        var result = _scorer.Score(prediction, _birth);

        Assert.Equal(10, result.TimeDifferenceMinutes);
        Assert.Equal(30, result.Total);
    }

    [Theory]
    [InlineData(0, 40)]
    [InlineData(7, 5)]
    [InlineData(8, 0)]
    public void Date_score_uses_specified_bands(int difference, int expected)
    {
        var prediction = Prediction(Gender.Boy, _birth.BirthDate.AddDays(difference), null, null, null, null);
        Assert.Equal(expected, _scorer.Score(prediction, _birth).Total);
    }

    [Theory]
    [InlineData(25, 30)]
    [InlineData(50, 25)]
    [InlineData(400, 5)]
    [InlineData(401, 0)]
    public void Weight_score_uses_specified_bands(int difference, int expected)
    {
        var prediction = Prediction(Gender.Boy, null, null, _birth.WeightGrams + difference, null, null);
        Assert.Equal(expected, _scorer.Score(prediction, _birth).Total);
    }

    [Theory]
    [InlineData(0, 30)]
    [InlineData(1, 25)]
    [InlineData(5, 5)]
    [InlineData(6, 0)]
    public void Height_score_uses_specified_bands(int difference, int expected)
    {
        var prediction = Prediction(Gender.Boy, null, null, null, _birth.HeightCentimeters + difference, null);
        Assert.Equal(expected, _scorer.Score(prediction, _birth).Total);
    }

    private static Prediction Prediction(
        Gender gender,
        DateOnly? date,
        TimeOnly? time,
        int? weight,
        int? height,
        string? name) => new()
    {
        Gender = gender,
        PredictedBirthDate = date,
        PredictedBirthTime = time,
        PredictedWeightGrams = weight,
        PredictedHeightCentimeters = height,
        PredictedName = name
    };
}
