using Xbl.Client.Models;
using Xbl.Client.Repositories;

namespace Xbl.Client.Queries;

public class JsonFilesBuiltInQueries : IBuiltInQueries
{
    private readonly Settings _settings;
    private readonly IXblRepository _repository;

    public JsonFilesBuiltInQueries(Settings settings, IXblRepository repository)
    {
        _settings = settings;
        _repository = repository;
        _settings.Limit = _settings.Limit > 0 ? _settings.Limit : 50;
    }

    public async Task<ProfilesSummary> Count()
    {
        var titles = await _repository.LoadTitles();
        var played = titles.Sum(t => _repository.LoadStats(t).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0);
        var uniqueTitlesCount = titles.GroupBy(t => t.Name).Count();
        var groups = titles.GroupBy(t => t.Source).Select(g =>
        {
            return new ProfileSummary(g.Key, g.Count(), g.Sum(t => t.Achievement?.CurrentAchievements ?? 0), g.Sum(t => t.Achievement?.CurrentGamerscore ?? 0), g.Key == DataSource.Live ? TimeSpan.FromMinutes(played) : TimeSpan.Zero);
        }).ToArray();

        return new ProfilesSummary(groups, uniqueTitlesCount);
    }

    public async Task<IEnumerable<RarestAchievementItem>> RarestAchievements()
    {
        var titles = await _repository.LoadTitles(false);
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => _repository.LoadAchievements(t).GetAwaiter().GetResult());

        return data.SelectMany(kvp => kvp.Value.Where(a => a.Unlocked).Select(a => new RarestAchievementItem(
            kvp.Key.Name,
            a.Name,
            a.Rarity.CurrentPercentage
        ))).OrderBy(a => a.Percentage).Take(_settings.Limit);
    }

    public async Task<IEnumerable<CompletenessItem>> MostComplete()
    {
        var titles = await _repository.LoadTitles();
        return titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            .Select(t => new CompletenessItem(t.Name, t.Achievement?.CurrentGamerscore ?? 0, t.Achievement?.TotalGamerscore ?? 0, t.Achievement?.ProgressPercentage ?? 0))
            .Take(_settings.Limit);
    }

    public async Task<IEnumerable<MinutesPlayed>> SpentMostTimeWith()
    {
        var titles = await _repository.LoadTitles(false);

        return titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            .ToDictionary(t => t, t => _repository.LoadStats(t).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0)
            .OrderByDescending(t => t.Value)
            .Take(_settings.Limit)
            .Select(kvp => new MinutesPlayed(kvp.Key.Name, kvp.Value));
    }

    public async Task<IEnumerable<WeightedAchievementItem>> WeightedRarity()
    {
        var titles = await _repository.LoadTitles(false);
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => _repository.LoadAchievements(t).GetAwaiter().GetResult());

        return data.Select(t => new WeightedAchievementItem(
            t.Key.Name,
            t.Key.Achievement,
            t.Value.Length,
            t.Value.Count(a => a.Unlocked),
            t.Value.Count(a => a.Unlocked && a.Rarity.CurrentCategory == "Rare"),
            t.Value.Where(a => a.Unlocked)
                .Sum(a =>
                {
                    double score = a.Gamerscore;
                    var weightFactor = Math.Exp((100 - a.Rarity.CurrentPercentage) / 5.0);
                    return score / t.Key.Achievement.TotalGamerscore * weightFactor;
                })
        )).OrderByDescending(a => a.Weight).Take(_settings.Limit);
    }

    public async Task<IEnumerable<CategorySlice>> Categories()
    {
        var titles = await _repository.LoadTitles();

        return titles
            .GroupBy(t => t.Category ?? "Other", StringComparer.InvariantCultureIgnoreCase)
            .Select(g => new CategorySlice(g.Key, g.Count()))
            .OrderByDescending(c => c.Count);
    }
}