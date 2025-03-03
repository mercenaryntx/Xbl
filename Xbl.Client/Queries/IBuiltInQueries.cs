using Xbl.Client.Models;

namespace Xbl.Client.Queries;

public interface IBuiltInQueries
{
    Task<ProfilesSummary> Count();
    Task<IEnumerable<RarestAchievementItem>> RarestAchievements();
    Task<IEnumerable<CompletenessItem>> MostComplete();
    Task<IEnumerable<MinutesPlayed>> SpentMostTimeWith();
    Task<IEnumerable<WeightedAchievementItem>> WeightedRarity();
    Task<IEnumerable<CategorySlice>> Categories();
}