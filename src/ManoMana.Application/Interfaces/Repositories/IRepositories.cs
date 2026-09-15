using ManoMana.Domain.Entities;

namespace ManoMana.Application.Interfaces.Repositories;

public interface IEventRepository
{
    Task<Event?> GetCurrentAsync(CancellationToken cancellationToken);
    Task AddAsync(Event entity, CancellationToken cancellationToken);
    void Remove(Event entity);
}

public interface IPredictionRepository
{
    Task<Prediction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Prediction>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken);
    Task AddAsync(Prediction prediction, CancellationToken cancellationToken);
    void Remove(Prediction prediction);
    void RemoveRange(IEnumerable<Prediction> predictions);
}

public interface IBirthRepository
{
    Task<Birth?> GetByEventAsync(Guid eventId, CancellationToken cancellationToken);
    Task AddAsync(Birth birth, CancellationToken cancellationToken);
    void Remove(Birth birth);
}

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
