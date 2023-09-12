using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Configurations;

public class CurrencySettingsConfiguration : IEntityTypeConfiguration<CurrencySettings>
{
    public void Configure(EntityTypeBuilder<CurrencySettings> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.DefaultCurrency).IsUnique();
    }
}