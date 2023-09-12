using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Configurations;

public class CacheTasksConfiguration : IEntityTypeConfiguration<CacheTask>
{
    public void Configure(EntityTypeBuilder<CacheTask> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.Id, p.LastUpdated });
        builder.Property(p => p.Id)
            .HasDefaultValueSql("uuid_generate_v4()");
    }
}