using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Marketplace;
using Xbl.Client.Models.Xbl.Player;

namespace Xbl.Client.Io;

public interface IXblApiClient
{
    Task<AchievementTitles> GetTitles();
    Task<LiveAchievement[]> GetAchievements(string titleId);
    Task<Achievement[]> Get360Achievements(string xuid, string titleId);
    Task<XblProduct[]> GetMarketplaceDetails(string titleId);
    Task<TitleStats> GetPlayerStats(PlayerStatsRequest request);
}
