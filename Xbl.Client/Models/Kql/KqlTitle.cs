namespace Xbl.Client.Models.Kql;

public class KqlTitle
{
    public string TitleId { get; set; }
    public string Name { get; set; }
    public string CompatibleWith { get; set; }
    public int CurrentAchievements { get; set; }
    public int TotalAchievements { get; set; }
    public int CurrentGamerscore { get; set; }
    public int TotalGamerscore { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime LastTimePlayed { get; set; }
    public string Category { get; set; }

    public string Xbox360ProductId { get; set; }
    public string XboxOneProductId { get; set; }
    public string XboxSeriesProductId { get; set; }

    public DateTime? Xbox360ReleaseDate { get; set; }
    public DateTime? XboxOneReleaseDate { get; set; }
    public DateTime? XboxSeriesReleaseDate { get; set; }
}