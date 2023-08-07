namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

public record CurrencyData
{
    public string Code { get; init; }
    public decimal Value { get; init; }
}