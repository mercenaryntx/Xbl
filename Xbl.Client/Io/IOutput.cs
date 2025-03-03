using KustoLoco.Core;
using Xbl.Client.Models;

namespace Xbl.Client.Io;

public interface IOutput
{
    void Render(ProfilesSummary summary);
    void Render(IEnumerable<RarestAchievementItem> data);
    void Render(IEnumerable<WeightedAchievementItem> weightedRarity);
    void Render(IEnumerable<CompletenessItem> data);
    void Render(IEnumerable<MinutesPlayed> data);
    void Render(IEnumerable<CategorySlice> slices);

    void KustoQueryResult(KustoQueryResult result);
}