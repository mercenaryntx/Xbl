namespace Xbl.Client.Models;

public class KqlTitle
{
    public string TitleId { get; set; }
    public string Name { get; set; }
    public string Devices { get; set; }
    public int CurrentAchievements { get; set; }
    public int TotalAchievements { get; set; }
    public int CurrentGamerscore { get; set; }
    public int TotalGamerscore { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime LastTimePlayed { get; set; }
}