using System.ComponentModel.DataAnnotations;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;

public enum CacheTaskStatus
{
    [Display(Name = "Задача создана")]
    Created,
    [Display(Name = "Задача в обработке")]
    InProgress, 
    [Display(Name = "Задача завершена успешно")]
    CompletedSuccessfully,
    [Display(Name = "Задача завершена с ошибкой")]
    CompletedWithError,
    [Display(Name = "Задача отменена")]
    Canceled
}