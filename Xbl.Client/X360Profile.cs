using Xbl.Models;
using Xbl.Xbox360.Io.Stfs;
using Xbl.Xbox360.Models;

namespace Xbl;

public class X360Profile
{
    public static Title[] MapProfileToTitleArray(string profilePath)
    {
        var profile = ModelFactory.GetModel<StfsPackage>(File.ReadAllBytes(profilePath));
        profile.ExtractContent();

        return profile.Games.Values.Select(g =>
        {
            g.Parse();

            var t = new Title
            {
                TitleId = g.TitleId,
                Name = g.Title,
                Type = "Game"
            };

            unchecked
            {
                if (g.Achievements.Any(a => a.Gamerscore is < byte.MinValue or > byte.MaxValue))
                {
                    //Console.WriteLine(g.Title);
                }
                else
                {
                    t.Achievement = new AchievementSummary
                    {
                        CurrentAchievements = g.UnlockedAchievementCount,
                        CurrentGamerscore = g.Gamerscore,
                        ProgressPercentage = g.TotalGamerscore > 0 ? 100 * g.Gamerscore / g.TotalGamerscore : 0,
                        TotalGamerscore = g.TotalGamerscore,
                        TotalAchievements = g.AchievementCount
                    };
                }
            }

            return t;
        }).ToArray();
    }
}