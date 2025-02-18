namespace Xbl.Client.Models;

public class KqlAchievement
{
    public string Name { get; set; }
    public string TitleId { get; set; }
    public string TitleName { get; set; }
    public string ProgressState { get; set; }
    public DateTime TimeUnlocked { get; set; }
    public string Platform { get; set; }
    public bool IsSecret { get; set; }
    public string Description { get; set; }
    public string LockedDescription { get; set; }
    public int Gamerscore { get; set; }
    public bool IsRare { get; set; }
    public double RarityPercentage { get; set; }
}