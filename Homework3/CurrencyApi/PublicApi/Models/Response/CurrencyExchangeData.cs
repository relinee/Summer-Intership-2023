namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

public record CurrencyExchangeData
{
    public Meta Meta { get; init; }
    public Dictionary<string, CurrencyData> Data { get; init; }
}