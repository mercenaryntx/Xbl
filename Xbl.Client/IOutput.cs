using KustoLoco.Core;
using Xbl.Client.Models;

namespace Xbl.Client;

public interface IOutput
{
    void RarestAchievements(IEnumerable<RarestAchievementItem> data);
    void WeightedRarity(IEnumerable<WeightedAchievementItem> weightedRarity);
    void MostComplete(IEnumerable<Title> data);
    void SpentMostTimeWith(IEnumerable<MinutesPlayed> data);
    void KustoQueryResult(KustoQueryResult result);
}