using ManoMana.Application.Contracts;
using ManoMana.Application.Exceptions;
using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Application.Interfaces.Services;
using ManoMana.Domain.Enums;

namespace ManoMana.Application.Services;

public sealed class EventService(IEventRepository events, IBirthRepository births) : IEventService
{
    public async Task<EventResponse> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var current = await events.GetCurrentAsync(cancellationToken)
            ?? throw new NotFoundException("EVENT_NOT_FOUND", "No current event exists.");

        BirthPublicResponse? publicBirth = null;
        if (current.Status == EventStatus.Published)
        {
            var birth = await births.GetByEventAsync(current.Id, cancellationToken)
                ?? throw new ConflictException("BIRTH_NOT_FOUND", "The published event has no birth result.");
            publicBirth = new(birth.Gender, birth.BirthDate, birth.BirthTime, birth.WeightGrams,
                birth.HeightCentimeters, birth.Name, birth.PhotoUrl);
        }

        return new(current.Id, current.Name, current.Status, current.PredictionsCloseAt, publicBirth);
    }
}
