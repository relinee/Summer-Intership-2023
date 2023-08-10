namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;

public record ApiSettings
{
    public string ApiKey { get; init; }
    public string BaseCurrency { get; init; }
    public string DefaultCurrency { get; init; }
    public int DecimalPlaces { get; init; }
    public string BaseUrl { get; init; }
}