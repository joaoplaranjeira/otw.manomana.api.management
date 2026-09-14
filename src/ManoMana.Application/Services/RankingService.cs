using ManoMana.Application.Contracts;
using ManoMana.Application.Exceptions;
using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Application.Interfaces.Services;
using ManoMana.Application.Scoring;
using ManoMana.Domain.Enums;

namespace ManoMana.Application.Services;

public sealed class RankingService(
    IEventRepository events,
    IPredictionRepository predictions,
    IBirthRepository births,
    IPredictionScorer scorer) : IRankingService
{
    public async Task<IReadOnlyCollection<RankingEntryResponse>> GetAsync(CancellationToken cancellationToken)
    {
        var current = await events.GetCurrentAsync(cancellationToken)
            ?? throw new NotFoundException("EVENT_NOT_FOUND", "No current event exists.");
        if (current.Status != EventStatus.Published)
            throw new ConflictException("RESULT_NOT_PUBLISHED", "The result has not been published yet.");
        var birth = await births.GetByEventAsync(current.Id, cancellationToken)
            ?? throw new ConflictException("BIRTH_NOT_FOUND", "The published event has no birth result.");
        var all = await predictions.GetByEventAsync(current.Id, cancellationToken);

        var ordered = all.Select(p => (Prediction: p, Score: scorer.Score(p, birth)))
            .OrderByDescending(x => x.Score.Total)
            .ThenByDescending(x => x.Score.GenderCorrect)
            .ThenBy(x => x.Score.DateDifferenceDays ?? int.MaxValue)
            .ThenBy(x => x.Score.TimeDifferenceMinutes ?? int.MaxValue)
            .ThenBy(x => x.Score.WeightDifferenceGrams ?? int.MaxValue)
            .ThenBy(x => x.Score.HeightDifferenceCentimeters ?? int.MaxValue)
            .ThenBy(x => x.Prediction.CreatedAt)
            .ToArray();

        return ordered.Select((x, index) => new RankingEntryResponse(
            index + 1, x.Prediction.Id, x.Prediction.ParticipantName, x.Score.Total,
            x.Score.GenderCorrect, x.Score.DateDifferenceDays, x.Score.TimeDifferenceMinutes,
            x.Score.WeightDifferenceGrams, x.Score.HeightDifferenceCentimeters, x.Score.NameCorrect)).ToArray();
    }
}
