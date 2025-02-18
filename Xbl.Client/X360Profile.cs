using Xbl.Client.Models;
using Xbl.Xbox360.Extensions;
using Xbl.Xbox360.Io.Stfs;
using Xbl.Xbox360.Models;

namespace Xbl.Client;

public class X360Profile
{
    public static Title[] MapProfileToTitleArray(string profilePath)
    {
        var bytes = File.ReadAllBytes(profilePath);
        var profile = ModelFactory.GetModel<StfsPackage>(bytes);

        profile.ExtractProfile();

        return profile
            .ProfileInfo
            .TitlesPlayed
            .Where(g => !string.IsNullOrEmpty(g.TitleName))
            .Select(g =>
            {
                var t = new Title
                {
                    TitleId = g.TitleCode,
                    Name = g.TitleName,
                    Type = "Game",
                    Achievement = new AchievementSummary
                    {
                        CurrentAchievements = g.AchievementsUnlocked,
                        CurrentGamerscore = g.GamerscoreUnlocked,
                        ProgressPercentage = g.TotalGamerScore > 0 ? 100 * g.GamerscoreUnlocked / g.TotalGamerScore : 0,
                        TotalGamerscore = g.TotalGamerScore,
                        TotalAchievements = g.AchievementCount
                    }
                };

                return t;

            })
            .ToArray();
    }

    public static Achievement[] MapProfileToAchievementArray(string profilePath)
    {
        var bytes = File.ReadAllBytes(profilePath);
        var profile = ModelFactory.GetModel<StfsPackage>(bytes);

        profile.ExtractGames();
        var p = profile.ProfileInfo.TitlesPlayed.Where(t => !string.IsNullOrEmpty(t.TitleName) && t.AchievementsUnlocked > 0).Select(t => t.TitleCode).ToHashSet();

        var first = true;
        return profile.Games.Values.SelectMany(g =>
        {
            if (p.Contains(g.TitleId)) g.Parse();

            unchecked
            {
                var bugReported = false;
                return g.Achievements.Select(a =>
                {
                    try
                    {
                        if (a.Gamerscore < 0 || a.AchievementId < 0) throw new Exception();

                        return new Achievement
                        {
                            Id = a.AchievementId.ToString(),
                            Name = a.Name,
                            TitleAssociations = new[]
                            {
                                new TitleAssociation
                                {
                                    Name = g.Title
                                }
                            },
                            ProgressState = a.IsUnlocked ? "Achieved" : "NotStarted",
                            Progression = new Progression
                            {
                                TimeUnlocked = a.IsUnlocked ? a.UnlockTime : DateTime.MinValue
                            },
                            Platforms = new[] {"Xbox360"},
                            IsSecret = a.IsSecret,
                            Description = a.UnlockedDescription,
                            LockedDescription = a.LockedDescription,
                            ProductId = g.TitleId,
                            AchievementType = "Persistent",
                            ParticipationType = "Individual",
                            Rewards = new[]
                            {
                                new Reward
                                {
                                    Value = a.Gamerscore.ToString(),
                                    Type = "Gamerscore",
                                    ValueType = "Int"
                                }
                            }
                        };
                    }
                    catch
                    {
                        if (!bugReported)
                        {
                            if (first)
                            {
                                Console.WriteLine();
                                first = false;
                            }
                            Console.WriteLine($"{g.Title} is buggy");
                            bugReported = true;
                        }
                        return null;
                    }
                });
            }
        }).Where(a => a != null).ToArray();
    }
}