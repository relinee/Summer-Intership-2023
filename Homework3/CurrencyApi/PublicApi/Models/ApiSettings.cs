namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

public record ApiSettings
{
    public string ApiKey { get; init; }
    public string BaseCurrency { get; init; }
    public string DefaultCurrency { get; init; }
    
    public int DecimalPlaces { get; init; }
}