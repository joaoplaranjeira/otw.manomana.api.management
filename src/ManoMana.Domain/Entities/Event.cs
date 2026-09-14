using ManoMana.Domain.Enums;

namespace ManoMana.Domain.Entities;

public sealed class Event
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public EventStatus Status { get; set; }
    public DateTimeOffset? PredictionsCloseAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Prediction> Predictions { get; set; } = [];
    public Birth? Birth { get; set; }
}
