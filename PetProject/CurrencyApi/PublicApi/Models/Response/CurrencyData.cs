﻿using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

public record CurrencyData
{
    [JsonPropertyName("code")]
    public string Code { get; init; }
    
    [JsonPropertyName("value")]
    public decimal Value { get; init; }
}