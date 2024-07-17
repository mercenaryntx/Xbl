﻿using System.Text.Json.Serialization;

namespace Xbl.Models;

public class MediaAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
}