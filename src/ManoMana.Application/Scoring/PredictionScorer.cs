using ManoMana.Domain.Entities;

namespace ManoMana.Application.Scoring;

public sealed record PredictionScore(
    int Total,
    bool GenderCorrect,
    int? DateDifferenceDays,
    int? TimeDifferenceMinutes,
    int? WeightDifferenceGrams,
    int? HeightDifferenceCentimeters,
    bool NameCorrect);

public interface IPredictionScorer
{
    PredictionScore Score(Prediction prediction, Birth birth);
}

public sealed class PredictionScorer : IPredictionScorer
{
    public PredictionScore Score(Prediction prediction, Birth birth)
    {
        var genderCorrect = prediction.Gender == birth.Gender;
        int? dateDifference = prediction.PredictedBirthDate is { } date
            ? Math.Abs(date.DayNumber - birth.BirthDate.DayNumber)
            : null;
        int? timeDifference = prediction.PredictedBirthTime is { } time
            ? CircularMinuteDifference(time, birth.BirthTime)
            : null;
        int? weightDifference = prediction.PredictedWeightGrams is { } weight
            ? Math.Abs(weight - birth.WeightGrams)
            : null;
        int? heightDifference = prediction.PredictedHeightCentimeters is { } height
            ? Math.Abs(height - birth.HeightCentimeters)
            : null;
        var nameCorrect = !string.IsNullOrWhiteSpace(prediction.PredictedName)
            && string.Equals(prediction.PredictedName.Trim(), birth.Name.Trim(), StringComparison.OrdinalIgnoreCase);

        var total = (genderCorrect ? 100 : 0)
            + ScoreDate(dateDifference)
            + ScoreTime(timeDifference)
            + ScoreWeight(weightDifference)
            + ScoreHeight(heightDifference);

        return new(total, genderCorrect, dateDifference, timeDifference, weightDifference, heightDifference, nameCorrect);
    }

    private static int ScoreDate(int? days) => days switch
    {
        0 => 40, 1 => 35, 2 => 30, 3 => 25, 4 => 20, 5 => 15, 6 => 10, 7 => 5, _ => 0
    };

    private static int ScoreTime(int? minutes) => minutes is null
        ? 0
        : Math.Clamp((int)Math.Round(30m * (1m - minutes.Value / 720m), MidpointRounding.AwayFromZero), 0, 30);

    private static int ScoreWeight(int? grams) => grams switch
    {
        <= 25 => 30, <= 50 => 25, <= 100 => 20, <= 150 => 15, <= 250 => 10, <= 400 => 5, _ => 0
    };

    private static int ScoreHeight(int? centimeters) => centimeters switch
    {
        0 => 30, 1 => 25, 2 => 20, 3 => 15, 4 => 10, 5 => 5, _ => 0
    };

    private static int CircularMinuteDifference(TimeOnly left, TimeOnly right)
    {
        var difference = Math.Abs((int)(left.ToTimeSpan() - right.ToTimeSpan()).TotalMinutes);
        return Math.Min(difference, 1440 - difference);
    }
}
