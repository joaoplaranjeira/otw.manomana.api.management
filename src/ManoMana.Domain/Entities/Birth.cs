using ManoMana.Domain.Enums;

namespace ManoMana.Domain.Entities;

public sealed class Birth
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Gender Gender { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly BirthTime { get; set; }
    public int WeightGrams { get; set; }
    public int HeightCentimeters { get; set; }
    public string Name { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public Event Event { get; set; } = null!;
}
