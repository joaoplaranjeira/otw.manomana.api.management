using ManoMana.Domain.Enums;

namespace ManoMana.Application.Contracts;

public sealed record BirthPublicResponse(
    Gender Gender,
    DateOnly BirthDate,
    TimeOnly BirthTime,
    int WeightGrams,
    int HeightCentimeters,
    string Name,
    string? PhotoUrl);

public sealed record EventResponse(
    Guid Id,
    string Name,
    EventStatus Status,
    DateTimeOffset? PredictionsCloseAt,
    BirthPublicResponse? Birth);

public sealed record CreatePredictionRequest(
    string Name,
    Gender Gender,
    DateOnly? PredictedBirthDate,
    TimeOnly? PredictedBirthTime,
    int? PredictedWeightGrams,
    int? PredictedHeightCentimeters,
    string? PredictedName);

public sealed record UpdatePredictionRequest(
    string Name,
    Gender Gender,
    DateOnly? PredictedBirthDate,
    TimeOnly? PredictedBirthTime,
    int? PredictedWeightGrams,
    int? PredictedHeightCentimeters,
    string? PredictedName);

public sealed record CreatePredictionResponse(Guid Id, string EditToken);

public sealed record PredictionResponse(
    Guid Id,
    string ParticipantName,
    Gender Gender,
    DateOnly? PredictedBirthDate,
    TimeOnly? PredictedBirthTime,
    int? PredictedWeightGrams,
    int? PredictedHeightCentimeters,
    string? PredictedName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record PredictionStatsResponse(
    int Total,
    int Boy,
    int Girl,
    decimal BoyPercentage,
    decimal GirlPercentage,
    DateOnly? AveragePredictedBirthDate,
    int? AveragePredictedWeightGrams,
    int? AveragePredictedHeightCentimeters);

public sealed record RankingEntryResponse(
    int Position,
    Guid PredictionId,
    string ParticipantName,
    int TotalScore,
    bool GenderCorrect,
    int? BirthDateDifferenceDays,
    int? BirthTimeDifferenceMinutes,
    int? WeightDifferenceGrams,
    int? HeightDifferenceCentimeters,
    bool NameCorrect);
