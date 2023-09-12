namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response.ExternalAPI;

public record ApiStatus
{
    public long Account_Id { get; init; }
    public Quotas Quotas { get; init; }
}



