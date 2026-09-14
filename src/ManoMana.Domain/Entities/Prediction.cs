using ManoMana.Domain.Enums;

namespace ManoMana.Domain.Entities;

public sealed class Prediction
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string ParticipantName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateOnly? PredictedBirthDate { get; set; }
    public TimeOnly? PredictedBirthTime { get; set; }
    public int? PredictedWeightGrams { get; set; }
    public int? PredictedHeightCentimeters { get; set; }
    public string? PredictedName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? EditTokenHash { get; set; }
    public Event Event { get; set; } = null!;
}
