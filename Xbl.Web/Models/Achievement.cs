namespace Xbl.Web.Models;

public class Achievement
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime TimeUnlocked { get; set; }
    public bool IsSecret { get; set; }
    public string Description { get; set; }
    public string LockedDescription { get; set; }
    public int Gamerscore { get; set; }
    public bool IsRare { get; set; }
    public double RarityPercentage { get; set; }
    public string DisplayImage { get; set; }
}