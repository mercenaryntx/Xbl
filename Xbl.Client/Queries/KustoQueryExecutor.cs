﻿using System.Collections.Immutable;
using AutoMapper;
using KustoLoco.Core;
using Microsoft.Extensions.DependencyInjection;
using Xbl.Client.Io;
using Xbl.Client.Models.Kql;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Data;
using Xbl.Data.Entities;

namespace Xbl.Client.Queries;

public class KustoQueryExecutor : IKustoQueryExecutor
{
    private readonly Settings _settings;
    private readonly IConsole _console;
    private readonly IMapper _mapper;
    private readonly IDatabaseContext _live;
    private readonly IDatabaseContext _x360;

    public KustoQueryExecutor(
        Settings settings, 
        IConsole console, 
        IMapper mapper, 
        [FromKeyedServices(DataSource.Live)] IDatabaseContext live, 
        [FromKeyedServices(DataSource.Xbox360)] IDatabaseContext x360)
    {
        _settings = settings;
        _console = console;
        _mapper = mapper;
        _live = live;
        _x360 = x360;
    }

    public async Task<KustoQueryResult> RunKustoQuery()
    {
        var context = new KustoQueryContext();

        var liveTitles = await GetAll<Title>(_live);
        var x360Titles = await GetAll<Title>(_x360);

        var titles = liveTitles.Concat(x360Titles).Select(_mapper.Map<KqlTitle>).ToImmutableArray();
        context.WrapDataIntoTable(DataTable.Titles, titles);

        var liveAchievements = await GetAll<Achievement>(_live);
        var x360Achievements = await GetAll<Achievement>(_x360);

        var achievements = liveAchievements.Concat(x360Achievements).Select(_mapper.Map<KqlAchievement>).ToImmutableArray();
        context.WrapDataIntoTable(DataTable.Achievements, achievements);

        var liveStats = await GetAll<Stat>(_live);
        var stats = liveStats.Select(_mapper.Map<KqlMinutesPlayed>).ToImmutableArray();
        context.WrapDataIntoTable(DataTable.Stats, stats);

        var kql = _settings.KustoQuery;
        if (kql.EndsWith(".kql", StringComparison.InvariantCultureIgnoreCase))
        {
            if (!File.Exists(kql))
            {
                _console.ShowError("KQL file cannot be found");
                return null;
            }
            kql = await File.ReadAllTextAsync(kql);
        }
        return await context.RunQuery(kql);
    }

    private static async Task<IEnumerable<T>> GetAll<T>(IDatabaseContext db) where T : class, IHaveId
    {
        var repo = await db.GetRepository<T>();
        return await repo.GetAll();
    }
}