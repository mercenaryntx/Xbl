﻿using System.Net.Http.Json;
using System.Text.Json;
using Spectre.Console;
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
                    var src = titles.Where(t => t.Achievement.CurrentAchievements > 0 && !t.CompatibleDevices.Contains(Device.Mobile));
                    await UpdateAchievements(ctx, src.Where(t => t.OriginalConsole != Device.Xbox360), "Live achievements", GetAchievements);
                    await UpdateAchievements(ctx, src.Where(t => t.OriginalConsole == Device.Xbox360), "X360 achievements", Get360Achievements);
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

    private async Task<Title[]> UpdateTitles(ProgressContext ctx)
    {
        var task = ctx.AddTask("[white]Getting titles[/]", maxValue: 5);
        var marketplace = await _dbox.GetMarketplaceProducts();
        task.Increment(1);
        var store = await _dbox.GetStoreProducts();
        task.Increment(1);
        await GetLiveTitles();
        var titles = await _xbl.LoadTitles(false);
        task.Increment(1);
        titles = titles.Select(t => CleanupTitle(t, store, marketplace)).ToArray();
        task.Increment(1);
        await _xbl.SaveJson(_xbl.GetTitlesFilePath(DataSource.Live), new AchievementTitles { Titles = titles, Xuid = _xbl.Xuid });
        task.Increment(1);
        return titles;
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

    private async Task UpdateAchievements(ProgressContext ctx, IEnumerable<Title> titles, string type, Func<Title, Task> updateLogic)
    {
        var changes = titles.Where(title =>
        {
            var x = new FileInfo(_xbl.GetAchievementFilePath(title));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        if (changes.Length == 0) return;

        foreach (var title in changes)
        {
            var task = ctx.AddTask($"[white]Updating[/] [cyan1]{title}[/]", maxValue: 1);
            await updateLogic(title);
            task.Increment(1);
        }
    }

    public async Task GetLiveTitles()
    {
        var s = await _client.GetStringAsync("achievements/");
        await _xbl.SaveJson(_xbl.GetTitlesFilePath(DataSource.Live), s);
    }

    public async Task GetAchievements(Title title)
    {
        var s = await _client.GetStringAsync("achievements/title/" + title.IntId);
        await _xbl.SaveJson(_xbl.GetAchievementFilePath(title), s);
    }

    public async Task Get360Achievements(Title title)
    {
        var s = await _client.GetStringAsync($"achievements/x360/{_xbl.Xuid}/title/{title.IntId}");
        //TODO: set title name
        await _xbl.SaveJson(_xbl.GetAchievementFilePath(title), s);
    }

    public async Task GetStatsBulk(ProgressContext ctx, IEnumerable<Title> titles)
    {
        var changes = titles.Where(title =>
        {
            if (title.CompatibleDevices.Contains(Device.Mobile) || title.OriginalConsole == Device.Xbox360) return false;
            var x = new FileInfo(_xbl.GetStatsFilePath(title));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        var a = await _xbl.LoadJson<AchievementTitles>(_xbl.GetTitlesFilePath(DataSource.Live));
        var pages = changes.Chunk(100).Select(c => new PlayerStatsRequest
        {
            XUIDs = new[] { a.Xuid },
            Stats = c.Select(x => new Stat { Name = "MinutesPlayed", TitleId = x.IntId }).ToArray()
        }).ToArray();

        var task = ctx.AddTask("[white]Updating stats[/]", maxValue: pages.Length);

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
                await _xbl.SaveJson(_xbl.GetStatsFilePath(DataSource.Live, stat.TitleId), titleStats);
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