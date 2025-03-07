using Microsoft.Extensions.DependencyInjection;
using Xbl.Client.Extensions;
using Xbl.Client.Models;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Data;

namespace Xbl.Client.Queries;

public class SqliteBuiltInQueries : IBuiltInQueries
{
    private readonly Settings _settings;
    private readonly IDatabaseContext _live;
    private readonly IDatabaseContext _x360;

    public SqliteBuiltInQueries(Settings settings, [FromKeyedServices(DataSource.Live)] IDatabaseContext live, [FromKeyedServices(DataSource.Xbox360)] IDatabaseContext x360)
    {
        _settings = settings;
        _live = live;
        _x360 = x360;
        _settings.Limit = _settings.Limit > 0 ? _settings.Limit : 50;
    }

    public async Task<ProfilesSummary> Count()
    {
        const string summarizeTitles = """
                                       SELECT 
                                         @Name AS Name, 
                                         COUNT(*) AS Games, 
                                         SUM(json_extract(Data, '$.achievement.currentAchievements')) AS Achievements, 
                                         SUM(json_extract(Data, '$.achievement.currentGamerscore')) AS Gamerscore 
                                       FROM title
                                       """;
        const string getIds = "SELECT Id FROM title";

        await _live.EnsureTable("title");
        await _x360.EnsureTable("title");
        var liveSummary = (await _live.Query<ProfileSummary>(summarizeTitles, new { Name = "Xbox Live" })).Single();
        var x360Summary = (await _x360.Query<ProfileSummary>(summarizeTitles, new { Name = "Xbox 360" })).Single();
        var liveTitles = (await _live.Query<int>(getIds)).ToArray();
        var x360Titles = (await _x360.Query<int>(getIds)).ToArray();

        await _live.EnsureTable("stat");
        const string summarizeStats = """
                                      SELECT SUM(json_extract(Data, '$.IntValue')) AS Achievements 
                                      FROM stat 
                                      WHERE json_extract(Data, '$.name') = 'MinutesPlayed'
                                      """;
        var liveStats = (await _live.Query<int>(summarizeStats)).Single();
        liveSummary = liveSummary with {MinutesPlayed = TimeSpan.FromMinutes(liveStats)};

        return new ProfilesSummary([liveSummary, x360Summary], liveTitles.Union(x360Titles).Count());
    }

    public async Task<IEnumerable<RarestAchievementItem>> RarestAchievements()
    {
        await _live.EnsureTable("achievement");

        const string query = """
                             SELECT 
                               json_extract(Data, '$.titleName') AS Title, 
                               json_extract(Data, '$.name') AS Achievement, 
                               json_extract(Data, '$.rarity.currentPercentage') AS Percentage 
                             FROM achievement
                             WHERE json_extract(Data, '$.unlocked') = true 
                             ORDER BY json_extract(Data, '$.rarity.currentPercentage') ASC
                             LIMIT @Limit
                             """;
        return await _live.Query<RarestAchievementItem>(query, _settings);
    }

    public async Task<IEnumerable<CompletenessItem>> MostComplete()
    {
        await _live.EnsureTable("title");

        const string query = """
                             SELECT 
                                json_extract(Data, '$.name') AS Title, 
                                json_extract(Data, '$.achievement.currentGamerscore') AS CurrentGamerscore, 
                                json_extract(Data, '$.achievement.totalGamerscore') AS TotalGamerscore,
                                json_extract(Data, '$.achievement.progressPercentage') AS ProgressPercentage 
                             FROM title
                             LIMIT @Limit
                             """;
        var live = await _live.Query<CompletenessItem>(query, _settings);
        var x360 = await _x360.Query<CompletenessItem>(query, _settings);
        return live.Concat(x360)
            .OrderByDescending(i => i.ProgressPercentage)
            .ThenByDescending(i => i.TotalGamerscore)
            .Take(_settings.Limit);
    }

    public async Task<IEnumerable<MinutesPlayed>> SpentMostTimeWith()
    {
        await _live.EnsureTable("stat");

        const string query = """
                             SELECT 
                               json_extract(title.Data, '$.name') as Key,
                               json_extract(stat.Data, '$.IntValue') AS Value
                             FROM stat
                             JOIN title ON stat.Id = title.Id
                             WHERE json_extract(stat.Data, '$.name') = 'MinutesPlayed'
                             ORDER BY json_extract(stat.Data, '$.IntValue') DESC
                             LIMIT @Limit
                             """;
        var live = await _live.Query<KeyValuePair<string, int>>(query, _settings);
        return live.Select(s => new MinutesPlayed(s.Key, s.Value));
    }

    public async Task<IEnumerable<WeightedAchievementItem>> WeightedRarity()
    {
        var titles = (await _live.GetAll<Title>()).ToDictionary(t => t.Id);
        var achievements = (await _live.GetAll<Achievement>()).GroupBy(a => a.TitleId);

        return achievements.Select(t =>
        {
            var tt = titles[t.Key];
            return new WeightedAchievementItem(
                tt.Name,
                tt.Achievement,
                t.Count(),
                t.Count(a => a.Unlocked),
                t.Count(a => a.Unlocked && a.Rarity.CurrentCategory == "Rare"),
                t.Where(a => a.Unlocked)
                    .Sum(a =>
                    {
                        double score = a.Gamerscore;
                        var weightFactor = Math.Exp((100 - a.Rarity.CurrentPercentage) / 5.0);
                        return score / tt.Achievement.TotalGamerscore * weightFactor;
                    })
            );
        }).OrderByDescending(a => a.Weight).Take(_settings.Limit);
    }

    public async Task<IEnumerable<CategorySlice>> Categories()
    {
        const string query = "SELECT Id AS Key, json_extract(Data, '$.category') AS Value FROM title";
        var live = await _live.Query<KeyValuePair<int, string>>(query, _settings);
        var x360 = await _x360.Query<KeyValuePair<int, string>>(query, _settings);

        return live.Concat(x360)
            .GroupBy(t => t.Key)
            .GroupBy(t => t.FirstOrDefault(x => x.Value != "Other").Value ?? "Other", StringComparer.InvariantCultureIgnoreCase)
            .Select(g => new CategorySlice(g.Key, g.Count()))
                .OrderByDescending(c => c.Count)
            .Take(_settings.Limit);
    }
}