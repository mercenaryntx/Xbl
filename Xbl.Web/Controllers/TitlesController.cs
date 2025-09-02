using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Xbl.Client;
using Xbl.Client.Io;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Data;
using Xbl.Web.Models;

namespace Xbl.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TitlesController : ControllerBase
{
    private readonly IDatabaseContext _x360;
    private readonly IMapper _mapper;
    private readonly IXblClient _xbl;
    private readonly ILogger<TitlesController> _logger;
    private readonly IDatabaseContext _live;

    private const string LastUpdateHeader = "X-Titles-Last-Update";
    private const string TitleSelector = """
                                         SELECT 
                                           json_extract(Data, '$.titleId') as TitleId,
                                           json_extract(Data, '$.name') AS Name, 
                                           json_extract(Data, '$.displayImage') AS DisplayImage, 
                                           json_extract(Data, '$.achievement.currentAchievements') AS CurrentAchievements, 
                                           json_extract(Data, '$.achievement.totalAchievements') AS TotalAchievements, 
                                           json_extract(Data, '$.achievement.currentGamerscore') AS CurrentGamerscore, 
                                           json_extract(Data, '$.achievement.totalGamerscore') AS TotalGamerscore, 
                                           json_extract(Data, '$.achievement.progressPercentage') AS ProgressPercentage, 
                                           json_extract(Data, '$.titleHistory.lastTimePlayed') AS LastTimePlayed
                                         FROM title
                                         """;

    public TitlesController(
        [FromKeyedServices(DataSource.Live)] IDatabaseContext live, 
        [FromKeyedServices(DataSource.Xbox360)] IDatabaseContext x360, 
        IMapper mapper,
        IXblClient xbl,
        ILogger<TitlesController> logger)
    {
        _mapper = mapper;
        _xbl = xbl;
        _logger = logger;
        _live = live.Mandatory();
        _x360 = x360.Mandatory();
    }

    [HttpGet("{source}")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = ["title", "orderBy", "orderDir", "page"], VaryByHeader = LastUpdateHeader)]
    public async Task<IEnumerable<Title>> Get(
        string source,
        [FromQuery] string title = "", 
        [FromQuery] string orderBy = "lastPlayed", 
        [FromQuery] string orderDir = "DESC", 
        [FromQuery] int page = 0)
    {
        var where = "WHERE json_extract(Data, '$.achievement.totalGamerscore') > 0";
        if (!string.IsNullOrEmpty(title)) where += " AND json_extract(Data, '$.name') LIKE @Title";
        var query = $"{TitleSelector} {where} ORDER BY json_extract(Data, @OrderBy) {orderDir} LIMIT @Limit OFFSET @Offset";
        const int limit = 50;
        orderBy = orderBy switch
        {
            "name" => "$.name",
            "lastPlayed" => "$.titleHistory.lastTimePlayed",
            "progress" => "$.achievement.progressPercentage",
            _ => "$.titleHistory.lastTimePlayed"
        };
        var p = new {Limit = limit, Offset = page * limit, OrderBy = orderBy, Title = $"%{title}%"};
        return source switch
        {
            "live" => await _live.Query<Title>(query, p),
            "x360" => await _x360.Query<Title>(query, p),
            _ => []
        };
    }

    [HttpGet("{source}/{titleId}")]
    [ResponseCache(Duration = 3600, VaryByHeader = LastUpdateHeader)]
    public async Task<TitleDetail> Get(string source, int titleId)
    {
        var repo = source switch
        {
            "live" => _live,
            "x360" => _x360,
            _ => throw new InvalidOperationException()
        };

        var achievements = await repo.GetRepository<Client.Models.Xbl.Achievements.Achievement>();

        var tt = repo.Query<TitleDetail>($"{TitleSelector} WHERE Id = {titleId}");
        var at = achievements.GetPartition(titleId);

        await Task.WhenAll(tt, at);

        var t = tt.Result.First();
        var a = at.Result;

        t.Achievements = _mapper.Map<Achievement[]>(a.OrderByDescending(aa => aa.TimeUnlocked));

        if (source == "live")
        {
            var stats = await repo.GetRepository<Stat>();
            var s = await stats.Get(titleId);
            t.Minutes = s?.IntValue;
        }
        return t;
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update()
    {
        await _xbl.Update();
        var lastUpdate = await _live.Query<DateTime>("SELECT json_extract(Data, '$.titleHistory.lastTimePlayed') FROM title ORDER BY json_extract(Data, '$.titleHistory.lastTimePlayed') DESC LIMIT 1");
        Response.Headers[LastUpdateHeader] = lastUpdate.Single().ToString("o");
        return NoContent();
    }

    [HttpOptions]
    public async Task<IActionResult> Options()
    {
        var lastUpdate = await _live.Query<DateTime>("SELECT json_extract(Data, '$.titleHistory.lastTimePlayed') FROM title ORDER BY json_extract(Data, '$.titleHistory.lastTimePlayed') DESC LIMIT 1");
        Response.Headers[LastUpdateHeader] = lastUpdate.Single().ToString("o");
        return NoContent();
    }
}