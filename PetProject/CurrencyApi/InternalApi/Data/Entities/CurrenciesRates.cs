
namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;

public class CurrenciesRates
{
    public int Id { get; set; }
    
    public string BaseCurrency { get; set; }
    
    public CurrencyRate[] Currencies { get; set; }
    
    public DateTimeOffset DateTime { get; set; }
}