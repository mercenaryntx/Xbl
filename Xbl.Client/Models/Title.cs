﻿using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class Title
{
    [JsonPropertyName("titleId")]
    public string TitleId { get; set; }
    [JsonPropertyName("pfn")]
    public string Pfn { get; set; }
    [JsonPropertyName("bingId")]
    public string BingId { get; set; }
    [JsonPropertyName("serviceConfigId")]
    public string ServiceConfigId { get; set; }
    [JsonPropertyName("windowsPhoneProductId")]
    public object WindowsPhoneProductId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("devices")]
    public string[] Devices { get; set; }
    [JsonPropertyName("displayImage")]
    public string DisplayImage { get; set; }
    [JsonPropertyName("mediaItemType")]
    public string MediaItemType { get; set; }
    [JsonPropertyName("modernTitleId")]
    public string ModernTitleId { get; set; }
    [JsonPropertyName("isBundle")]
    public bool IsBundle { get; set; }
    [JsonPropertyName("achievement")]
    public AchievementSummary Achievement { get; set; }
    [JsonPropertyName("stats")]
    public object Stats { get; set; }
    [JsonPropertyName("gamePass")]
    public object GamePass { get; set; }
    [JsonPropertyName("images")]
    public Image[] Images { get; set; }
    [JsonPropertyName("titleHistory")]
    public TitleHistory TitleHistory { get; set; }
    [JsonPropertyName("titleRecord")]
    public object TitleRecord { get; set; }
    [JsonPropertyName("detail")]
    public object Detail { get; set; }
    [JsonPropertyName("friendsWhoPlayed")]
    public object FriendsWhoPlayed { get; set; }
    [JsonPropertyName("alternateTitleIds")]
    public object AlternateTitleIds { get; set; }
    [JsonPropertyName("contentBoards")]
    public object ContentBoards { get; set; }
    [JsonPropertyName("xboxLiveTier")]
    public string XboxLiveTier { get; set; }
    [JsonPropertyName("isStreamable")]
    public bool IsStreamable { get; set; }

    public bool IsLive { get; set; }
    public bool IsXbox360 { get; set; }
    public bool IsMobile { get; set; }
    public string HexId { get; set; }
}