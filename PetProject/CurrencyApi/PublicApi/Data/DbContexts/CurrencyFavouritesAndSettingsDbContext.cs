using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts;

public class CurrencyFavouritesAndSettingsDbContext : DbContext
{
    public CurrencyFavouritesAndSettingsDbContext(DbContextOptions<CurrencyFavouritesAndSettingsDbContext> options)
        : base(options) { }
    
    public DbSet<CurrencySettings> Settings { get; set; }
    
    public DbSet<CurrencyFavourite> FavouritesCurrenciesRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { 
        modelBuilder.HasDefaultSchema("user");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}