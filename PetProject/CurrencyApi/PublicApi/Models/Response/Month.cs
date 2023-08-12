namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

public record Month
{
    public int Total { get; init; }
    public int Used { get; init; }
    public int Remaining { get; init; }
}