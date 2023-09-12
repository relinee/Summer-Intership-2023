using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Configurations;

public class CurrenciesRatesConfiguration : IEntityTypeConfiguration<CurrenciesRates>
{
    public void Configure(EntityTypeBuilder<CurrenciesRates> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Currencies)
            .HasColumnType("jsonb");
        builder.HasIndex(p => new { p.BaseCurrency, p.DateTime });
        
    }
}