using System.Globalization;
using System.Text;
using ManoMana.Application.Contracts;
using ManoMana.Application.Exceptions;
using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Application.Interfaces.Services;
using ManoMana.Domain.Entities;
using ManoMana.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ManoMana.Application.Services;

public sealed class AdminService(
    IEventRepository events,
    IPredictionRepository predictions,
    IBirthRepository births,
    IUnitOfWork unitOfWork,
    ILogger<AdminService> logger) : IAdminService
{
    public async Task<IReadOnlyCollection<AdminPredictionResponse>> GetPredictionsAsync(CancellationToken cancellationToken)
    {
        var current = await Current(cancellationToken);
        var all = await predictions.GetByEventAsync(current.Id, cancellationToken);
        return all.Select(x => new AdminPredictionResponse(
            x.Id, x.ParticipantName, x.Gender, x.PredictedBirthDate, x.PredictedBirthTime,
            x.PredictedWeightGrams, x.PredictedHeightCentimeters, x.PredictedName, x.CreatedAt)).ToArray();
    }

    public async Task DeletePredictionAsync(Guid id, CancellationToken cancellationToken)
    {
        var prediction = await predictions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PREDICTION_NOT_FOUND", "Prediction was not found.");
        predictions.Remove(prediction);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Admin deleted prediction {PredictionId}", id);
    }

    public async Task OpenEventAsync(CancellationToken cancellationToken)
    {
        var current = await Current(cancellationToken);
        if (current.Status == EventStatus.Open) return;
        if (current.Status != EventStatus.Closed)
            throw new ConflictException("INVALID_EVENT_TRANSITION", "Only a closed event can be reopened.");
        current.Status = EventStatus.Open;
        current.PredictionsCloseAt = null;
        current.UpdatedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CloseEventAsync(CancellationToken cancellationToken)
    {
        var current = await Current(cancellationToken);
        if (current.Status == EventStatus.Closed) return;
        if (current.Status != EventStatus.Open)
            throw new ConflictException("INVALID_EVENT_TRANSITION", "Only an open event can be closed.");
        current.Status = EventStatus.Closed;
        current.PredictionsCloseAt = DateTimeOffset.UtcNow;
        current.UpdatedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task ResetEventAsync(CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var current = await Current(ct);
            var currentPredictions = await predictions.GetByEventAsync(current.Id, ct);
            var currentBirth = await births.GetByEventAsync(current.Id, ct);
            var now = DateTimeOffset.UtcNow;

            predictions.RemoveRange(currentPredictions);
            if (currentBirth is not null) births.Remove(currentBirth);
            events.Remove(current);
            await events.AddAsync(new Event
            {
                Id = Guid.NewGuid(),
                Name = current.Name,
                Status = EventStatus.Open,
                CreatedAt = now,
                UpdatedAt = now
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
            logger.LogWarning("Event {EventId} and all of its data were reset", current.Id);
        }, cancellationToken);

    public async Task CreateBirthAsync(UpsertBirthRequest request, CancellationToken cancellationToken)
    {
        RequestValidator.ValidateBirth(request);
        var current = await Current(cancellationToken);
        if (current.Status != EventStatus.Closed)
            throw new ConflictException("INVALID_EVENT_TRANSITION", "Birth can only be registered for a closed event.");
        if (await births.GetByEventAsync(current.Id, cancellationToken) is not null)
            throw new ConflictException("BIRTH_ALREADY_EXISTS", "Birth has already been registered.");

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await births.AddAsync(new Birth
            {
                Id = Guid.NewGuid(), EventId = current.Id, Gender = request.Gender,
                BirthDate = request.BirthDate, BirthTime = request.BirthTime,
                WeightGrams = request.WeightGrams, HeightCentimeters = request.HeightCentimeters,
                Name = request.Name.Trim(),
                PhotoUrl = NullIfWhiteSpace(request.PhotoUrl), CreatedAt = DateTimeOffset.UtcNow
            }, ct);
            current.Status = EventStatus.Born;
            current.UpdatedAt = DateTimeOffset.UtcNow;
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
        logger.LogInformation("Birth was registered for event {EventId}; result remains private", current.Id);
    }

    public async Task UpdateBirthAsync(UpsertBirthRequest request, CancellationToken cancellationToken)
    {
        RequestValidator.ValidateBirth(request);
        var current = await Current(cancellationToken);
        if (current.Status != EventStatus.Born)
            throw new ConflictException("BIRTH_NOT_EDITABLE", "Birth can only be edited before publication.");
        var birth = await births.GetByEventAsync(current.Id, cancellationToken)
            ?? throw new NotFoundException("BIRTH_NOT_FOUND", "Birth has not been registered.");
        birth.Gender = request.Gender;
        birth.BirthDate = request.BirthDate;
        birth.BirthTime = request.BirthTime;
        birth.WeightGrams = request.WeightGrams;
        birth.HeightCentimeters = request.HeightCentimeters;
        birth.Name = request.Name.Trim();
        birth.PhotoUrl = NullIfWhiteSpace(request.PhotoUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task PublishBirthAsync(CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var current = await Current(ct);
            if (current.Status == EventStatus.Published)
                throw new ConflictException("ALREADY_PUBLISHED", "The result has already been published.");
            if (current.Status != EventStatus.Born)
                throw new ConflictException("INVALID_EVENT_TRANSITION", "Only a registered birth can be published.");
            var birth = await births.GetByEventAsync(current.Id, ct)
                ?? throw new NotFoundException("BIRTH_NOT_FOUND", "Birth has not been registered.");
            birth.PublishedAt = DateTimeOffset.UtcNow;
            current.Status = EventStatus.Published;
            current.UpdatedAt = DateTimeOffset.UtcNow;
            await unitOfWork.SaveChangesAsync(ct);
            logger.LogInformation("Birth result was published for event {EventId}", current.Id);
        }, cancellationToken);

    public async Task<byte[]> ExportPredictionsCsvAsync(CancellationToken cancellationToken)
    {
        var values = await GetPredictionsAsync(cancellationToken);
        var builder = new StringBuilder("Participant,Gender,PredictedDate,PredictedTime,PredictedWeight,PredictedHeight,PredictedName,CreatedAt\r\n");
        foreach (var value in values)
        {
            builder.AppendJoin(',',
                Csv(value.ParticipantName), value.Gender.ToString(),
                value.PredictedBirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                value.PredictedBirthTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty,
                value.PredictedWeightGrams?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                value.PredictedHeightCentimeters?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                Csv(value.PredictedName), value.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
            builder.Append("\r\n");
        }
        return new UTF8Encoding(true).GetBytes(builder.ToString());
    }

    private async Task<Event> Current(CancellationToken cancellationToken) =>
        await events.GetCurrentAsync(cancellationToken)
        ?? throw new NotFoundException("EVENT_NOT_FOUND", "No current event exists.");

    private static string Csv(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
