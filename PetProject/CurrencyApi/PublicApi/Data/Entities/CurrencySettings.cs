using System.ComponentModel.DataAnnotations;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;

public class CurrencySettings
{
    [StringLength(3, MinimumLength = 3)]
    public string DefaultCurrency { get; set; }
    
    public uint CurrencyRoundCount { get; set; }
}