using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Application.Interfaces.Services;
using ManoMana.Application.Options;
using ManoMana.Domain.Entities;
using ManoMana.Domain.Enums;
using ManoMana.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManoMana.Infrastructure.Seeding;

public sealed class DatabaseSeeder(
    ManoManaDbContext dbContext,
    IEventRepository events,
    IAdminUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IOptions<AdminSeedOptions> options,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.IsRelational()) await dbContext.Database.MigrateAsync(cancellationToken);
        else await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await events.GetCurrentAsync(cancellationToken) is null)
        {
            var now = DateTimeOffset.UtcNow;
            await events.AddAsync(new Event
            {
                Id = Guid.NewGuid(), Name = "mano mana", Status = EventStatus.Open, CreatedAt = now, UpdatedAt = now
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var seed = options.Value;
        if (string.IsNullOrWhiteSpace(seed.Username) || string.IsNullOrEmpty(seed.Password))
        {
            logger.LogWarning("Initial admin was not seeded because AdminSeed credentials are not configured");
            return;
        }
        if (await users.GetByUsernameAsync(seed.Username.Trim(), cancellationToken) is not null) return;
        await users.AddAsync(new AdminUser
        {
            Id = Guid.NewGuid(), Username = seed.Username.Trim(), PasswordHash = passwordHasher.Hash(seed.Password), CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Initial admin {Username} was created", seed.Username.Trim());
    }
}
