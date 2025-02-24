using System.Collections.Immutable;
using AutoMapper;
using KustoLoco.Core;
using Xbl.Client.Io;
using Xbl.Client.Models.Kql;
using Xbl.Client.Repositories;

namespace Xbl.Client.Queries;

public class KustoQueryExecutor : IKustoQueryExecutor
{
    private readonly Settings _settings;
    private readonly IXblRepository _xbl;
    private readonly IConsole _console;
    private readonly IMapper _mapper;
    private readonly IOutput _output;

    public KustoQueryExecutor(Settings settings, IXblRepository xbl, IConsole console, IMapper mapper)
    {
        _settings = settings;
        _xbl = xbl;
        _console = console;
        _mapper = mapper;
        _output = settings.Output?.ToLower() switch
        {
            "json" => new JsonOutput(),
            _ => _console
        };
    }

    public async Task<int> RunKustoQuery()
    {
        var context = new KustoQueryContext();
        var titles = await _xbl.LoadTitles();

        var sources = _settings.KustoQuerySource.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (sources.Length == 0) sources = new[] {DataTable.Titles, DataTable.Achievements, DataTable.Stats};
        foreach (var source in sources)
        {
            switch (source)
            {
                case DataTable.Achievements:
                    var tasks = titles
                        .OrderByDescending(t => t.Achievement.ProgressPercentage)
                        .Select(_xbl.LoadAchievements);
                    var data = await Task.WhenAll(tasks);
                    var achievements = data.SelectMany(a => a);

                    context.WrapDataIntoTable(DataTable.Achievements, achievements.Select(_mapper.Map<KqlAchievement>).ToImmutableArray());
                    break;
                case DataTable.Stats:
                    var tasks1 = titles
                        .OrderByDescending(t => t.Achievement.ProgressPercentage)
                        .Select(_xbl.LoadStats);
                    var data1 = await Task.WhenAll(tasks1);

                    context.WrapDataIntoTable(DataTable.Stats, data1.SelectMany(s => s.Select(m => _mapper.Map<KqlMinutesPlayed>(m))).ToImmutableArray());
                    break;
                case DataTable.Titles:
                    context.WrapDataIntoTable(DataTable.Titles, titles.Select(_mapper.Map<KqlTitle>).ToImmutableArray());
                    break;
            }
        }

        var kql = _settings.KustoQuery;
        if (kql.EndsWith(".kql", StringComparison.InvariantCultureIgnoreCase))
        {
            if (!File.Exists(kql)) return _console.ShowError("KQL file cannot be found");
            kql = await File.ReadAllTextAsync(kql);
        }
        var result = await context.RunQuery(kql);
        if (!string.IsNullOrEmpty(result.Error))
        {
            _console.MarkupLineInterpolated($"[red]Error:[/] [silver]{result.Error}[/]");
            return 1;
        }

        _output.KustoQueryResult(result);
        return 0;
    }
}