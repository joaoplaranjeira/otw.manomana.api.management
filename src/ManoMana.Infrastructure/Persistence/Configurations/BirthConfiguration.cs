using ManoMana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ManoMana.Infrastructure.Persistence.Configurations;

public sealed class BirthConfiguration : IEntityTypeConfiguration<Birth>
{
    public void Configure(EntityTypeBuilder<Birth> builder)
    {
        var dateConverter = new ValueConverter<DateOnly, DateTime>(
            value => value.ToDateTime(TimeOnly.MinValue),
            value => DateOnly.FromDateTime(value));
        var timeConverter = new ValueConverter<TimeOnly, TimeSpan>(
            value => value.ToTimeSpan(),
            value => TimeOnly.FromTimeSpan(value));

        builder.ToTable("Births");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Gender).HasConversion<int>().IsRequired();
        builder.Property(x => x.BirthDate).HasConversion(dateConverter).HasColumnType("date").IsRequired();
        builder.Property(x => x.BirthTime).HasConversion(timeConverter).HasColumnType("time").IsRequired();
        builder.Property(x => x.HeightCentimeters).HasColumnType("int").IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PhotoUrl).HasMaxLength(2048);
        builder.Property(x => x.CreatedAt).HasPrecision(6);
        builder.Property(x => x.PublishedAt).HasPrecision(6);
        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasOne(x => x.Event).WithOne(x => x.Birth).HasForeignKey<Birth>(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
    }
}
