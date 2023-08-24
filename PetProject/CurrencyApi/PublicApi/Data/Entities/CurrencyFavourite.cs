
using System.ComponentModel.DataAnnotations;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;

public class CurrencyFavourite
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; }
    
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; set; }
}