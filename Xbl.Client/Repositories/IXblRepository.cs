using Xbl.Client.Models.Xbl;

namespace Xbl.Client.Repositories;

public interface IXblRepository : IRepository
{
    public string Xuid { get; }

    string GetTitlesFilePath(string env);
    string GetAchievementFilePath(string env, string hexId);
    string GetAchievementFilePath(Title title);
    string GetStatsFilePath(string env, string hexId);
    string GetStatsFilePath(Title title);

    Task<Title[]> LoadTitles(bool union = true);
    Task<Achievement[]> LoadAchievements(Title title);
    Task<Stat[]> LoadStats(Title title);
}