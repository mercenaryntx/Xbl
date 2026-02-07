using System.Net.Http.Json;
using System.Text.Json;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xbl.Client.Extensions;
using Xbl.Client.Infrastructure;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Data;
using Xbl.Data.Entities;
using Xbl.Data.Repositories;

namespace Xbl.Client.Io;

public class XblClient : IXblClient
{
    private readonly Settings _settings;
    private readonly IConsole _console;
    private readonly IMapper _mapper;
    private readonly IDatabaseContext _live;
    private readonly IDatabaseContext _dbox;
    private readonly HttpClient _client;

    public XblClient(
        Settings settings, 
        HttpClient client, 
        IConsole console, 
        IMapper mapper, 
        [FromKeyedServices(DataSource.Live)] IDatabaseContext live, 
        [FromKeyedServices(DataSource.Dbox)] IDatabaseContext dbox)
    {
        _settings = settings;
        _client = client;
        _console = console;
        _mapper = mapper;
        _live = live.Mandatory(SqliteOpenMode.ReadWriteCreate);
        _dbox = dbox;
    }

    public async Task<UpdateResult> Update()
    {
        var update = _settings.Update;
        var result = new UpdateResult();
        
        try
        {
            await _console.Progress(async ctx =>
            {
                if (!_dbox.IsExists) await DownloadLatestDboxDb(ctx);
                _dbox.Mandatory();
                var titlesResult = await UpdateTitles(ctx);
                result = result with 
                { 
                    TitlesInserted = titlesResult.Inserted, 
                    TitlesUpdated = titlesResult.Updated 
                };

                if (update is "all" or "achievements")
                {
                    var ar = await _live.GetRepository<Achievement>();
                    var headers = (await ar.GetHeaders()).Cast<IntKeyedJsonEntity>().GroupBy(t => t.PartitionKey).ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Id));

                    var src = titlesResult.Titles.Titles
                        .Where(t => t.Achievement.CurrentAchievements > 0 && !t.CompatibleDevices.Contains(Device.Mobile))
                        .GroupBy(t => t.OriginalConsole == Device.Xbox360);
                    foreach (var grouping in src)
                    {
                        var achievementsResult = grouping.Key switch
                        {
                            true => await UpdateAchievements(ctx, grouping, ar, headers, t => Get360Achievements(t, titlesResult.Xuid)),
                            false => await UpdateAchievements(ctx, grouping, ar, headers, GetAchievements)
                        };
                        result = result with
                        {
                            AchievementsInserted = result.AchievementsInserted + achievementsResult.Inserted,
                            AchievementsUpdated = result.AchievementsUpdated + achievementsResult.Updated
                        };
                    }
                }
                if (update is "all" or "stats")
                {
                    var statsResult = await UpdateStats(ctx, titlesResult);
                    result = result with
                    {
                        StatsInserted = statsResult.Inserted,
                        StatsUpdated = statsResult.Updated
                    };
                }
            });

            return result;
        }
        catch (HttpRequestException ex)
        {
            _console.MarkupLine(string.Empty);
            _console.ShowError($"[silver]OpenXBL API returned an error [/] [red]({(int?) ex.StatusCode}) {ex.StatusCode}[/]");
            throw;
        }
    }

    private async Task DownloadLatestDboxDb(IProgressContext ctx)
    {
        var task = ctx.AddTask("[white]Getting store data[/]", 100);
        var response = await _client.GetAsync(new Uri("http://www.mercenary.hu/xbl/dbox.db"), HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1;

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var filePath = Path.Combine(DataSource.DataFolder, "dbox.db");
        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var buffer = new byte[8192];
            long totalReadBytes = 0;
            int readBytes;

            while ((readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, readBytes);
                if (canReportProgress)
                {
                    totalReadBytes += readBytes;
                    var progress = (double)totalReadBytes / totalBytes * 100;
                    task.Value = progress;
                }
            }
        }
        task.Increment(100 - task.Value); // Ensure the task is marked as complete
    }

    private async Task<(AchievementTitles Titles, string Xuid, int Inserted, int Updated)> UpdateTitles(IProgressContext ctx)
    {
        var marketplaceRepository = await _dbox.GetRepository<Product>(DataTable.Marketplace);
        var storeRepository = await _dbox.GetRepository<Product>(DataTable.Store);

        var marketplace = (await marketplaceRepository.GetAll()).ToDictionary(m => m.TitleId);
        var store = (await storeRepository.GetAll()).ToDictionary(m => m.TitleId);

        var task = ctx.AddTask("[white]Getting titles[/]", 4);
        var json = await _client.GetStringAsync("achievements/");
        var a = JsonSerializer.Deserialize<AchievementTitles>(json);
        task.Increment(1);

        var titlesRepository = await _live.GetRepository<Title>();
        var headers = (await titlesRepository.GetHeaders()).Cast<IntKeyedJsonEntity>().ToDictionary(m => m.Id);
        task.Increment(1);

        var insert = a.Titles
            .Where(t => !headers.ContainsKey(t.Id))
            .Select(t => EnrichTitle(t, store, marketplace))
            .ToArray();
        await titlesRepository.BulkInsert(insert);
        task.Increment(1);

        var update = a.Titles
            .Where(t => headers.TryGetValue(t.Id, out var header) && header.UpdatedOn < t.TitleHistory.LastTimePlayed)
            .Select(t => EnrichTitle(t, store, marketplace))
            .ToArray();
        await titlesRepository.BulkUpdate(update);
        task.Increment(1);

        a.Titles = insert.Concat(update).ToArray();
        if (a.Titles.Any(t => t.Products == null)) _console.ShowError("Some titles are missing from the Store or Marketplace data.");
        return (a, a.Xuid, insert.Length, update.Length);
    }

    private static Title EnrichTitle(Title title, Dictionary<string, Product> store, Dictionary<string, Product> marketplace)
    {
        title.Source = DataSource.Live;
        title.HexId = title.IntId.ToHexId();

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
            title.IsBackCompat = true;
            title.Products.Add("BackCompat", new TitleProduct
            {
                TitleId = sp2.TitleId,
                ProductId = sp2.Versions[Device.Xbox360].ProductId
            });
        }

        return title;
    }

    private static async Task<(int Inserted, int Updated)> UpdateAchievements(IProgressContext ctx, IEnumerable<Title> titles, IRepository<Achievement> ar, Dictionary<int, Dictionary<int, IntKeyedJsonEntity>> headers, Func<Title, Task<Achievement[]>> updateLogic)
    {
        int totalInserted = 0;
        int totalUpdated = 0;
        
        foreach (var title in titles)
        {
            var task = ctx.AddTask($"[white]Updating[/] [cyan1]{title.Name}[/]", 2);
            var a = await updateLogic(title);

            if (!headers.TryGetValue(title.Id, out var achievements))
            {
                await ar.BulkInsert(a);
                totalInserted += a.Length;
                task.Increment(2);
                continue;
            }

            var toInsert = a.Where(t => !achievements.ContainsKey(t.Id)).ToArray();
            await ar.BulkInsert(toInsert);
            totalInserted += toInsert.Length;
            task.Increment(1);

            var toUpdate = a.Where(t => achievements.ContainsKey(t.Id)).ToArray();
            await ar.BulkUpdate(toUpdate);
            totalUpdated += toUpdate.Length;
            task.Increment(1);
        }
        
        return (totalInserted, totalUpdated);
    }

    private async Task<Achievement[]> GetAchievements(Title title)
    {
        var s = await _client.GetStringAsync("achievements/title/" + title.IntId);
        return JsonSerializer.Deserialize<TitleDetails<LiveAchievement>>(s).Achievements.Select(_mapper.Map<Achievement>).ToArray();
    }

    private async Task<Achievement[]> Get360Achievements(Title title, string xuid)
    {
        var s = await _client.GetStringAsync($"achievements/x360/{xuid}/title/{title.IntId}");
        var a = JsonSerializer.Deserialize<TitleDetails<Achievement>>(s);
        foreach (var achievement in a.Achievements)
        {
            achievement.TitleName = title.Name;
        }

        return a.Achievements;
    }

    private async Task<(int Inserted, int Updated)> UpdateStats(IProgressContext ctx, (AchievementTitles Titles, string Xuid, int Inserted, int Updated) titlesResult)
    {
        var statRepository = await _live.GetRepository<Stat>();
        var headers = (await statRepository.GetHeaders()).Cast<IntKeyedJsonEntity>().ToDictionary(m => m.Id);

        var changes = titlesResult.Titles.Titles.Where(title => !title.CompatibleDevices.Contains(Device.Mobile) && title.OriginalConsole != Device.Xbox360).ToArray();

        var pages = changes.Chunk(100).Select(c => new PlayerStatsRequest
        {
            XUIDs = [titlesResult.Xuid],
            Stats = c.Select(x => new Stat { Name = "MinutesPlayed", TitleId = x.IntId }).ToArray()
        }).ToArray();

        var task = ctx.AddTask("[white]Updating stats[/]", pages.Length + 1);

        int totalInserted = 0;
        int totalUpdated = 0;
        
        foreach (var page in pages)
        {
            var response = await _client.PostAsJsonAsync("player/stats", page);
            var content = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<TitleStats>(content);

            var toInsert = stats.StatListsCollection[0].Stats.Where(t => !headers.ContainsKey(t.Id)).ToArray();
            await statRepository.BulkInsert(toInsert);
            totalInserted += toInsert.Length;

            var toUpdate = stats.StatListsCollection[0].Stats.Where(t => headers.ContainsKey(t.Id)).ToArray();
            await statRepository.BulkUpdate(toUpdate);
            totalUpdated += toUpdate.Length;
            task.Increment(1);
        }
        task.Increment(1);
        
        return (totalInserted, totalUpdated);
    }
}