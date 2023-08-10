namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

public record ErrorApiResponse
{
    public string Message { get; init; }
    public ErrorApiDetails Errors { get; init; }
}