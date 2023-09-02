using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;

public class CurrencyRateDbContext : DbContext
{
    public CurrencyRateDbContext(DbContextOptions<CurrencyRateDbContext> options)
        : base(options) { }

    public DbSet<CurrenciesRates> Currencies { get; set; }
    public DbSet<CurrencySettings> Settings { get; set; }
    public DbSet<CacheTask> CacheTasks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { 
        modelBuilder.HasDefaultSchema("cur");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.HasPostgresExtension("uuid-ossp");
    }
}