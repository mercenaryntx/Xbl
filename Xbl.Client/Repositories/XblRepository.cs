using AutoMapper;
using Spectre.Console;
using Xbl.Client.Models.Xbl;

namespace Xbl.Client.Repositories;

public class XblRepository : RepositoryBase, IXblRepository
{
    private readonly IMapper _mapper;

    public string Xuid { get; private set; }

    public XblRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    public string GetTitlesFilePath(string env) => Path.Combine(DataSource.DataFolder, $"titles.{env}.json");
    public string GetAchievementFilePath(string env, string hexId) => Path.Combine(DataSource.DataFolder, env, $"{hexId}\\{DataTable.Achievements}.json");
    public string GetAchievementFilePath(Title title) => GetAchievementFilePath(title.Source, title.HexId);
    public string GetStatsFilePath(string env, string hexId) => Path.Combine(DataSource.DataFolder, env, $"{hexId}\\{DataTable.Stats}.json");
    public string GetStatsFilePath(Title title) => GetStatsFilePath(title.Source, title.HexId);
    
    public async Task<Title[]> LoadTitles(bool union = true)
    {
        var path = GetTitlesFilePath(DataSource.Live);
        if (!File.Exists(path))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] [silver]Data files cannot be found. Please run an update first[/]");
            return Array.Empty<Title>();
        }

        var a = await LoadJson<AchievementTitles>(path);
        Xuid = a.Xuid;
        IEnumerable<Title> titles = a.Titles.OrderByDescending(t => t.Achievement.ProgressPercentage);

        path = GetTitlesFilePath(DataSource.Xbox360);
        if (union && File.Exists(path))
        {
            a = await LoadJson<AchievementTitles>(path);
            titles = titles.Union(a.Titles);
        }

        return titles.ToArray();
    }

    public async Task<Achievement[]> LoadAchievements(Title title)
    {
        var path = GetAchievementFilePath(title);
        if (!File.Exists(path)) return Array.Empty<Achievement>();

        if (title.OriginalConsole == Device.Xbox360)
        {
            var details360 = await LoadJson<TitleDetails<Achievement>>(path);
            foreach (var a in details360.Achievements)
            {
                a.TitleName = title.Name;
            }
            return details360.Achievements;
        }

        var details = await LoadJson<TitleDetails<LiveAchievement>>(path);
        return _mapper.Map<Achievement[]>(details.Achievements);
    }

    public async Task<Stat[]> LoadStats(Title title)
    {
        var path = GetStatsFilePath(title);
        if (!File.Exists(path))
        {
            return Array.Empty<Stat>();
        }

        var details = await LoadJson<TitleStats>(path);
        return details.StatListsCollection.Length == 0 ? Array.Empty<Stat>() : details.StatListsCollection[0].Stats;
    }
}