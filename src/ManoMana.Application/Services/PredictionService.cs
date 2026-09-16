using System.Security.Cryptography;
using System.Text;
using ManoMana.Application.Contracts;
using ManoMana.Application.Exceptions;
using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Application.Interfaces.Services;
using ManoMana.Application.Options;
using ManoMana.Domain.Entities;
using ManoMana.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManoMana.Application.Services;

public sealed class PredictionService(
    IEventRepository events,
    IPredictionRepository predictions,
    IUnitOfWork unitOfWork,
    IOptions<PredictionOptions> options,
    ILogger<PredictionService> logger) : IPredictionService
{
    private static readonly DateOnly MinimumDateIncludedInBirthDateAverage = new(2026, 9, 15);
    private readonly PredictionOptions _options = options.Value;

    public async Task<CreatePredictionResponse> CreateAsync(CreatePredictionRequest request, CancellationToken cancellationToken)
    {
        var current = await GetOpenEvent(cancellationToken);
        RequestValidator.ValidatePrediction(request.Name, request.PredictedWeightGrams, request.PredictedHeightCentimeters, request.PredictedName, request.PredictedBirthDate, _options);

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            EventId = current.Id,
            ParticipantName = request.Name.Trim(),
            Gender = request.Gender,
            PredictedBirthDate = request.PredictedBirthDate,
            PredictedBirthTime = request.PredictedBirthTime,
            PredictedWeightGrams = request.PredictedWeightGrams,
            PredictedHeightCentimeters = request.PredictedHeightCentimeters,
            PredictedName = NullIfWhiteSpace(request.PredictedName),
            CreatedAt = DateTimeOffset.UtcNow,
            EditTokenHash = HashToken(token)
        };

        await predictions.AddAsync(prediction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Prediction {PredictionId} was created for event {EventId}", prediction.Id, current.Id);
        return new(prediction.Id, token);
    }

    public async Task<PredictionResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var prediction = await predictions.GetByIdAsync(id, cancellationToken);
        return prediction is null ? null : Map(prediction);
    }

    public async Task UpdateAsync(Guid id, string token, UpdatePredictionRequest request, CancellationToken cancellationToken)
    {
        await GetOpenEvent(cancellationToken);
        var prediction = await predictions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PREDICTION_NOT_FOUND", "Prediction was not found.");
        if (string.IsNullOrWhiteSpace(token) || !TokenMatches(token, prediction.EditTokenHash))
            throw new ForbiddenException("INVALID_EDIT_TOKEN", "The prediction edit token is invalid.");
        RequestValidator.ValidatePrediction(request.Name, request.PredictedWeightGrams, request.PredictedHeightCentimeters, request.PredictedName, request.PredictedBirthDate, _options);

        prediction.ParticipantName = request.Name.Trim();
        prediction.Gender = request.Gender;
        prediction.PredictedBirthDate = request.PredictedBirthDate;
        prediction.PredictedBirthTime = request.PredictedBirthTime;
        prediction.PredictedWeightGrams = request.PredictedWeightGrams;
        prediction.PredictedHeightCentimeters = request.PredictedHeightCentimeters;
        prediction.PredictedName = NullIfWhiteSpace(request.PredictedName);
        prediction.UpdatedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Prediction {PredictionId} was updated", prediction.Id);
    }

    public async Task<PredictionStatsResponse> GetStatsAsync(CancellationToken cancellationToken)
    {
        var current = await events.GetCurrentAsync(cancellationToken)
            ?? throw new NotFoundException("EVENT_NOT_FOUND", "No current event exists.");
        var all = await predictions.GetByEventAsync(current.Id, cancellationToken);
        var boy = all.Count(x => x.Gender == Gender.Boy);
        var girl = all.Count(x => x.Gender == Gender.Girl);
        var dates = all
            .Where(x => x.PredictedBirthDate >= MinimumDateIncludedInBirthDateAverage)
            .Select(x => x.PredictedBirthDate!.Value.DayNumber)
            .ToArray();
        var weights = all.Where(x => x.PredictedWeightGrams.HasValue).Select(x => x.PredictedWeightGrams!.Value).ToArray();
        var heights = all.Where(x => x.PredictedHeightCentimeters.HasValue).Select(x => x.PredictedHeightCentimeters!.Value).ToArray();
        var total = all.Count;
        return new(
            total,
            boy,
            girl,
            total == 0 ? 0 : Math.Round(100m * boy / total, 2),
            total == 0 ? 0 : Math.Round(100m * girl / total, 2),
            dates.Length == 0 ? null : DateOnly.FromDayNumber((int)Math.Round(dates.Average())),
            weights.Length == 0 ? null : (int)Math.Round(weights.Average()),
            heights.Length == 0 ? null : (int)Math.Round(heights.Average()));
    }

    private async Task<Event> GetOpenEvent(CancellationToken cancellationToken)
    {
        var current = await events.GetCurrentAsync(cancellationToken)
            ?? throw new NotFoundException("EVENT_NOT_FOUND", "No current event exists.");
        if (current.Status != EventStatus.Open || current.PredictionsCloseAt is { } closesAt && closesAt <= DateTimeOffset.UtcNow)
            throw new ConflictException("EVENT_CLOSED", "Predictions are closed.");
        return current;
    }

    private static PredictionResponse Map(Prediction p) => new(
        p.Id, p.ParticipantName, p.Gender, p.PredictedBirthDate, p.PredictedBirthTime,
        p.PredictedWeightGrams, p.PredictedHeightCentimeters, p.PredictedName, p.CreatedAt, p.UpdatedAt);

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static bool TokenMatches(string token, string? expectedHash)
    {
        if (expectedHash is null || expectedHash.Length != 64) return false;
        var actual = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return CryptographicOperations.FixedTimeEquals(actual, Convert.FromHexString(expectedHash));
    }
}
