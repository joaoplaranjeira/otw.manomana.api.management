using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Application.Interfaces.Services;
using ManoMana.Infrastructure.Authentication;
using ManoMana.Infrastructure.Persistence;
using ManoMana.Infrastructure.Repositories;
using ManoMana.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManoMana.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:Database must be configured.");

        services.AddDbContext<ManoManaDbContext>(options => options.UseMySQL(connectionString));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IPredictionRepository, PredictionRepository>();
        services.AddScoped<IBirthRepository, BirthRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<DatabaseSeeder>();
        return services;
    }
}
