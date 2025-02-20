using Xbl.Client.Models;

namespace Xbl.Client.Io;

public interface IJsonRepository
{
    public string Xuid { get; }

    string GetTitlesFilePath(string env);
    string GetAchievementFilePath(string env, string hexId);
    string GetAchievementFilePath(Title title);
    string GetStatsFilePath(string env, string hexId);
    string GetStatsFilePath(Title title);

    Task SaveJson(string path, string json);

    Task<Title[]> LoadTitles(bool union = true);
    Task<Achievement[]> LoadAchievements(Title title);
    Task<Stat[]> LoadStats(Title title);
}