namespace ManoMana.Application.Options;

public sealed class PredictionOptions
{
    public const string SectionName = "Predictions";
    public DateOnly? EstimatedDueDate { get; set; }
    public int AllowedDaysBefore { get; set; } = 30;
    public int AllowedDaysAfter { get; set; } = 30;
}

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "ManoMana";
    public string Audience { get; set; } = "ManoMana";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 120;
}

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
