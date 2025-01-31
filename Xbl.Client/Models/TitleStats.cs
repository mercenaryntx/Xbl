﻿using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class TitleStats
{
    [JsonPropertyName("groups")]
    public TitleStatGroup[] Groups { get; set; }

    [JsonPropertyName("statlistscollection")]
    public StatList[] StatListsCollection { get; set; }
}