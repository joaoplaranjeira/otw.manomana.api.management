using ManoMana.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ManoMana.IntegrationTests;

public sealed class ManoManaApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"manomana-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.UseSetting("ConnectionStrings:Database", "Server=unused;Database=unused;User=unused;Password=unused");
        builder.UseSetting("Jwt:Issuer", "ManoManaTests");
        builder.UseSetting("Jwt:Audience", "ManoManaTests");
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-at-least-32-bytes");
        builder.UseSetting("Jwt:ExpirationMinutes", "10");
        builder.UseSetting("AdminSeed:Username", "admin");
        builder.UseSetting("AdminSeed:Password", "A-strong-test-password!");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ManoManaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ManoManaDbContext>>();
            services.RemoveAll<ManoManaDbContext>();
            services.AddDbContext<ManoManaDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
