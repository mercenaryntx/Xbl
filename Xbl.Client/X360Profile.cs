using System.Diagnostics;
using Xbl.Client.Models;
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

        var result = profile
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

        //var p = profile.ProfileInfo.TitlesPlayed.Where(t => !string.IsNullOrEmpty(t.TitleName) && t.AchievementsUnlocked > 0).Select(t => t.TitleCode).ToHashSet();

        //var result = profile.Games.Values.Select(g =>
        //{
        //    if (p.Contains(g.TitleId)) g.Parse();

        //    var t = new Title
        //    {
        //        TitleId = g.TitleId,
        //        Name = g.Title,
        //        Type = "Game"
        //    };

        //    unchecked
        //    {
        //        if (g.Achievements.Any(a => a.Gamerscore is < byte.MinValue or > byte.MaxValue))
        //        {
        //            //Console.WriteLine(g.Title);
        //        }
        //        else
        //        {
        //            t.Achievement = new AchievementSummary
        //            {
        //                CurrentAchievements = g.UnlockedAchievementCount,
        //                CurrentGamerscore = g.Gamerscore,
        //                ProgressPercentage = g.TotalGamerscore > 0 ? 100 * g.Gamerscore / g.TotalGamerscore : 0,
        //                TotalGamerscore = g.TotalGamerscore,
        //                TotalAchievements = g.AchievementCount
        //            };
        //        }
        //    }

        //    return t;
        //}).ToArray();
        return result;
    }
}