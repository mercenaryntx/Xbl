namespace Xbl.Client.Models.Xbl;

public record RarestAchievementItem(string Title, string Achievement, double Percentage);
public record WeightedAchievementItem(string Title, AchievementSummary Summary, int TotalCount, int AchievedCount, int RareCount, double Weight);
public record MinutesPlayed(string Title, int Minutes);
public record CategorySlice(string Category, int Count);