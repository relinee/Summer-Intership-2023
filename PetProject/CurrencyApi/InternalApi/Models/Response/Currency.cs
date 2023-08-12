namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

/// <summary>
/// Курс валюты
/// </summary>
/// <param name="Code">Код валюты</param>
/// <param name="Value">Значение курса валют, относительно базовой валюты</param>
public record Currency(string Code, decimal Value);