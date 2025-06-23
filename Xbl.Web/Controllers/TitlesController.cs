using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Xbl.Client;
using Xbl.Client.Models.Kql;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Data;
using Xbl.Web.Models;

namespace Xbl.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TitlesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<TitlesController> _logger;
    private readonly IDatabaseContext _live;

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

    public TitlesController([FromKeyedServices(DataSource.Live)] IDatabaseContext live, IMapper mapper, ILogger<TitlesController> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _live = live.Mandatory();
    }

    [HttpGet]
    public async Task<IEnumerable<Title>> Get([FromQuery] string orderBy = "lastPlayed", [FromQuery] string orderDir = "DESC", [FromQuery] int page = 0)
    {
        var query = $"{TitleSelector} ORDER BY json_extract(Data, @OrderBy) {orderDir} LIMIT @Limit OFFSET @Offset";
        const int limit = 50;
        orderBy = orderBy switch
        {
            "name" => "$.name",
            "lastPlayed" => "$.titleHistory.lastTimePlayed",
            "progress" => "$.achievement.progressPercentage",
            _ => "$.titleHistory.lastTimePlayed"
        };
        return await _live.Query<Title>(query, new { Limit = limit, Offset = page * limit, OrderBy = orderBy });
    }

    [HttpGet("{titleId}")]
    public async Task<TitleDetail> Get(int titleId)
    {
        var achievements = await _live.GetRepository<Client.Models.Xbl.Achievements.Achievement>();
        var stats = await _live.GetRepository<Stat>();

        var tt = _live.Query<TitleDetail>($"{TitleSelector} WHERE Id = {titleId}");
        var at = achievements.GetPartition(titleId);
        var st = stats.Get(titleId);

        await Task.WhenAll(tt, at, st);

        var t = tt.Result.First();
        var a = at.Result;
        var s = st.Result;

        t.Achievements = _mapper.Map<Achievement[]>(a.OrderByDescending(aa => aa.TimeUnlocked));
        t.Minutes = s.IntValue;
        return t;
    }
}