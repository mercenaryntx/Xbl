using Xbl.Client.Models.Xbl.Achievements;

namespace Xbl.Client.Models;

public record RarestAchievementItem(string Title, string Achievement, double Percentage);
public record WeightedAchievementItem(string Title, AchievementSummary Summary, int TotalCount, int AchievedCount, int RareCount, double Weight);
public record MinutesPlayed(string Title, int Minutes);
public record CategorySlice(string Category, int Count);
public record CompletenessItem(string Name, int CurrentGamerscore, int TotalGamerscore, double ProgressPercentage);

public record ProfileSummary(string Name, int Games, int Achievements, int Gamerscore, TimeSpan MinutesPlayed);

public record ProfilesSummary(ProfileSummary[] Profiles, int UniqueTitlesCount);