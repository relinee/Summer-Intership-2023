﻿using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

public record CurrencyData
{
    [JsonPropertyName("code")]
    public string Code { get; init; }
    
    [JsonPropertyName("value")]
    public decimal Value { get; init; }
}