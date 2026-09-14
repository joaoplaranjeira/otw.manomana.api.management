using ManoMana.Application.Interfaces.Services;
using ManoMana.Application.Scoring;
using ManoMana.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ManoMana.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<IEventService, EventService>()
        .AddScoped<IPredictionService, PredictionService>()
        .AddScoped<IRankingService, RankingService>()
        .AddScoped<IAdminService, AdminService>()
        .AddScoped<IAuthenticationService, AuthenticationService>()
        .AddSingleton<IPredictionScorer, PredictionScorer>();
}
