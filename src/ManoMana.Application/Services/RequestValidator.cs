using ManoMana.Application.Contracts;
using ManoMana.Application.Exceptions;
using ManoMana.Application.Options;

namespace ManoMana.Application.Services;

internal static class RequestValidator
{
    public static void ValidatePrediction(
        string name,
        int? weight,
        int? height,
        string? predictedName,
        DateOnly? predictedDate,
        PredictionOptions options)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length is < 2 or > 100)
            throw new ValidationException("INVALID_PARTICIPANT_NAME", "Participant name must contain between 2 and 100 characters.");
        if (weight is < 1000 or > 6000)
            throw new ValidationException("INVALID_WEIGHT", "Predicted weight must be between 1000 and 6000 grams.");
        if (height is < 30 or > 70)
            throw new ValidationException("INVALID_HEIGHT", "Predicted height must be between 30 and 70 centimeters.");
        if (predictedName?.Trim().Length > 100)
            throw new ValidationException("INVALID_PREDICTED_NAME", "Predicted name cannot exceed 100 characters.");
        if (predictedDate is { } date && options.EstimatedDueDate is { } dueDate)
        {
            var minimum = dueDate.AddDays(-options.AllowedDaysBefore);
            var maximum = dueDate.AddDays(options.AllowedDaysAfter);
            if (date < minimum || date > maximum)
                throw new ValidationException("INVALID_PREDICTED_DATE", $"Predicted date must be between {minimum:yyyy-MM-dd} and {maximum:yyyy-MM-dd}.");
        }
    }

    public static void ValidateBirth(UpsertBirthRequest request)
    {
        if (request.Name?.Trim().Length is < 1 or > 100)
            throw new ValidationException("INVALID_BIRTH_NAME", "Birth name must contain between 1 and 100 characters.");
        if (request.WeightGrams is < 1000 or > 6000)
            throw new ValidationException("INVALID_BIRTH_WEIGHT", "Birth weight must be between 1000 and 6000 grams.");
        if (request.HeightCentimeters is < 30 or > 70)
            throw new ValidationException("INVALID_BIRTH_HEIGHT", "Birth height must be between 30 and 70 centimeters.");
        if (request.PhotoUrl?.Length > 2048 || request.PhotoUrl is { Length: > 0 } value
            && !Uri.TryCreate(value, UriKind.Absolute, out _))
            throw new ValidationException("INVALID_PHOTO_URL", "Photo URL must be an absolute URL with at most 2048 characters.");
    }
}
