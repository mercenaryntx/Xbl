using System.Net.Http.Json;
using System.Text.Json;
using Xbl.Client.Infrastructure;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl;
using Xbl.Client.Repositories;
using Xbl.Xbox360.Extensions;

namespace Xbl.Client.Io;

public class XblClient : IXblClient
{
    private readonly Settings _settings;
    private readonly IXblRepository _xbl;
    private readonly IDboxRepository _dbox;
    private readonly IConsole _console;
    private readonly HttpClient _client;

    public XblClient(Settings settings, HttpClient client, IXblRepository xbl, IDboxRepository dbox, IConsole console)
    {
        _settings = settings;
        _client = client;
        _xbl = xbl;
        _dbox = dbox;
        _console = console;
    }

    public async Task<int> Update()
    {
        var update = _settings.Update;
        try
        {
            await _console.Progress(async ctx =>
            {
                var titles = await UpdateTitles(ctx);

                if (update is "all" or "achievements")
                {
                    var src = titles.Titles.Where(t => t.Achievement.CurrentAchievements > 0 && !t.CompatibleDevices.Contains(Device.Mobile));
                    await UpdateAchievements(ctx, src.Where(t => t.OriginalConsole != Device.Xbox360), GetAchievements);
                    await UpdateAchievements(ctx, src.Where(t => t.OriginalConsole == Device.Xbox360), t => Get360Achievements(t, titles.Xuid));
                }
                if (update is "all" or "stats")
                {
                    await GetStatsBulk(ctx, titles);
                }
            });

            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            return _console.ShowError($"[silver]OpenXBL API returned an error [/] [red]({(int?) ex.StatusCode}) {ex.StatusCode}[/]");
        }
    }

    private async Task<AchievementTitles> UpdateTitles(IProgressContext ctx)
    {
        var task = ctx.AddTask("[white]Getting titles[/]", 5);
        var marketplace = await _dbox.GetMarketplaceProducts();
        task.Increment(1);

        var store = await _dbox.GetStoreProducts();
        task.Increment(1);

        var json = await _client.GetStringAsync("achievements/");
        var a = JsonSerializer.Deserialize<AchievementTitles>(json);
        var titles = a.Titles;
        task.Increment(1);

        a.Titles = titles.Select(t => CleanupTitle(t, store, marketplace)).ToArray();
        task.Increment(1);

        await _xbl.SaveTitles(DataSource.Live, a);
        task.Increment(1);

        return a;
    }

    private static Title CleanupTitle(Title title, Dictionary<string, Product> store, Dictionary<string, Product> marketplace)
    {
        title.Source = DataSource.Live;
        title.HexId = ToHexId(title.IntId);

        store.TryGetValue(title.HexId, out var sp);

        if (sp != null)
        {
            title.Category = sp.Category;
            title.Products = sp.Versions.ToDictionary(kvp => kvp.Key, kvp => new TitleProduct
            {
                TitleId = sp.TitleId,
                ProductId = kvp.Value.ProductId
            });
            title.OriginalConsole = title.Products.Keys.OrderBy(k => k).FirstOrDefault(k => k.StartsWith("Xbox"));
            return title;
        }
        
        marketplace.TryGetValue(title.HexId, out var mp);
        if (mp == null) return title;

        title.Category = mp.Category;
        title.IsBackCompat = true;
        title.OriginalConsole = Device.Xbox360;
        title.Products = new Dictionary<string, TitleProduct>
        {
            {
                Device.Xbox360,
                new TitleProduct
                {
                    TitleId = mp.TitleId, 
                    ProductId = mp.Versions[Device.Xbox360].ProductId,
                    ReleaseDate = mp.Versions[Device.Xbox360].ReleaseDate
                }
            }
        };
        var sp2 = store.Values.SingleOrDefault(p => p.Title.StartsWith("[Fission]") && p.Title.EndsWith($"({title.HexId})"));
        if (sp2 != null)
        {
            title.Products.Add("BackCompat", new TitleProduct
            {
                TitleId = sp2.TitleId,
                ProductId = sp2.Versions[Device.Xbox360].ProductId
            });
        }

        return title;
    }

    private async Task UpdateAchievements(IProgressContext ctx, IEnumerable<Title> titles, Func<Title, Task> updateLogic)
    {
        var changes = titles.Where(title => title.TitleHistory.LastTimePlayed > _xbl.GetAchievementSaveDate(title)).ToArray();

        if (changes.Length == 0) return;

        foreach (var title in changes)
        {
            var task = ctx.AddTask($"[white]Updating[/] [cyan1]{title.Name}[/]", 1);
            await updateLogic(title);
            task.Increment(1);
        }
    }

    private async Task GetAchievements(Title title)
    {
        var s = await _client.GetStringAsync("achievements/title/" + title.IntId);
        await _xbl.SaveAchievements(title, s);
    }

    private async Task Get360Achievements(Title title, string xuid)
    {
        var s = await _client.GetStringAsync($"achievements/x360/{xuid}/title/{title.IntId}");
        var a = JsonSerializer.Deserialize<TitleDetails<Achievement>>(s);
        foreach (var achievement in a.Achievements)
        {
            achievement.TitleName = title.Name;
        }
        await _xbl.SaveAchievements(title.Source, title.HexId, a);
    }

    private async Task GetStatsBulk(IProgressContext ctx, AchievementTitles titles)
    {
        var changes = titles.Titles.Where(title =>
        {
            if (title.CompatibleDevices.Contains(Device.Mobile) || title.OriginalConsole == Device.Xbox360) return false;
            return title.TitleHistory.LastTimePlayed > _xbl.GetStatsSaveDate(title);
        }).ToArray();

        var pages = changes.Chunk(100).Select(c => new PlayerStatsRequest
        {
            XUIDs = new[] { titles.Xuid },
            Stats = c.Select(x => new Stat { Name = "MinutesPlayed", TitleId = x.IntId }).ToArray()
        }).ToArray();

        var task = ctx.AddTask("[white]Updating stats[/]", pages.Length);

        foreach (var page in pages)
        {
            var response = await _client.PostAsJsonAsync("player/stats", page);
            var content = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<TitleStats>(content);

            foreach (var stat in stats.StatListsCollection[0].Stats)
            {
                var titleStats = new TitleStats
                {
                    Groups = Array.Empty<TitleStatGroup>(),
                    StatListsCollection = new[]
                    {
                        new StatList
                        {
                            Stats = new[]
                            {
                                stat
                            }
                        }
                    }
                };
                await _xbl.SaveStats(DataSource.Live, ToHexId(stat.TitleId), titleStats);
            }
            task.Increment(1);
        }
    }

    private static string ToHexId(string titleId)
    {
        var id = uint.Parse(titleId);
        var bytes = BitConverter.GetBytes(id);
        bytes.SwapEndian(4);
        return bytes.ToHex();
    }
}