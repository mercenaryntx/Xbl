using Xbl.Client.Models;

namespace Xbl.Client;

public interface IOutput
{
    void RarestAchievements(IEnumerable<Records> data);
    void WeightedRarity(IEnumerable<WeightedAchievementItem> weightedRarity);
    void MostComplete(IEnumerable<Title> data);
    void SpentMostTimeWith(IEnumerable<MinutesPlayed> data);
}