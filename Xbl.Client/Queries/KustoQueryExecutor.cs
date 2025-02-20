using System.Collections.Immutable;
using AutoMapper;
using KustoLoco.Core;
using Xbl.Client.Io;
using Xbl.Client.Models;

namespace Xbl.Client.Queries;

public class KustoQueryExecutor : IKustoQueryExecutor
{
    private readonly Settings _settings;
    private readonly IJsonRepository _repository;
    private readonly IConsole _console;
    private readonly IMapper _mapper;
    private readonly IOutput _output;

    public KustoQueryExecutor(Settings settings, IJsonRepository repository, IConsole console, IMapper mapper)
    {
        _settings = settings;
        _repository = repository;
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
        var titles = await _repository.LoadTitles();

        var sources = _settings.KustoQuerySource.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var source in sources)
        {
            switch (source)
            {
                case Constants.Achievements:
                    var tasks = titles
                        .OrderByDescending(t => t.Achievement.ProgressPercentage)
                        .Select(_repository.LoadAchievements);
                    var data = await Task.WhenAll(tasks);
                    var achievements = data.SelectMany(a => a);

                    context.WrapDataIntoTable(Constants.Achievements, achievements.Select(_mapper.Map<KqlAchievement>).ToImmutableArray());
                    break;
                case Constants.Stats:
                    var tasks1 = titles
                        .OrderByDescending(t => t.Achievement.ProgressPercentage)
                        .Select(_repository.LoadStats);
                    var data1 = await Task.WhenAll(tasks1);

                    context.WrapDataIntoTable(Constants.Stats, data1.SelectMany(s => s.Select(m => _mapper.Map<KqlMinutesPlayed>(m))).ToImmutableArray());
                    break;
                default:
                    context.WrapDataIntoTable(Constants.Titles, titles.Select(_mapper.Map<KqlTitle>).ToImmutableArray());
                    break;
            }
        }

        var kql = _settings.KustoQuery;
        if (kql.EndsWith(".kql", StringComparison.InvariantCultureIgnoreCase))
        {
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