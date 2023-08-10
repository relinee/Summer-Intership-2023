namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

/// <summary>
/// Курс валюты
/// </summary>
/// <param name="CurrencyType">Валюта</param>
/// <param name="Value">Значение курса</param>
public record CurrencyDTO(CurrencyType CurrencyType, decimal Value);