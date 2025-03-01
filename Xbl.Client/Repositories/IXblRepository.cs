using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;

namespace Xbl.Client.Repositories;

public interface IXblRepository : IRepository
{
    Task SaveTitles(string source, AchievementTitles titles);
    Task SaveAchievements(Title title, string achievements);
    Task SaveAchievements(string source, string hexId, TitleDetails<Achievement> achievements);
    Task SaveStats(string source, string hexId, TitleStats stats);

    DateTime GetAchievementSaveDate(Title title);
    DateTime GetStatsSaveDate(Title title);

    //string GetTitlesFilePath(string env);
    //string GetAchievementFilePath(string env, string hexId);
    //string GetAchievementFilePath(Title title);
    //string GetStatsFilePath(string env, string hexId);
    //string GetStatsFilePath(Title title);

    Task<Title[]> LoadTitles(bool union = true);
    Task<Achievement[]> LoadAchievements(Title title);
    Task<Stat[]> LoadStats(Title title);
}