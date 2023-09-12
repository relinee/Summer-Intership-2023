using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Configurations;

public class CurrencySettingsConfiguration : IEntityTypeConfiguration<CurrencySettings>
{
    public void Configure(EntityTypeBuilder<CurrencySettings> builder)
    {
        builder.HasNoKey();
    }
}