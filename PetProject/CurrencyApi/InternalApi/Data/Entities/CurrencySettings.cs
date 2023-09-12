using System.ComponentModel.DataAnnotations;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;

public class CurrencySettings
{
    public int Id { get; set; }
    
    [StringLength(3, MinimumLength = 3)]
    public string DefaultCurrency { get; set; }
}