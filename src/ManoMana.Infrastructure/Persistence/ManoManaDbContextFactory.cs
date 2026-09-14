using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace ManoMana.Infrastructure.Persistence;

public sealed class ManoManaDbContextFactory : IDesignTimeDbContextFactory<ManoManaDbContext>
{
    public ManoManaDbContext CreateDbContext(string[] args)
    {
        var appSettingsPath = FindAppSettings();
        using var appSettings = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        var connectionString = appSettings.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("Database")
            .GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"ConnectionStrings:Database must be configured in {appSettingsPath}.");

        return new ManoManaDbContext(
            new DbContextOptionsBuilder<ManoManaDbContext>().UseMySQL(connectionString).Options);
    }

    private static string FindAppSettings()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.Combine(currentDirectory, "src", "ManoMana.Api", "appsettings.json"),
            Path.GetFullPath(Path.Combine(currentDirectory, "..", "ManoMana.Api", "appsettings.json"))
        };

        return candidates.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException(
                "Could not find src/ManoMana.Api/appsettings.json for design-time database operations.");
    }
}
