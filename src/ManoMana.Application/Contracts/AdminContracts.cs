using ManoMana.Domain.Enums;

namespace ManoMana.Application.Contracts;

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);

public sealed record UpsertBirthRequest(
    Gender Gender,
    DateOnly BirthDate,
    TimeOnly BirthTime,
    int WeightGrams,
    int HeightCentimeters,
    string Name,
    string? PhotoUrl);

public sealed record AdminPredictionResponse(
    Guid Id,
    string ParticipantName,
    Gender Gender,
    DateOnly? PredictedBirthDate,
    TimeOnly? PredictedBirthTime,
    int? PredictedWeightGrams,
    int? PredictedHeightCentimeters,
    string? PredictedName,
    DateTimeOffset CreatedAt);

public sealed record ErrorResponse(string Code, string Message);
