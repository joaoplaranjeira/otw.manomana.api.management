using ManoMana.Application.Contracts;
using ManoMana.Domain.Entities;

namespace ManoMana.Application.Interfaces.Services;

public interface IEventService
{
    Task<EventResponse> GetCurrentAsync(CancellationToken cancellationToken);
}

public interface IPredictionService
{
    Task<CreatePredictionResponse> CreateAsync(CreatePredictionRequest request, CancellationToken cancellationToken);
    Task<PredictionResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Guid id, string token, UpdatePredictionRequest request, CancellationToken cancellationToken);
    Task<PredictionStatsResponse> GetStatsAsync(CancellationToken cancellationToken);
}

public interface IRankingService
{
    Task<IReadOnlyCollection<RankingEntryResponse>> GetAsync(CancellationToken cancellationToken);
}

public interface IAdminService
{
    Task<IReadOnlyCollection<AdminPredictionResponse>> GetPredictionsAsync(CancellationToken cancellationToken);
    Task DeletePredictionAsync(Guid id, CancellationToken cancellationToken);
    Task OpenEventAsync(CancellationToken cancellationToken);
    Task CloseEventAsync(CancellationToken cancellationToken);
    Task ResetEventAsync(CancellationToken cancellationToken);
    Task CreateBirthAsync(UpsertBirthRequest request, CancellationToken cancellationToken);
    Task UpdateBirthAsync(UpsertBirthRequest request, CancellationToken cancellationToken);
    Task PublishBirthAsync(CancellationToken cancellationToken);
    Task<byte[]> ExportPredictionsCsvAsync(CancellationToken cancellationToken);
}

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenIssuer
{
    LoginResponse Issue(AdminUser user);
}
