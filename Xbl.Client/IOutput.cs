using Xbl.Models;

namespace Xbl;

public interface IOutput
{
    void RarestAchievements(IEnumerable<RarestAchievementItem> data);
    void MostComplete(IEnumerable<Title> data);
    void SpentMostTimeWith(IEnumerable<MinutesPlayed> data);
}