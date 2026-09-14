using System.Data;
using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Domain.Entities;
using ManoMana.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ManoMana.Infrastructure.Repositories;

public sealed class EventRepository(ManoManaDbContext dbContext) : IEventRepository
{
    public Task<Event?> GetCurrentAsync(CancellationToken cancellationToken) => dbContext.Events
        .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);
    public Task AddAsync(Event entity, CancellationToken cancellationToken) =>
        dbContext.Events.AddAsync(entity, cancellationToken).AsTask();
}

public sealed class PredictionRepository(ManoManaDbContext dbContext) : IPredictionRepository
{
    public Task<Prediction?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Predictions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyCollection<Prediction>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken) =>
        await dbContext.Predictions.Where(x => x.EventId == eventId).OrderBy(x => x.CreatedAt).ToArrayAsync(cancellationToken);
    public Task AddAsync(Prediction prediction, CancellationToken cancellationToken) =>
        dbContext.Predictions.AddAsync(prediction, cancellationToken).AsTask();
    public void Remove(Prediction prediction) => dbContext.Predictions.Remove(prediction);
}

public sealed class BirthRepository(ManoManaDbContext dbContext) : IBirthRepository
{
    public Task<Birth?> GetByEventAsync(Guid eventId, CancellationToken cancellationToken) =>
        dbContext.Births.FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken);
    public Task AddAsync(Birth birth, CancellationToken cancellationToken) =>
        dbContext.Births.AddAsync(birth, cancellationToken).AsTask();
}

public sealed class AdminUserRepository(ManoManaDbContext dbContext) : IAdminUserRepository
{
    public Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken) =>
        dbContext.AdminUsers.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    public Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken) =>
        dbContext.AdminUsers.AddAsync(adminUser, cancellationToken).AsTask();
}

public sealed class UnitOfWork(ManoManaDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            await operation(cancellationToken);
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
