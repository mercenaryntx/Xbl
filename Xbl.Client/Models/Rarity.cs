﻿using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class Rarity
{
    [JsonPropertyName("currentCategory")]
    public string CurrentCategory { get; set; }
    [JsonPropertyName("currentPercentage")]
    public double CurrentPercentage { get; set; }
}