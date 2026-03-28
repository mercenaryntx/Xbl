using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Marketplace;
using Xbl.Client.Models.Xbl.Player;

namespace Xbl.Client.Io;

public class XblApiClientV2(HttpClient client) : IXblApiClient
{
    public async Task<AchievementTitles> GetTitles()
    {
        var json = await client.GetStringAsync("v2/achievements/");
        return JsonSerializer.Deserialize<ContentResponse<AchievementTitles>>(json).Content;
    }

    public async Task<LiveAchievement[]> GetAchievements(string titleId)
    {
        var json = await client.GetStringAsync("v2/achievements/title/" + titleId);
        return JsonSerializer.Deserialize<ContentResponse<TitleDetails<LiveAchievement>>>(json).Content.Achievements;
    }

    public async Task<Achievement[]> Get360Achievements(string xuid, string titleId)
    {
        var json = await client.GetStringAsync($"v2/achievements/player/{xuid}/title/{titleId}");
        return JsonSerializer.Deserialize<ContentResponse<TitleDetails<Achievement>>>(json).Content.Achievements;
    }

    public async Task<XblProduct[]> GetMarketplaceDetails(string titleId)
    {
        var json = await client.GetStringAsync("v2/marketplace/title/" + titleId);
        return JsonSerializer.Deserialize<ContentResponse<XblProductCollection>>(json).Content.Products;
    }

    public async Task<TitleStats> GetPlayerStats(PlayerStatsRequest request)
    {
        var response = await client.PostAsJsonAsync("v2/player/stats", request, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ContentResponse<TitleStats>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Content;
    }
}
