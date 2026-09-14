using ManoMana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ManoMana.Infrastructure.Persistence.Configurations;

public sealed class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
{
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        var dateConverter = new ValueConverter<DateOnly, DateTime>(
            value => value.ToDateTime(TimeOnly.MinValue),
            value => DateOnly.FromDateTime(value));
        var timeConverter = new ValueConverter<TimeOnly, TimeSpan>(
            value => value.ToTimeSpan(),
            value => TimeOnly.FromTimeSpan(value));

        builder.ToTable("Predictions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ParticipantName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Gender).HasConversion<int>().IsRequired();
        builder.Property(x => x.PredictedBirthDate).HasConversion(dateConverter).HasColumnType("date");
        builder.Property(x => x.PredictedBirthTime).HasConversion(timeConverter).HasColumnType("time");
        builder.Property(x => x.PredictedHeightCentimeters).HasColumnType("int");
        builder.Property(x => x.PredictedName).HasMaxLength(100);
        builder.Property(x => x.EditTokenHash).HasMaxLength(64);
        builder.Property(x => x.CreatedAt).HasPrecision(6);
        builder.Property(x => x.UpdatedAt).HasPrecision(6);
        builder.HasIndex(x => x.EventId);
        builder.HasOne(x => x.Event).WithMany(x => x.Predictions).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}
