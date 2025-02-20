using AutoMapper;
using Spectre.Console;
using Xbl.Client.Models;
using Xbl.Xbox360.Extensions;

namespace Xbl.Client.Io;

public class JsonRepository : IJsonRepository
{
    private readonly IMapper _mapper;

    public string Xuid { get; private set; }

    public JsonRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    public string GetTitlesFilePath(string env) => Path.Combine(Constants.DataFolder, $"titles.{env}.json");
    public string GetAchievementFilePath(string env, string hexId) => Path.Combine(Constants.DataFolder, env, $"{hexId}\\{Constants.Achievements}.json");
    public string GetAchievementFilePath(Title title) => GetAchievementFilePath(title.IsLive ? Constants.Live : Constants.Xbox360, title.HexId);
    public string GetStatsFilePath(string env, string hexId) => Path.Combine(Constants.DataFolder, env, $"{hexId}\\{Constants.Stats}.json");
    public string GetStatsFilePath(Title title) => GetStatsFilePath(title.IsLive ? Constants.Live : Constants.Xbox360, title.HexId);

    public async Task SaveJson(string path, string json)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<Title[]> LoadTitles(bool union = true)
    {
        var path = GetTitlesFilePath(Constants.Live);
        if (!File.Exists(path))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] [silver]Data files cannot be found. Please run an update first[/]");
            return Array.Empty<Title>();
        }

        var a = await JsonHelper.FromFile<AchievementTitles>(path);
        Xuid = a.Xuid;
        IEnumerable<Title> titles = a.Titles.Select(t =>
        {
            t.IsLive = true;
            t.IsXbox360 = t.Devices.Contains("Xbox360");
            t.IsMobile = t.Devices.Contains("Mobile");
            t.HexId = ToHexId(t.TitleId);
            return t;
        }).OrderByDescending(t => t.Achievement.ProgressPercentage);

        path = GetTitlesFilePath(Constants.Xbox360);
        if (union && File.Exists(path))
        {
            a = await JsonHelper.FromFile<AchievementTitles>(path);
            titles = titles.Union(a.Titles);
        }

        return titles.ToArray();
    }

    public async Task<Achievement[]> LoadAchievements(Title title)
    {
        var path = GetAchievementFilePath(title);
        if (!File.Exists(path)) return Array.Empty<Achievement>();

        if (title.IsXbox360)
        {
            var details360 = await JsonHelper.FromFile<TitleDetails<Achievement>>(path);
            foreach (var a in details360.Achievements)
            {
                a.TitleName = title.Name;
            }
            return details360.Achievements;
        }

        var details = await JsonHelper.FromFile<TitleDetails<LiveAchievement>>(path);
        return _mapper.Map<Achievement[]>(details.Achievements);
    }

    public async Task<Stat[]> LoadStats(Title title)
    {
        var path = GetStatsFilePath(title);
        if (!File.Exists(path))
        {
            return Array.Empty<Stat>();
        }

        var details = await JsonHelper.FromFile<TitleStats>(path);
        return details.StatListsCollection.Length == 0 ? Array.Empty<Stat>() : details.StatListsCollection[0].Stats;
    }

    private static string ToHexId(string titleId)
    {
        var id = uint.Parse(titleId);
        var bytes = BitConverter.GetBytes(id);
        bytes.SwapEndian(4);
        return bytes.ToHex();
    }

}