namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

public record ApiStatus
{
    public long Account_Id { get; init; }
    public Quotas Quotas { get; init; }
}



