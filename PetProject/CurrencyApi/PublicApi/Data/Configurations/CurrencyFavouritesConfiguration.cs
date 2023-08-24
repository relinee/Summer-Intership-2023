using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Configurations;

public class CurrencyFavouritesConfiguration : IEntityTypeConfiguration<CurrencyFavourite>
{
    public void Configure(EntityTypeBuilder<CurrencyFavourite> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Name).IsUnique();
        builder.HasIndex(p => new { p.Currency, p.BaseCurrency }).IsUnique();
    }
}