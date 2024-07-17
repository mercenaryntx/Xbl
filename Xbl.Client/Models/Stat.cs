﻿using System.Text.Json.Serialization;

namespace Xbl.Models;

public class Stat
{

    [JsonPropertyName("xuid")]
    public string XUID { get; set; }

    [JsonPropertyName("scid")]
    public string SCID { get; set; }

    [JsonPropertyName("titleid")]
    public string TitleId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    public int IntValue => Type == "Integer" && int.TryParse(Value, out var x) ? x : 0;

    [JsonPropertyName("groupproperties")]
    public StatGroupProperties GroupProperties { get; set; }

    [JsonPropertyName("properties")]
    public StatProperties StatProperties { get; set; }

    public override string ToString()
    {
        var name = StatProperties?.DisplayName ?? Name;
        return $"{name}={Value}";
    }
}