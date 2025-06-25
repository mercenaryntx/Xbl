using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xbl.Client.Extensions;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Data;
using Xbl.Xbox360.Extensions;
using Xbl.Xbox360.Io.Gpd;
using Xbl.Xbox360.Io.Stfs;
using Xbl.Xbox360.Models;

namespace Xbl.Client.Io;

public class Xbox360ProfileImporter : IXbox360ProfileImporter
{
    private readonly Settings _settings;
    private readonly IDatabaseContext _dbox;
    private readonly IDatabaseContext _x360;
    private readonly IConsole _console;

    public Xbox360ProfileImporter(Settings settings, [FromKeyedServices(DataSource.Dbox)] IDatabaseContext dbox, [FromKeyedServices(DataSource.Xbox360)] IDatabaseContext x360, IConsole console)
    {
        _settings = settings;
        _dbox = dbox.Mandatory();
        _x360 = x360.Mandatory(SqliteOpenMode.ReadWriteCreate);
        _console = console;
    }

    public async Task<int> Import()
    {
        var bugs = new List<string>();
		try
        {
            await _console.Progress(async ctx =>
            {
                var task = ctx.AddTask("[white]Importing X360 profile[/]", maxValue: 4);
                var marketplace = (await _dbox.GetAll<Product>(DataTable.Marketplace)).ToDictionary(m => m.TitleId);
                task.Increment(1);

                var profilePath = _settings.ProfilePath;
                var bytes = await File.ReadAllBytesAsync(profilePath);
                var profile = ModelFactory.GetModel<StfsPackage>(bytes);
                task.Increment(1);

                profile.ExtractGames();
                task.Increment(1);

                var titles = GetTitlesFromProfile(profile, marketplace);
                var titlesRep = await _x360.GetRepository<Title>();
                await titlesRep.BulkInsert(titles);
                task.Increment(1);

                var task2 = ctx.AddTask("[white]Importing achievements[/]", maxValue: profile.Games.Count);
                var achievementRep = await _x360.GetRepository<Achievement>();

                foreach (var (fileEntry, game) in profile.Games)
                {
                    game.Parse();
                    var achievements = GetAchievementsFromGameFile(game, out var hadBug);
                    if (hadBug) bugs.Add($"[#f9fba5]Warning:[/] {game.Title} ([grey]{fileEntry.Name}[/]) is corrupted. Invalid entries were omitted.");

                    await achievementRep.BulkInsert(achievements);
                    task2.Increment(1);
                }
            });
            foreach (var bug in bugs)
            {
                _console.MarkupLine(bug);
            }
            if (bugs.Count > 0) _console.MarkupLine("");
            return 0;
        }
        catch (Exception ex)
        {
            return _console.ShowError($"[silver]Failed to import. {ex.Message}[/]");
        }
    }

    private static IEnumerable<Title> GetTitlesFromProfile(StfsPackage profile, Dictionary<string, Product> marketplace)
    {
        return profile
            .ProfileInfo
            .TitlesPlayed
            .Where(g => !string.IsNullOrEmpty(g.TitleName))
            .Select(g =>
            {
                marketplace.TryGetValue(g.TitleCode, out var mp);
                var product = mp?.Versions[Device.Xbox360];

				var t = new Title
                {
                    IntId = BitConverter.ToInt32(g.TitleCode.FromHex()).ToString(),
                    HexId = g.TitleCode,
                    Name = g.TitleName,
                    Type = "Game",
                    OriginalConsole = Device.Xbox360,
                    CompatibleDevices = [Device.Xbox360],
                    Source = DataSource.Xbox360,
                    Products = new Dictionary<string, TitleProduct> {
                        { Device.Xbox360, new TitleProduct {
                            TitleId = g.TitleCode,
                            ProductId = product?.ProductId,
                            ReleaseDate = product?.ReleaseDate
						} }
                    }, 
                    Category = mp?.Category,
					Achievement = new AchievementSummary
                    {
                        CurrentAchievements = g.AchievementsUnlocked,
                        CurrentGamerscore = g.GamerscoreUnlocked,
                        ProgressPercentage = g.TotalGamerScore > 0 ? 100 * g.GamerscoreUnlocked / g.TotalGamerScore : 0,
                        TotalGamerscore = g.TotalGamerScore,
                        TotalAchievements = g.AchievementCount
                    },
                    TitleHistory = new TitleHistory
                    {
                        LastTimePlayed = g.LastPlayed,
                    },
                    DisplayImage = $"{g.TitleCode}.png"
                };

                return t;
            });
    }

    private static IEnumerable<Achievement> GetAchievementsFromGameFile(GameFile game, out bool hadBug)
    {
        var bug = false;
        var achievements = game.Achievements.Select(a =>
        {
            try
            {
                if (a.Gamerscore < 0 || a.AchievementId < 0) throw new Exception();

                return new Achievement
                {
                    Id = a.AchievementId,
                    Name = a.Name,
                    TitleId = BitConverter.ToInt32(game.TitleId.FromHex()),
                    TitleName = game.Title,
                    Unlocked = a.IsUnlocked,
                    TimeUnlocked = a.IsUnlocked ? a.UnlockTime : DateTime.MinValue,
                    Platform = "Xbox360",
                    IsSecret = a.IsSecret,
                    Description = a.UnlockedDescription,
                    LockedDescription = a.LockedDescription,
                    Gamerscore = a.Gamerscore,
                    DisplayImage = $"{game.TitleId}.{a.ImageId}.png"
                };
            }
            catch
            {
                bug = true;
                return null;
            }
        }).Where(a => a != null);

        foreach (var imageEntry in game.Images)
        {
            File.WriteAllBytes(Path.Combine(DataSource.DataFolder, "img", $"{game.TitleId}.{imageEntry.Entry.Id}.png"), imageEntry.ImageData);
        }

        hadBug = bug;
        return achievements;
    }

}