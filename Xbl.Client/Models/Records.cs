namespace Xbl.Client.Models;

public record Records(string Title, string Achievement, double Percentage);
public record WeightedAchievementItem(string Title, AchievementSummary Summary, int TotalCount, int AchievedCount, int RareCount, double Weight);
public record MinutesPlayed(string Title, int Minutes);