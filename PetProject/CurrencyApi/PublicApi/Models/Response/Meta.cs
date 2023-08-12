using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

public record Meta
{
    [JsonPropertyName("last_updated_at")]
    public DateTimeOffset LastUpdatedAt { get; init; }
}