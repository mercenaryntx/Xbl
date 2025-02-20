using System.Net.Http.Json;
using System.Text.Json;
using Xbl.Client.Models;

namespace Xbl.Client.Io;

public class XblClient : IXblClient
{
    private readonly Settings _settings;
    private readonly IJsonRepository _repository;
    private readonly IConsole _console;
    private readonly HttpClient _client = new();

    public XblClient(Settings settings, IJsonRepository repository, IConsole console)
    {
        _settings = settings;
        _repository = repository;
        _console = console;
        _client.DefaultRequestHeaders.Add("x-authorization", settings.ApiKey);
        _client.BaseAddress = new Uri("https://xbl.io/api/v2/");
    }

    public async Task<int> Update()
    {
        var update = _settings.Update;
        try
        {
            _console.Markup("[white]Getting titles... [/]");
            await GetLiveTitles();
            var titles = await _repository.LoadTitles(false);
            _console.MarkupLineInterpolated($"[#16c60c]OK[/] [white][[{titles.Length}]][/]");

            if (update is "all" or "achievements")
            {
                var src = titles.Where(t => t.Achievement.CurrentAchievements > 0 && !t.IsMobile);
                var x360 = src.Where(t => t.IsXbox360);
                var rest = src.Where(t => !t.IsXbox360);
                await UpdateLiveTitles(rest, "Live achievements", GetAchievements);
                await UpdateLiveTitles(x360, "X360 achievements", Get360Achievements);
            }
            if (update is "all" or "stats")
            {
                await GetStatsBulk(titles);
            }

            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            return _console.ShowError($"[silver]OpenXBL API returned an error [/] [red]({(int?) ex.StatusCode}) {ex.StatusCode}[/]");
        }
    }

    private async Task UpdateLiveTitles(IEnumerable<Title> titles, string type, Func<Title, Task> updateLogic)
    {
        var changes = titles.Where(title =>
        {
            var x = new FileInfo(_repository.GetAchievementFilePath(title));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        _console.MarkupLineInterpolated($"[white]Found[/] [cyan1]{changes.Length}[/] [white]{type} changes[/]");

        for (var i = 0; i < changes.Length; i++)
        {
            var title = changes[i];
            _console.MarkupInterpolated($"[silver][[{i + 1:D3}/{changes.Length:D3}]][/] [white]Updating [/] [cyan1]{title.Name}[/][white]... [/]");
            await updateLogic(title);
            _console.MarkupLine("[#16c60c]OK[/]");
        }
    }

    public async Task GetLiveTitles()
    {
        var s = await _client.GetStringAsync("achievements/");
        await _repository.SaveJson(_repository.GetTitlesFilePath(Constants.Live), s);
    }

    public async Task GetAchievements(Title title)
    {
        var s = await _client.GetStringAsync("achievements/title/" + title.TitleId);
        await _repository.SaveJson(_repository.GetAchievementFilePath(title), s);
    }

    public async Task Get360Achievements(Title title)
    {
        var s = await _client.GetStringAsync($"achievements/x360/{_repository.Xuid}/title/{title.TitleId}");
        await _repository.SaveJson(_repository.GetAchievementFilePath(title), s);
    }

    public async Task GetStatsBulk(IEnumerable<Title> titles)
    {
        var changes = titles.Where(title =>
        {
            if (title.IsMobile || title.IsXbox360) return false;
            var x = new FileInfo(_repository.GetStatsFilePath(title));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        var a = await JsonHelper.FromFile<AchievementTitles>(_repository.GetTitlesFilePath(Constants.Live));
        var pages = changes.Chunk(100).Select(c => new PlayerStatsRequest
        {
            XUIDs = new[] { a.Xuid },
            Stats = c.Select(x => new Stat { Name = "MinutesPlayed", TitleId = x.TitleId }).ToArray()
        }).ToArray();

        _console.MarkupInterpolated($"[white]Updating stats...[/] ");
        var cursor = Console.GetCursorPosition();
        _console.Markup("[#f9f1a5]0%[/]");

        var i = 0;
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
                var json = JsonSerializer.Serialize(titleStats);
                await _repository.SaveJson(_repository.GetStatsFilePath(Constants.Live, stat.TitleId), json);
            }
            Console.SetCursorPosition(cursor.Left, cursor.Top);
            _console.MarkupInterpolated($"[#f9f1a5]{++i*100/pages.Length}%[/]");
        }
    }
}