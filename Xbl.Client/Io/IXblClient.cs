namespace Xbl.Client.Io;

public interface IXblClient
{
    Task<UpdateResult> Update();
}

public record UpdateResult
{
    public int TitlesInserted { get; init; }
    public int TitlesUpdated { get; init; }
    public int AchievementsInserted { get; init; }
    public int AchievementsUpdated { get; init; }
    public int StatsInserted { get; init; }
    public int StatsUpdated { get; init; }

    public int TotalChanges => TitlesInserted + TitlesUpdated + AchievementsInserted + AchievementsUpdated + StatsInserted + StatsUpdated;
}