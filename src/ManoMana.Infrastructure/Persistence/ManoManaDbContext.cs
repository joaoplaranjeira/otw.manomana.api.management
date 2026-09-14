using ManoMana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManoMana.Infrastructure.Persistence;

public sealed class ManoManaDbContext(DbContextOptions<ManoManaDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<Birth> Births => Set<Birth>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManoManaDbContext).Assembly);
}
