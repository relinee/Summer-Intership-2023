namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;

public class CacheTask
{
    public Guid Id { get; set; }
    public CacheTaskStatus Status { get; set; }
    public string NewBaseCurrency { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}